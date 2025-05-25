Shader "Custom/UberShader"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)

        [Toggle(_USE_NORMALMAP)] _EnableNormal("Enable Normal Map", Float) = 0
        _NormalMap("Normal Map", 2D) = "bump" {}

        [Toggle(_USE_EMISSION)] _EnableEmission("Enable Emission", Float) = 0
        _EmissiveMap("Emissive Map", 2D) = "black" {}
        _EmissiveColor("Emissive Color", Color) = (1,1,1,1)
        _EmissiveStrength("Emission Strength", Float) = 1
        _PulseSpeed("Pulse Speed", Float) = 0

        [Toggle(_FORCE_UNLIT_EMISSION)] _UnlitEmission("Emission Overrides Lighting", Float) = 0

        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _AOMap("Ambient Occlusion Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _USE_NORMALMAP
            #pragma multi_compile _ _USE_EMISSION
            #pragma multi_compile _ _FORCE_UNLIT_EMISSION
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
#if _USE_NORMALMAP
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
#endif
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_EmissiveMap); SAMPLER(sampler_EmissiveMap);
            TEXTURE2D(_AOMap); SAMPLER(sampler_AOMap);

            float4 _BaseColor;
            float4 _EmissiveColor;
            float _EmissiveStrength;
            float _PulseSpeed;
            float _Cutoff;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = IN.uv;
                OUT.normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                OUT.shadowCoord = TransformWorldToShadowCoord(worldPos);

#if _USE_NORMALMAP
                float3 normal = normalize(IN.normalOS);
                float3 tangent = normalize(IN.tangentOS.xyz);
                float3 bitangent = cross(normal, tangent) * IN.tangentOS.w;
                OUT.tangentWS = normalize(TransformObjectToWorldDir(tangent));
                OUT.bitangentWS = normalize(TransformObjectToWorldDir(bitangent));
#endif
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 albedo = baseSample.rgb * _BaseColor.rgb;
                clip(baseSample.a - _Cutoff);

                float3 normalWS = normalize(IN.normalWS);
#if _USE_NORMALMAP
                float3 tangentNormal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                float3x3 TBN = float3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS);
                normalWS = normalize(mul(tangentNormal, TBN));
#endif

                float ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, IN.uv).r;
                float3 viewDir = normalize(IN.viewDirWS);

                float3 lighting = 0;

#if !_FORCE_UNLIT_EMISSION
                // Only calculate lighting if not overridden
                Light light = GetMainLight(IN.shadowCoord);
                float3 lightDir = normalize(light.direction);
                float NdotL = saturate(dot(normalWS, -lightDir));
                float shadow = light.shadowAttenuation;
                lighting = albedo * NdotL * light.color.rgb * shadow * ao;
#endif

                float3 emission = 0;
#if _USE_EMISSION
                float pulse = 1 + sin(_Time.y * _PulseSpeed);
                float3 emissiveTex = SAMPLE_TEXTURE2D(_EmissiveMap, sampler_EmissiveMap, IN.uv).rgb;
                emission = emissiveTex * _EmissiveColor.rgb * _EmissiveStrength * pulse;
#endif

#if _FORCE_UNLIT_EMISSION
                return float4(emission, 1.0); // ignore lighting
#else
                return float4(lighting + emission, 1.0); // combined
#endif
            }

            ENDHLSL
        }

        // Shadow pass
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }

    FallBack "Hidden/InternalErrorShader"
}
