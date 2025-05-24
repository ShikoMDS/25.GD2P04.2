Shader "Custom/UberShader"
{
    Properties
    {
        _BaseMap ("Base (Diffuse)", 2D) = "white" {}
        _BaseColor ("Base Tint", Color) = (1, 1, 1, 1)

        [Toggle(_USENORMALMAP)] _UseNormalMap ("Use Normal Map", Float) = 0
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Float) = 1.0

        [Toggle(_USEMETALLICWORKFLOW)] _UseMetallicWorkflow ("Use Metallic Workflow", Float) = 0
        _MetallicMap ("Metallic Map", 2D) = "white" {}
        _MetallicColor ("Metallic Tint", Color) = (1, 1, 1, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.5

        [Toggle(_ENABLEEMISSION)] _EnableEmission ("Enable Emission", Float) = 0
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionStrength ("Emission Strength", Float) = 1.0

        [Toggle(_ENABLEPULSE)] _EnablePulse ("Enable Pulse", Float) = 0
        _PulseSpeed ("Pulse Speed", Float) = 0.0
        _PulseIntensity ("Pulse Intensity", Float) = 1.0

        [Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha Test", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _USENORMALMAP
            #pragma multi_compile _USEMETALLICWORKFLOW
            #pragma multi_compile _ENABLEEMISSION
            #pragma multi_compile _ENABLEPULSE
            #pragma multi_compile _ALPHATEST_ON

            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            float4 _BaseColor;

            sampler2D _NormalMap;
            float _NormalStrength;

            sampler2D _MetallicMap;
            float4 _MetallicColor;
            float _Metallic;

            sampler2D _EmissionMap;
            float4 _EmissionColor;
            float _EmissionStrength;

            float _PulseSpeed;
            float _PulseIntensity;

            float _Cutoff;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 baseCol = tex2D(_BaseMap, i.uv) * _BaseColor;

                #ifdef _ALPHATEST_ON
                clip(baseCol.a - _Cutoff);
                #endif

                float3 finalCol = baseCol.rgb;

                #ifdef _ENABLEEMISSION
                    float pulse = 1.0;
                    #ifdef _ENABLEPULSE
                        pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                        pulse *= _PulseIntensity;
                    #endif
                    float3 emission = tex2D(_EmissionMap, i.uv).rgb * _EmissionColor.rgb * _EmissionStrength * pulse;
                    finalCol += emission;
                #endif

                return float4(finalCol, baseCol.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
