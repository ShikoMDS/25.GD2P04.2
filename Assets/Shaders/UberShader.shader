Shader "Custom/UberShaderEnhanced"
{
	Properties
	{
		// =============================================================================
		// BASE MATERIAL PROPERTIES
		// =============================================================================
		[Header(Base Properties)]
		_BaseMap("Base Map", 2D) = "white" {}
		_BaseColor("Base Color", Color) = (1,1,1,1)
		_Metallic("Metallic", Range(0,1)) = 0
		_Smoothness("Smoothness", Range(0,1)) = 0.5

		// =============================================================================
		// TRANSPARENCY & BLENDING
		// =============================================================================
		[Header(Transparency)]
		[Enum(Opaque,0,Transparent,1,Cutout,2)] _BlendMode("Blend Mode", Float) = 0
		_Alpha("Alpha", Range(0,1)) = 1.0
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.5

		// Hidden blend state properties - set by custom editor
		[HideInInspector] _SrcBlend("Src Blend", Float) = 1.0
		[HideInInspector] _DstBlend("Dst Blend", Float) = 0.0
		[HideInInspector] _ZWrite("Z Write", Float) = 1.0

		// =============================================================================
		// SHADOW SYSTEM
		// =============================================================================
		[Header(Shadow Settings)]
		[Toggle(_RECEIVE_SHADOWS)] _ReceiveShadows("Receive Shadows", Float) = 1
		_ShadowStrength("Shadow Strength", Range(0,2)) = 1.0
		_ShadowColor("Shadow Tint", Color) = (0.5,0.5,0.7,1)
		_AmbientDarkening("Ambient Darkening", Range(0,1)) = 0.3
		[Toggle(_ENHANCED_SHADOWS)] _EnhancedShadows("Enhanced Shadow Quality", Float) = 0

		// =============================================================================
		// LIGHTING RESPONSE
		// =============================================================================
		[Header(Light Response)]
		_LightSensitivity("Light Sensitivity", Range(0,2)) = 1.0
		_DarkeningThreshold("Darkening Threshold", Range(0,1)) = 0.1
		_MaxDarkening("Max Darkening", Range(0,1)) = 0.8

		// =============================================================================
		// SURFACE DETAIL MAPS
		// =============================================================================
		[Header(Detail Maps)]
		[Toggle(_USE_METALLICMAP)] _EnableMetallic("Enable Metallic Map", Float) = 0
		_MetallicMap("Metallic Map (R=Metallic)", 2D) = "white" {}

		[Toggle(_USE_ROUGHNESSMAP)] _EnableRoughness("Enable Roughness Map", Float) = 0
		_RoughnessMap("Roughness Map (R=Roughness)", 2D) = "white" {}

		[Toggle(_USE_NORMALMAP)] _EnableNormal("Enable Normal Map", Float) = 0
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Range(0,2)) = 1

		_AOMap("Ambient Occlusion Map", 2D) = "white" {}
		_AOStrength("AO Strength", Range(0,1)) = 1

		// =============================================================================
		// EMISSION SYSTEM
		// =============================================================================
		[Header(Emission)]
		[Toggle(_USE_EMISSION)] _EnableEmission("Enable Emission", Float) = 0
		_EmissiveMap("Emissive Map", 2D) = "black" {}
		_EmissiveColor("Emissive Color", Color) = (1,1,1,1)
		_EmissiveStrength("Emission Strength", Range(0,10)) = 1
		_PulseSpeed("Pulse Speed", Range(0,10)) = 0
		_PulseAmplitude("Pulse Amplitude", Range(0,1)) = 0.5

		[Toggle(_FORCE_UNLIT_EMISSION)] _UnlitEmission("Emission Overrides Lighting", Float) = 0

		// =============================================================================
		// RENDERING SETTINGS
		// =============================================================================
		[Header(Rendering)]
		[Enum(Back,2,Front,1,Off,0)] _CullMode("Cull Mode", Float) = 2

		// =============================================================================
		// ADDITIONAL EFFECTS
		// =============================================================================
		[Header(Detail Texturing)]
		[Toggle(_USE_DETAIL)] _EnableDetail("Enable Detail Map", Float) = 0
		_DetailMap("Detail Map", 2D) = "gray" {}
		_DetailStrength("Detail Strength", Range(0,1)) = 0.5

		[Header(Fresnel Effect)]
		[Toggle(_USE_FRESNEL)] _EnableFresnel("Enable Fresnel", Float) = 0
		_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		_FresnelPower("Fresnel Power", Range(0,10)) = 2
		_FresnelStrength("Fresnel Strength", Range(0,5)) = 1

		[Header(Scrolling UV)]
		[Toggle(_USE_UV_SCROLL)] _EnableUVScroll("Enable UV Scrolling", Float) = 0
		_ScrollSpeedX("Scroll Speed X", Range(-5,5)) = 0
		_ScrollSpeedY("Scroll Speed Y", Range(-5,5)) = 0
	}

	SubShader
	{
		LOD 200

		// =============================================================================
		// FORWARD LIGHTING PASS
		// =============================================================================
		Pass
		{
			Name "ForwardLit"
			Tags
			{
				"LightMode" = "UniversalForward"
			}

			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull [_CullMode]

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// -------------------------------------------------------------------------
			// SHADER FEATURE TOGGLES
			// -------------------------------------------------------------------------
			#pragma shader_feature_local _USE_NORMALMAP
			#pragma shader_feature_local _USE_EMISSION
			#pragma shader_feature_local _USE_METALLICMAP
			#pragma shader_feature_local _USE_ROUGHNESSMAP
			#pragma shader_feature_local _USE_DETAIL
			#pragma shader_feature_local _USE_FRESNEL
			#pragma shader_feature_local _USE_UV_SCROLL
			#pragma shader_feature_local _FORCE_UNLIT_EMISSION
			#pragma shader_feature_local _ALPHATEST_ON
			#pragma shader_feature_local _ALPHABLEND_ON
			#pragma shader_feature_local _RECEIVE_SHADOWS
			#pragma shader_feature_local _ENHANCED_SHADOWS

			// -------------------------------------------------------------------------
			// UNITY LIGHTING MULTI-COMPILE VARIANTS
			// -------------------------------------------------------------------------
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _LIGHT_LAYERS
			#pragma multi_compile_fragment _ _LIGHT_COOKIES
			#pragma multi_compile _ _CLUSTERED_RENDERING

			// -------------------------------------------------------------------------
			// UNITY URP INCLUDES
			// -------------------------------------------------------------------------
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			// -------------------------------------------------------------------------
			// VERTEX INPUT STRUCTURE
			// -------------------------------------------------------------------------
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			// -------------------------------------------------------------------------
			// VERTEX TO FRAGMENT STRUCTURE
			// -------------------------------------------------------------------------
			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float3 viewDirWS : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
				#if _USE_NORMALMAP
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD7;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// -------------------------------------------------------------------------
			// TEXTURE DECLARATIONS
			// -------------------------------------------------------------------------
			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);
			TEXTURE2D(_NormalMap);
			SAMPLER(sampler_NormalMap);
			TEXTURE2D(_EmissiveMap);
			SAMPLER(sampler_EmissiveMap);
			TEXTURE2D(_AOMap);
			SAMPLER(sampler_AOMap);
			TEXTURE2D(_MetallicMap);
			SAMPLER(sampler_MetallicMap);
			TEXTURE2D(_RoughnessMap);
			SAMPLER(sampler_RoughnessMap);
			TEXTURE2D(_DetailMap);
			SAMPLER(sampler_DetailMap);

			// -------------------------------------------------------------------------
			// MATERIAL PROPERTIES BUFFER
			// -------------------------------------------------------------------------
			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float4 _EmissiveColor;
				float4 _FresnelColor;
				float4 _ShadowColor;
				float _Metallic;
				float _Smoothness;
				float _Alpha;
				float _Cutoff;
				float _EmissiveStrength;
				float _PulseSpeed;
				float _PulseAmplitude;
				float _NormalStrength;
				float _AOStrength;
				float _DetailStrength;
				float _FresnelPower;
				float _FresnelStrength;
				float _ScrollSpeedX;
				float _ScrollSpeedY;
				float _ShadowStrength;
				float _AmbientDarkening;
				float _LightSensitivity;
				float _DarkeningThreshold;
				float _MaxDarkening;
				float4 _BaseMap_ST;
				float4 _DetailMap_ST;
			CBUFFER_END

			// -------------------------------------------------------------------------
			// VERTEX SHADER
			// -------------------------------------------------------------------------
			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				// Transform positions and normals
				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
				VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

				// Set output values
				OUT.positionHCS = positionInputs.positionCS;
				OUT.worldPos = positionInputs.positionWS;
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
				OUT.uv2 = IN.uv2;
				OUT.normalWS = normalInputs.normalWS;
				OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

				// Normal mapping data
				#if _USE_NORMALMAP
                OUT.tangentWS = normalInputs.tangentWS;
                OUT.bitangentWS = normalInputs.bitangentWS;
				#endif

				// Shadow coordinates
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    OUT.shadowCoord = GetShadowCoord(positionInputs);
				#endif

				return OUT;
			}

			// -------------------------------------------------------------------------
			// FRAGMENT SHADER
			// -------------------------------------------------------------------------
			half4 frag(Varyings IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				// ---------------------------------------------------------------------
				// UV SCROLLING
				// ---------------------------------------------------------------------
				float2 scrolledUV = IN.uv;
				#if _USE_UV_SCROLL
                scrolledUV += float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;
				#endif

				// ---------------------------------------------------------------------
				// BASE ALBEDO AND ALPHA
				// ---------------------------------------------------------------------
				float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, scrolledUV);
				float3 albedo = baseSample.rgb * _BaseColor.rgb;
				float alpha = baseSample.a * _BaseColor.a * _Alpha;

				// Alpha testing for cutout materials
				#if _ALPHATEST_ON
                clip(alpha - _Cutoff);
				#endif

				// ---------------------------------------------------------------------
				// NORMAL MAPPING
				// ---------------------------------------------------------------------
				float3 normalWS = normalize(IN.normalWS);
				#if _USE_NORMALMAP
                float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, scrolledUV);
                float3 tangentNormal = UnpackNormalScale(normalSample, _NormalStrength);
                float3x3 TBN = float3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalWS);
                normalWS = normalize(mul(tangentNormal, TBN));
				#endif

				// ---------------------------------------------------------------------
				// SURFACE PROPERTIES
				// ---------------------------------------------------------------------
				float metallic = _Metallic;
				float smoothness = _Smoothness;

				// Metallic map sampling
				#if _USE_METALLICMAP
                float metallicSample = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, scrolledUV).r;
                metallic *= metallicSample;
				#endif

				// Roughness map sampling (inverted to smoothness)
				#if _USE_ROUGHNESSMAP
                float roughnessSample = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, scrolledUV).r;
                smoothness *= (1.0 - roughnessSample);
				#endif

				// Ambient occlusion
				float ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, IN.uv2).r;
				ao = lerp(1.0, ao, _AOStrength);

				// ---------------------------------------------------------------------
				// DETAIL TEXTURING
				// ---------------------------------------------------------------------
				#if _USE_DETAIL
                float2 detailUV = TRANSFORM_TEX(IN.uv, _DetailMap);
                float3 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV).rgb;
                detail = lerp(float3(0.5, 0.5, 0.5), detail, _DetailStrength);
                albedo = albedo * detail * 2.0;
				#endif

				float3 viewDirWS = normalize(IN.viewDirWS);
				float3 lighting = 0;

				// ---------------------------------------------------------------------
				// LIGHTING CALCULATIONS
				// ---------------------------------------------------------------------
				#if !_FORCE_UNLIT_EMISSION
				// Setup input data for Unity's PBR lighting
				InputData inputData = (InputData)0;
				inputData.positionWS = IN.worldPos;
				inputData.normalWS = normalWS;
				inputData.viewDirectionWS = viewDirWS;
				inputData.fogCoord = 0;
				inputData.vertexLighting = 0;
				inputData.bakedGI = SampleSH(normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.uv2);

				// Shadow coordinate setup
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    inputData.shadowCoord = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
				#else
				inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				// Setup surface data for Unity's PBR lighting
				SurfaceData surfaceData = (SurfaceData)0;
				surfaceData.albedo = albedo;
				surfaceData.metallic = metallic;
				surfaceData.specular = 0;
				surfaceData.smoothness = smoothness;
				surfaceData.normalTS = float3(0, 0, 1);
				surfaceData.emission = 0;
				surfaceData.occlusion = ao;
				surfaceData.alpha = alpha;
				surfaceData.clearCoatMask = 0;
				surfaceData.clearCoatSmoothness = 0;

				// Calculate PBR lighting
				lighting = UniversalFragmentPBR(inputData, surfaceData).rgb;

				// ---------------------------------------------------------------------
				// CUSTOM SHADOW PROCESSING
				// ---------------------------------------------------------------------
				Light mainLight = GetMainLight(inputData.shadowCoord);

				#if _RECEIVE_SHADOWS
                float shadowAttenuation = mainLight.shadowAttenuation;
                
                // Apply shadow strength
                shadowAttenuation = lerp(1.0 - _ShadowStrength, 1.0, shadowAttenuation);
                
                // Apply shadow color tinting
                float3 shadowTint = lerp(_ShadowColor.rgb, float3(1,1,1), shadowAttenuation);
                lighting *= shadowTint;
                
                // Enhanced shadow quality with multi-sampling
				#if _ENHANCED_SHADOWS
                float enhancedShadow = 0.0;
                float2 shadowTexelSize = GetMainLightShadowParams().zw;
                float2 offsets[4] = {
                    float2(-0.5, -0.5),
                    float2(0.5, -0.5),
                    float2(-0.5, 0.5),
                    float2(0.5, 0.5)
                };
                
                // Sample shadow map at multiple offsets
                for(int i = 0; i < 4; i++)
                {
                    float4 offsetCoord = inputData.shadowCoord;
                    offsetCoord.xy += offsets[i] * shadowTexelSize;
                    Light offsetLight = GetMainLight(offsetCoord);
                    enhancedShadow += offsetLight.shadowAttenuation;
                }
                enhancedShadow *= 0.25;
                
                // Blend enhanced shadows with regular shadows
                shadowAttenuation = lerp(shadowAttenuation, enhancedShadow, 0.5);
                shadowAttenuation = lerp(1.0 - _ShadowStrength, 1.0, shadowAttenuation);
                lighting *= lerp(1.0, shadowAttenuation, 0.5);
				#endif
				#endif

				// ---------------------------------------------------------------------
				// CUSTOM LIGHT RESPONSE
				// ---------------------------------------------------------------------
				float lightContribution = dot(normalWS, mainLight.direction) * 0.5 + 0.5;
				lightContribution = saturate(lightContribution * _LightSensitivity);

				// Apply additional darkening in low light conditions
				if (lightContribution < _DarkeningThreshold)
				{
					float darkeningFactor = 1.0 - (lightContribution / max(_DarkeningThreshold, 0.01));
					float darkening = lerp(1.0, 1.0 - _MaxDarkening, darkeningFactor);
					lighting *= darkening;
				}
				#endif

				// ---------------------------------------------------------------------
				// EMISSION SYSTEM
				// ---------------------------------------------------------------------
				float3 emission = 0;
				#if _USE_EMISSION
                // Pulsing emission effect
                float pulse = 1.0;
                if (_PulseSpeed > 0)
                {
                    pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmplitude;
                }
                
                // Sample emission texture and apply properties
                float3 emissiveTex = SAMPLE_TEXTURE2D(_EmissiveMap, sampler_EmissiveMap, scrolledUV).rgb;
                emission = emissiveTex * _EmissiveColor.rgb * _EmissiveStrength * pulse;
				#endif

				// ---------------------------------------------------------------------
				// FRESNEL EFFECT
				// ---------------------------------------------------------------------
				#if _USE_FRESNEL
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                float3 fresnelContribution = fresnel * _FresnelColor.rgb * _FresnelStrength;
                emission += fresnelContribution;
				#endif

				// ---------------------------------------------------------------------
				// FINAL OUTPUT
				// ---------------------------------------------------------------------
				#if _FORCE_UNLIT_EMISSION
                // Emission only mode - ignore lighting
                return float4(emission, alpha);
				#else
				// Standard mode - combine lighting and emission
				return float4(lighting + emission, alpha);
				#endif
			}
			ENDHLSL
		}

		// =============================================================================
		// SHADOW CASTER PASS
		// =============================================================================
		Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull [_CullMode]

			HLSLPROGRAM
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			#pragma shader_feature_local _ALPHATEST_ON
			#pragma multi_compile_instancing
			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			// -------------------------------------------------------------------------
			// SHADOW PASS GLOBALS
			// -------------------------------------------------------------------------
			float3 _LightDirection;
			float3 _LightPosition;

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float _Alpha;
				float _Cutoff;
				float4 _BaseMap_ST;
			CBUFFER_END

			// -------------------------------------------------------------------------
			// SHADOW PASS STRUCTURES
			// -------------------------------------------------------------------------
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// -------------------------------------------------------------------------
			// SHADOW POSITION CALCULATION
			// -------------------------------------------------------------------------
			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
				float3 lightDirectionWS = _LightDirection;
				#endif

				// Apply shadow bias
				positionWS = positionWS + lightDirectionWS * _ShadowBias.x + normalWS * _ShadowBias.y;

				float4 positionCS = TransformWorldToHClip(positionWS);

				// Clamp to near plane
				#if UNITY_REVERSED_Z
				positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}

			// -------------------------------------------------------------------------
			// SHADOW VERTEX SHADER
			// -------------------------------------------------------------------------
			Varyings ShadowPassVertex(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = GetShadowPositionHClip(input);
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				return output;
			}

			// -------------------------------------------------------------------------
			// SHADOW FRAGMENT SHADER
			// -------------------------------------------------------------------------
			half ShadowPassFragment(Varyings input) : SV_TARGET
			{
				// Alpha testing for cutout shadows
				#if _ALPHATEST_ON
                    float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                    float alpha = baseSample.a * _BaseColor.a * _Alpha;
                    clip(alpha - _Cutoff);
				#endif
				return 0;
			}
			ENDHLSL
		}

		// =============================================================================
		// DEPTH ONLY PASS
		// =============================================================================
		Pass
		{
			Name "DepthOnly"
			Tags
			{
				"LightMode" = "DepthOnly"
			}

			ZWrite On
			ColorMask 0
			Cull [_CullMode]

			HLSLPROGRAM
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			#pragma shader_feature_local _ALPHATEST_ON

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float _Alpha;
				float _Cutoff;
				float4 _BaseMap_ST;
			CBUFFER_END

			// -------------------------------------------------------------------------
			// DEPTH PASS STRUCTURES
			// -------------------------------------------------------------------------
			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// -------------------------------------------------------------------------
			// DEPTH VERTEX SHADER
			// -------------------------------------------------------------------------
			Varyings DepthOnlyVertex(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				return output;
			}

			// -------------------------------------------------------------------------
			// DEPTH FRAGMENT SHADER
			// -------------------------------------------------------------------------
			half DepthOnlyFragment(Varyings input) : SV_TARGET
			{
				// Alpha testing for cutout depth
				#if _ALPHATEST_ON
                    float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                    float alpha = baseSample.a * _BaseColor.a * _Alpha;
                    clip(alpha - _Cutoff);
				#endif
				return 0;
			}
			ENDHLSL
		}
	}

	// =============================================================================
	// SHADER METADATA
	// =============================================================================
	CustomEditor "UberShaderGUI"
	FallBack "Hidden/InternalErrorShader"
}