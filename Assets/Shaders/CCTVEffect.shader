Shader "Custom/CCTV"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Effect Intensity", Range(0, 1)) = 1
        _ScanLineIntensity ("Scan Line Intensity", Range(0, 1)) = 0.3
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.15
        _Desaturation ("Desaturation", Range(0, 1)) = 0.8
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.4
        _Time ("Time", Float) = 0
        _ShowTimestamp ("Show Timestamp", Float) = 1
        _ShowScanLines ("Show Scan Lines", Float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "CCTV"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float _Intensity;
            float _ScanLineIntensity;
            float _NoiseIntensity;
            float _Desaturation;
            float _VignetteIntensity;
            float _ShowTimestamp;
            float _ShowScanLines;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            // Noise function
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Generate timestamp digits
            float drawDigit(float2 uv, int digit, float2 pos, float scale)
            {
                float2 digitUV = (uv - pos) / scale;
                if (digitUV.x < 0 || digitUV.x > 1 || digitUV.y < 0 || digitUV.y > 1) return 0;
                
                // Simple 7-segment display patterns
                float segments[10 * 7] = {
                    // 0
                    1, 1, 1, 0, 1, 1, 1,
                    // 1
                    0, 0, 1, 0, 0, 1, 0,
                    // 2
                    1, 0, 1, 1, 1, 0, 1,
                    // 3
                    1, 0, 1, 1, 0, 1, 1,
                    // 4
                    0, 1, 1, 1, 0, 1, 0,
                    // 5
                    1, 1, 0, 1, 0, 1, 1,
                    // 6
                    1, 1, 0, 1, 1, 1, 1,
                    // 7
                    1, 0, 1, 0, 0, 1, 0,
                    // 8
                    1, 1, 1, 1, 1, 1, 1,
                    // 9
                    1, 1, 1, 1, 0, 1, 1
                };
                
                // Simplified digit rendering
                float intensity = 0;
                if (digitUV.y > 0.8 && segments[digit * 7 + 0] > 0.5) intensity = 1; // top
                if (digitUV.x < 0.2 && digitUV.y > 0.5 && segments[digit * 7 + 1] > 0.5) intensity = 1; // top-left
                if (digitUV.x > 0.8 && digitUV.y > 0.5 && segments[digit * 7 + 2] > 0.5) intensity = 1; // top-right
                if (digitUV.y > 0.4 && digitUV.y < 0.6 && segments[digit * 7 + 3] > 0.5) intensity = 1; // middle
                if (digitUV.x < 0.2 && digitUV.y < 0.5 && segments[digit * 7 + 4] > 0.5) intensity = 1; // bottom-left
                if (digitUV.x > 0.8 && digitUV.y < 0.5 && segments[digit * 7 + 5] > 0.5) intensity = 1; // bottom-right
                if (digitUV.y < 0.2 && segments[digit * 7 + 6] > 0.5) intensity = 1; // bottom
                
                return intensity;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // Apply desaturation
                float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
                color.rgb = lerp(color.rgb, float3(gray, gray, gray), _Desaturation);
                
                // Add slight green tint (typical CCTV look)
                color.rgb *= float3(0.9, 1.1, 0.8);
                
                // Scan lines
                if (_ShowScanLines > 0.5)
                {
                    float scanLine = sin(uv.y * 800.0) * 0.5 + 0.5;
                    scanLine = pow(scanLine, 8.0);
                    color.rgb *= 1.0 - scanLine * _ScanLineIntensity;
                }
                
                // Noise
                float noise = random(uv + _Time * 0.1) * 2.0 - 1.0;
                color.rgb += noise * _NoiseIntensity;
                
                // Vignette
                float2 vignetteUV = uv * 2.0 - 1.0;
                float vignette = 1.0 - dot(vignetteUV, vignetteUV) * _VignetteIntensity;
                color.rgb *= vignette;
                
                // Timestamp overlay
                if (_ShowTimestamp > 0.5)
                {
                    float time = _Time;
                    int hours = int(time / 3600.0) % 24;
                    int minutes = int(time / 60.0) % 60;
                    int seconds = int(time) % 60;
                    
                    float timestamp = 0;
                    float digitScale = 0.02;
                    float2 startPos = float2(0.02, 0.9);
                    
                    // Draw time digits (simplified)
                    timestamp += drawDigit(uv, hours / 10, startPos, digitScale);
                    timestamp += drawDigit(uv, hours % 10, startPos + float2(0.03, 0), digitScale);
                    timestamp += drawDigit(uv, minutes / 10, startPos + float2(0.08, 0), digitScale);
                    timestamp += drawDigit(uv, minutes % 10, startPos + float2(0.11, 0), digitScale);
                    timestamp += drawDigit(uv, seconds / 10, startPos + float2(0.16, 0), digitScale);
                    timestamp += drawDigit(uv, seconds % 10, startPos + float2(0.19, 0), digitScale);
                    
                    // Add colon separators
                    if (uv.x > 0.065 && uv.x < 0.075 && ((uv.y > 0.92 && uv.y < 0.925) || (uv.y > 0.905 && uv.y < 0.91)))
                        timestamp = 1;
                    if (uv.x > 0.135 && uv.x < 0.145 && ((uv.y > 0.92 && uv.y < 0.925) || (uv.y > 0.905 && uv.y < 0.91)))
                        timestamp = 1;
                    
                    color.rgb = lerp(color.rgb, float3(1, 1, 1), timestamp * 0.8);
                }
                
                // Add recording indicator
                float2 recUV = uv - float2(0.95, 0.95);
                float recDot = length(recUV);
                if (recDot < 0.01)
                {
                    float blink = sin(_Time * 3.0) * 0.5 + 0.5;
                    color.rgb = lerp(color.rgb, float3(1, 0, 0), blink);
                }
                
                // Apply overall intensity
                color.rgb = lerp(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb, color.rgb, _Intensity);
                
                return color;
            }
            ENDHLSL
        }
    }
}