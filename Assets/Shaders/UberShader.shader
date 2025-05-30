// ========================================
// ENHANCED UBER SHADER WITH ALPHA SUPPORT
// ========================================

Shader "Custom/UberShader"
{
    Properties
    {
        [Header(Base Properties)]
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        
        [Header(Transparency)]
        [Enum(Opaque,0,Transparent,1,Cutout,2)] _BlendMode("Blend Mode", Float) = 0
        _Alpha("Alpha", Range(0,1)) = 1.0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        
        [Header(Detail Maps)]
        [Toggle(_USE_METALLICMAP)] _EnableMetallic("Enable Metallic/Roughness Map", Float) = 0
        _MetallicMap("Metallic Map (R=Metallic, G=Roughness)", 2D) = "white" {}
        
        [Toggle(_USE_NORMALMAP)] _EnableNormal("Enable Normal Map", Float) = 0
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0,2)) = 1
        
        _AOMap("Ambient Occlusion Map", 2D) = "white" {}
        _AOStrength("AO Strength", Range(0,1)) = 1
        
        [Header(Emission)]
        [Toggle(_USE_EMISSION)] _EnableEmission("Enable Emission", Float) = 0
        _EmissiveMap("Emissive Map", 2D) = "black" {}
        _EmissiveColor("Emissive Color", Color) = (1,1,1,1)
        _EmissiveStrength("Emission Strength", Range(0,10)) = 1
        _PulseSpeed("Pulse Speed", Range(0,10)) = 0
        _PulseAmplitude("Pulse Amplitude", Range(0,1)) = 0.5
        
        [Toggle(_FORCE_UNLIT_EMISSION)] _UnlitEmission("Emission Overrides Lighting", Float) = 0
        
        [Header(Rendering)]
        [Enum(Back,2,Front,1,Off,0)] _CullMode("Cull Mode", Float) = 2
        
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

        // Opaque Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Feature toggles
            #pragma multi_compile _ _USE_NORMALMAP
            #pragma multi_compile _ _USE_EMISSION
            #pragma multi_compile _ _USE_METALLICMAP
            #pragma multi_compile _ _USE_DETAIL
            #pragma multi_compile _ _USE_FRESNEL
            #pragma multi_compile _ _USE_UV_SCROLL
            #pragma multi_compile _ _FORCE_UNLIT_EMISSION
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _ALPHABLEND_ON
            
            // Unity lighting
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
#if _USE_NORMALMAP
                float3 tangentWS : TEXCOORD6;
                float3 bitangentWS : TEXCOORD7;
#endif
            };

            // Texture declarations
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_EmissiveMap); SAMPLER(sampler_EmissiveMap);
            TEXTURE2D(_AOMap); SAMPLER(sampler_AOMap);
            TEXTURE2D(_MetallicMap); SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);

            // Property declarations
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmissiveColor;
                float4 _FresnelColor;
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
                float4 _BaseMap_ST;
                float4 _DetailMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                
                OUT.positionHCS = positionInputs.positionCS;
                OUT.worldPos = positionInputs.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv2 = IN.uv2;
                OUT.normalWS = normalInputs.normalWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                OUT.shadowCoord = GetShadowCoord(positionInputs);

#if _USE_NORMALMAP
                OUT.tangentWS = normalInputs.tangentWS;
                OUT.bitangentWS = normalInputs.bitangentWS;
#endif
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // UV Scrolling
                float2 scrolledUV = IN.uv;
#if _USE_UV_SCROLL
                scrolledUV += float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;
#endif

                // Sample base textures
                float4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, scrolledUV);
                float3 albedo = baseSample.rgb * _BaseColor.rgb;
                float alpha = baseSample.a * _BaseColor.a * _Alpha;

                // Alpha testing for cutout mode
#if _ALPHATEST_ON
                clip(alpha - _Cutoff);
#endif

                // Normal mapping
                float3 normalWS = normalize(IN.normalWS);
#if _USE_NORMALMAP
                float4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, scrolledUV);
                float3 tangentNormal = UnpackNormalScale(normalSample, _NormalStrength);
                float3x3 TBN = float3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalWS);
                normalWS = normalize(mul(tangentNormal, TBN));
#endif

                // Material properties
                float metallic = _Metallic;
                float smoothness = _Smoothness;

#if _USE_METALLICMAP
                float4 metallicSample = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, scrolledUV);
                metallic *= metallicSample.r;
                smoothness *= (1.0 - metallicSample.g); // Green channel is roughness
#endif

                // Ambient occlusion
                float ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, IN.uv2).r;
                ao = lerp(1.0, ao, _AOStrength);

                // Detail mapping
#if _USE_DETAIL
                float2 detailUV = TRANSFORM_TEX(IN.uv, _DetailMap);
                float3 detail = SAMPLE_TEXTURE2D(_DetailMap, sampler_DetailMap, detailUV).rgb;
                detail = lerp(float3(0.5, 0.5, 0.5), detail, _DetailStrength);
                albedo = albedo * detail * 2.0;
#endif

                // View direction
                float3 viewDirWS = normalize(IN.viewDirWS);

                // Initialize lighting
                float3 lighting = 0;

#if !_FORCE_UNLIT_EMISSION
                // PBR Lighting
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(IN.uv2);

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

                lighting = UniversalFragmentPBR(inputData, surfaceData).rgb;
#endif

                // Emission
                float3 emission = 0;
#if _USE_EMISSION
                float pulse = 1.0;
                if (_PulseSpeed > 0)
                {
                    pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmplitude;
                }
                
                float3 emissiveTex = SAMPLE_TEXTURE2D(_EmissiveMap, sampler_EmissiveMap, scrolledUV).rgb;
                emission = emissiveTex * _EmissiveColor.rgb * _EmissiveStrength * pulse;
#endif

                // Fresnel effect
#if _USE_FRESNEL
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                float3 fresnelContribution = fresnel * _FresnelColor.rgb * _FresnelStrength;
                emission += fresnelContribution;
#endif

                // Final color combination
#if _FORCE_UNLIT_EMISSION
                return float4(emission, alpha);
#else
                return float4(lighting + emission, alpha);
#endif
            }

            ENDHLSL
        }

        // Shadow pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile _ _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile _ _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    CustomEditor "UberShaderGUI"
    FallBack "Hidden/InternalErrorShader"
}

// ========================================
// CUSTOM SHADER GUI FOR BLEND MODE CONTROL
// ========================================

/*
Create this C# script in your project as "UberShaderGUI.cs":

using UnityEngine;
using UnityEditor;

public class UberShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material target = materialEditor.target as Material;
        
        // Find blend mode property
        MaterialProperty blendMode = FindProperty("_BlendMode", properties);
        
        // Draw default inspector
        base.OnGUI(materialEditor, properties);
        
        // Set render states based on blend mode
        SetupMaterialBlendMode(target, (int)blendMode.floatValue);
    }
    
    private void SetupMaterialBlendMode(Material material, int blendMode)
    {
        switch (blendMode)
        {
            case 0: // Opaque
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = -1;
                break;
                
            case 1: // Transparent
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
                
            case 2: // Cutout
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
        }
    }
}
*/

// ========================================
// MATERIAL SETUP EXAMPLES WITH ALPHA
// ========================================

/*
TRANSPARENT GLASS DOOR MATERIAL:
===============================
Material Name: "GlassDoorMaterial"

Base Properties:
- Base Map: GlassDoor_Albedo.png
- Base Color: (1, 1, 1, 1)
- Metallic: 0.1
- Smoothness: 0.9 (very smooth for glass)

Transparency:
- Blend Mode: Transparent
- Alpha: 0.3 (30% opacity for glass effect)

Emission:
- Enable Emission: ON
- Emissive Map: GlassDoor_Emission.png
- Emissive Color: (0.2, 0.8, 1, 1)
- Emission Strength: 1.5

Fresnel Effect:
- Enable Fresnel: ON
- Fresnel Color: (1, 1, 1, 1)
- Fresnel Power: 5 (strong fresnel for glass)
- Fresnel Strength: 2

FADING HOLOGRAM PANEL:
=====================
Material Name: "HologramPanelMaterial"

Base Properties:
- Base Map: Hologram_Albedo.png
- Base Color: (0.5, 1, 0.8, 1)
- Metallic: 0
- Smoothness: 0.1

Transparency:
- Blend Mode: Transparent
- Alpha: 0.6 (semi-transparent hologram)

Emission:
- Enable Emission: ON
- Emissive Map: Hologram_Emission.png
- Emissive Color: (0, 1, 0.5, 1)
- Emission Strength: 3
- Pulse Speed: 2 (animated hologram flicker)
- Pulse Amplitude: 0.4

UV Scrolling:
- Enable UV Scrolling: ON
- Scroll Speed Y: 0.5 (scrolling data effect)

USAGE NOTES:
===========
1. Transparent objects render after opaque ones, so order matters
2. Use Cutout mode for sharp edges (like perforated metal)
3. Fresnel effect is very effective on transparent materials
4. Combine alpha with emission for glowing transparent effects
5. The Alpha slider controls overall transparency
6. Base texture alpha channel is multiplied with the Alpha slider
*/