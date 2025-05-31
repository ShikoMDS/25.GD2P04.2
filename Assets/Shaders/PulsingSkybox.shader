Shader "Skybox/PulsingSkybox"
{
	// ====================================================================
	// PROPERTIES - Exposed parameters for material inspector
	// ====================================================================
	Properties
	{
		// Skybox texture inputs for each face of the cubemap
		[NoScaleOffset] _FrontTex ("Front (+Z)", 2D) = "black" {}
		[NoScaleOffset] _BackTex ("Back (-Z)", 2D) = "black" {}
		[NoScaleOffset] _LeftTex ("Left (+X)", 2D) = "black" {}
		[NoScaleOffset] _RightTex ("Right (-X)", 2D) = "black" {}
		[NoScaleOffset] _UpTex ("Up (+Y)", 2D) = "black" {}
		[NoScaleOffset] _DownTex ("Down (-Y)", 2D) = "black" {}

		// Pulsing animation controls
		_PulseSpeed ("Pulse Speed", Range(0.1, 5.0)) = 1.0
		_PulseIntensity ("Pulse Intensity", Range(0.0, 2.0)) = 0.5
		_BaseIntensity ("Base Intensity", Range(0.0, 2.0)) = 0.8

		// Toggle switches for each skybox face
		[Toggle] _UseFront ("Use Front", Float) = 1
		[Toggle] _UseBack ("Use Back", Float) = 1
		[Toggle] _UseLeft ("Use Left", Float) = 1
		[Toggle] _UseRight ("Use Right", Float) = 1
		[Toggle] _UseUp ("Use Up", Float) = 1
		[Toggle] _UseDown ("Use Down", Float) = 1

		// Visual enhancement properties
		_Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
		_Exposure ("Exposure", Range(0, 8)) = 1.3
		_Rotation ("Rotation", Range(0, 360)) = 0

		// Star rendering controls
		_StarThreshold ("Star Threshold", Range(0.0, 1.0)) = 0.7
		_StarSize ("Star Size", Range(0.0, 2.0)) = 1.0
	}

	// ====================================================================
	// SUBSHADER - Main rendering implementation
	// ====================================================================
	SubShader
	{
		Tags
		{
			"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
		}
		Cull Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			// ================================================================
			// SHADER VARIABLES - Property declarations for GPU
			// ================================================================

			// Texture samplers for each skybox face
			sampler2D _FrontTex;
			sampler2D _BackTex;
			sampler2D _LeftTex;
			sampler2D _RightTex;
			sampler2D _UpTex;
			sampler2D _DownTex;

			// Animation parameters
			float _PulseSpeed;
			float _PulseIntensity;
			float _BaseIntensity;

			// Face toggle switches
			float _UseFront;
			float _UseBack;
			float _UseLeft;
			float _UseRight;
			float _UseUp;
			float _UseDown;

			// Visual enhancement variables
			half4 _Tint;
			half _Exposure;
			float _Rotation;

			// Star rendering parameters
			float _StarThreshold;
			float _StarSize;

			// ================================================================
			// VERTEX SHADER STRUCTURES
			// ================================================================
			struct appdata_t
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			// ================================================================
			// UTILITY FUNCTIONS
			// ================================================================

			// Rotates a 3D point around the Y-axis by specified degrees
			float3 RotateAroundYInDegrees(float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
			}

			// ================================================================
			// VERTEX SHADER - Transform vertices and prepare interpolants
			// ================================================================
			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				// Apply rotation to the skybox
				float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
				o.vertex = UnityObjectToClipPos(rotated);
				o.texcoord = v.vertex.xyz;

				return o;
			}

			// ================================================================
			// STAR SAMPLING FUNCTION - Handles star detection and pulsing
			// ================================================================
			float4 SampleStarTexture(sampler2D tex, float2 uv, float useTexture, float pulseValue)
			{
				// Early exit if this texture face is disabled
				if (useTexture < 0.5) return float4(0, 0, 0, 0);

				// Sample the texture and calculate luminance
				float4 col = tex2D(tex, uv);
				float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));

				// Determine if this pixel represents a star based on brightness threshold
				float isStar = smoothstep(_StarThreshold - 0.1, _StarThreshold + 0.1, luminance);

				// Apply star size multiplier
				col.rgb *= isStar * _StarSize;

				// Apply pulsing animation to star brightness
				float pulse = _BaseIntensity + _PulseIntensity * pulseValue;
				col.rgb *= pulse * isStar;

				// Set alpha based on star visibility
				col.a = saturate(luminance * isStar * 2.0);

				return col;
			}

			// ================================================================
			// FRAGMENT SHADER - Main rendering logic
			// ================================================================
			fixed4 frag(v2f i) : SV_Target
			{
				float3 dir = normalize(i.texcoord);
				float4 col = float4(0, 0, 0, 1);

				// ============================================================
				// PULSE TIMING CALCULATION
				// ============================================================
				// Create position-based variation for pulse timing to avoid uniform pulsing
				float posVariation = frac(dot(dir, float3(12.9898, 78.233, 45.164))) * 0.5;
				float timeVariation = _Time.y * _PulseSpeed + posVariation;
				float pulseValue = sin(timeVariation) * 0.5 + 0.5;

				// ============================================================
				// CUBEMAP FACE SELECTION AND SAMPLING
				// ============================================================
				// Determine which face of the cube to sample based on the largest direction component
				float3 absDir = abs(dir);

				if (absDir.x >= absDir.y && absDir.x >= absDir.z)
				{
					// X face (Left/Right)
					float2 uv = float2(-dir.z, dir.y) / absDir.x * 0.5 + 0.5;
					if (dir.x > 0)
						col += SampleStarTexture(_LeftTex, uv, _UseLeft, pulseValue);
					else
						col += SampleStarTexture(_RightTex, uv, _UseRight, pulseValue);
				}
				else if (absDir.y >= absDir.x && absDir.y >= absDir.z)
				{
					// Y face (Up/Down)
					float2 uv = float2(dir.x, -dir.z) / absDir.y * 0.5 + 0.5;
					if (dir.y > 0)
						col += SampleStarTexture(_UpTex, uv, _UseUp, pulseValue);
					else
						col += SampleStarTexture(_DownTex, uv, _UseDown, pulseValue);
				}
				else
				{
					// Z face (Front/Back)
					float2 uv = float2(dir.x, dir.y) / absDir.z * 0.5 + 0.5;
					if (dir.z > 0)
						col += SampleStarTexture(_FrontTex, uv, _UseFront, pulseValue);
					else
						col += SampleStarTexture(_BackTex, uv, _UseBack, pulseValue);
				}

				// ============================================================
				// FINAL COLOR PROCESSING
				// ============================================================
				// Apply tint and exposure adjustments
				col.rgb *= _Tint.rgb * unity_ColorSpaceDouble.rgb;
				col.rgb *= _Exposure;

				return col;
			}
			ENDCG
		}
	}

	// ====================================================================
	// CUSTOM EDITOR - Links to custom material inspector
	// ====================================================================
	CustomEditor "PulsatingStarsShaderGUI"
}