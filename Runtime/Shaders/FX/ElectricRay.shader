Shader "UltimateXR/FX/Electric Ray"
{
	Properties
	{
		_MainTex                        ("Texture Main",            2D)               = "white" {}
		[NoScaleOffset]_NoiseTex        ("Texture Noise",           2D)               = "white" {}
		_Color                          ("Main Color",              Color)            = (1.0, 1.0, 1.0, 0.4)
		_NoiseScale1                    ("Noise Scale 1",           Float)            = 0.1
		_NoiseScale2                    ("Noise Scale 2",           Float)            = 0.2
		_NoiseSpeed1                    ("Noise Speed 1",           Vector)           = (0.1, 0.1, 0, 0)
		_NoiseSpeed2                    ("Noise Speed 2",           Vector)           = (-0.1, -0.1, 0, 0)
		_NoiseAmplitude1                ("Noise Amplitude 1",       Float)            = 1
		_NoiseAmplitude2                ("Noise Amplitude 2",       Float)            = 1
	    _DistortTimeStart               ("Distort Time Start",      Float)            = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				float4 color  : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv     : TEXCOORD0;
				float2 uv2    : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float4 color  : COLOR;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex, _NoiseTex;
			float4 _MainTex_ST;
			half4  _Color;
			half   _NoiseScale1, _NoiseScale2;
			half4  _NoiseSpeed1, _NoiseSpeed2;
			half   _NoiseAmplitude1, _NoiseAmplitude2;
			half   _DistortTimeStart;
			
			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2    = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color  = v.color;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Compute distort

				half2 noiseUV1 = i.uv + (_NoiseSpeed1.xy * (_DistortTimeStart + _Time.y * 1.3));
				half2 noiseUV2 = i.uv + (_NoiseSpeed2.xy * (_DistortTimeStart + _Time.y * 1.3));

				half3 noise1 = tex2D(_NoiseTex, noiseUV1 * _NoiseScale1);
				half3 noise2 = tex2D(_NoiseTex, noiseUV2 * _NoiseScale2);

				half noiseGlobal = saturate(1.0 - (abs(i.uv2.x - 0.5) * 2));

				noise1 = (noise1 - half3(0.5, 0.5, 0.5)) * 2 * _NoiseAmplitude1 * noiseGlobal;
				noise2 = (noise2 - half3(0.5, 0.5, 0.5)) * 2 * _NoiseAmplitude2 * noiseGlobal;

				// Compute

				half u = i.uv.x;
				half v = i.uv.y;

				half4 mainTex = tex2D(_MainTex, half2(u, v) + noise1.xy + noise2.xy);
				half3 finalRGB = mainTex.rgb * _Color.rgb * i.color.rgb;
				half finalAlpha = mainTex.a * _Color.a * i.color.a;

				return half4(finalRGB, finalAlpha);
			}
			ENDCG
		}
	}
}
