Shader "Custom/UberShader"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Alpha("Alpha", Range(0, 1)) = 1.0
        _EmissionMap("Emission Map", 2D) = "black" {}
        _EmissionColor("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionStrength("Emission Strength", Float) = 1
        _PulseSpeed("Pulse Speed", Float) = 1
        _MetallicGlossMap("Metallic Map", 2D) = "black" {}
        _MetallicColor("Metallic Color", Color) = (1, 1, 1, 1)
        _Metallic("Metallic", Range(0, 1)) = 0.0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BaseMap, _EmissionMap, _MetallicGlossMap;
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _Alpha;
            float4 _EmissionColor;
            float4 _MetallicColor;
            float _EmissionStrength, _PulseSpeed;
            float _Metallic, _Smoothness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                // Sample and tint the base map
                float4 baseCol = tex2D(_BaseMap, i.uv) * _BaseColor;
                
                // Sample and tint the metallic map
                float4 metallicSample = tex2D(_MetallicGlossMap, i.uv) * _MetallicColor;
                
                // Apply metallic effect (you can customize this blend mode)
                baseCol = lerp(baseCol, baseCol * metallicSample, _Metallic);
                
                // Sample emission map
                float4 emissive = tex2D(_EmissionMap, i.uv) * _EmissionColor * (_EmissionStrength * pulse);

                float4 finalColor = baseCol + emissive;
                finalColor.a = _Alpha;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Unlit/Texture"
}