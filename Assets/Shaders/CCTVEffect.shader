Shader "Fullscreen/CCTVEffect"
{
    Properties
    {
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineCount ("Scanline Count", Range(1, 50)) = 25
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.4
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.1
        _DistortionAmount ("Distortion Amount", Range(0, 1)) = 0.02
        _ColorDesaturation ("Color Desaturation", Range(0, 1)) = 0.6
        _TintColor ("Tint Color", Color) = (0.8, 1, 0.8, 1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Pass
        {
            Name "CCTVFullscreenPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _ScanlineIntensity;
                float _ScanlineCount;
                float _VignetteIntensity;
                float _NoiseIntensity;
                float _DistortionAmount;
                float _ColorDesaturation;
                float4 _TintColor;
            CBUFFER_END

            // Random function for noise generation
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Improved noise function
            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Apply barrel distortion
            float2 applyBarrelDistortion(float2 uv, float distortionAmount)
            {
                float2 center = uv - 0.5;
                float distortion = dot(center, center);
                return uv + center * distortion * distortionAmount;
            }

            // Calculate vignette effect
            float calculateVignette(float2 uv, float intensity)
            {
                float2 vignetteUV = uv * (1.0 - uv.yx);
                float vignette = vignetteUV.x * vignetteUV.y * 15.0;
                vignette = pow(saturate(vignette), 0.25);
                return lerp(vignette, 1.0, 1.0 - intensity);
            }

            // Generate scanlines
            float generateScanlines(float2 uv, float intensity, float count)
            {
                float scanline = sin(uv.y * count * PI) * 0.5 + 0.5;
                scanline = pow(scanline, 2.0);
                return lerp(1.0, scanline, intensity);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 uv = input.texcoord;
                
                // Apply barrel distortion first
                float2 distortedUV = applyBarrelDistortion(uv, _DistortionAmount);
                
                // Sample the source texture
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, distortedUV);
                
                // Desaturate the image
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, luminance.xxx, _ColorDesaturation);
                
                // Apply color tint
                col.rgb *= _TintColor.rgb;
                
                // Add scanlines
                col.rgb *= generateScanlines(uv, _ScanlineIntensity, _ScanlineCount);
                
                // Apply vignette
                col.rgb *= calculateVignette(uv, _VignetteIntensity);
                
                // Add spatial noise
                float spatialNoise = noise(uv * 100.0 + _Time * 5.0);
                col.rgb += (spatialNoise - 0.5) * _NoiseIntensity * 0.1;
                
                // Add temporal noise for that authentic camera feel
                float temporalNoise = random(uv + frac(_Time * 0.1));
                col.rgb += (temporalNoise - 0.5) * _NoiseIntensity * 0.05;
                
                // Add subtle flickering
                float flicker = sin(_Time * 60.0) * 0.005 + 1.0;
                col.rgb *= flicker;
                
                // Add occasional horizontal interference lines
                float interference = step(0.995, random(float2(uv.y * 100.0, floor(_Time * 10.0))));
                col.rgb += interference * 0.1;
                
                // Ensure proper color range
                col.rgb = saturate(col.rgb);
                
                #ifdef _LINEAR_TO_SRGB_CONVERSION
                col.rgb = LinearToSRGB(col.rgb);
                #endif
                
                return col;
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}