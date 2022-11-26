Shader "UltimateXR/FX/Scroll Dual Texture"
{
	Properties
	{
		_MainTex    ("Texture Color+Alpha", 2D)      = "white" {}
		_Color      ("Color Background",    Color)   = (1, 1, 1, 1)
		_ScrollTex  ("Texture Scroll",      2D)      = "white" {}
		_ColorScroll("Color Scroll",        Color)   = (1, 1, 1, 1)
		_Speed      ("Scroll Speed",        Vector)  = (0, 1, 0, 0)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
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
			};

			struct v2f
			{
				float4 vertex   : POSITION;
				float2 uv       : TEXCOORD0;
				float2 uvScroll : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4    _MainTex_ST;
			sampler2D _ScrollTex;
			float4    _ScrollTex_ST;
			float4    _Color;
			float4    _ColorScroll;
			float4    _Speed;
			
			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex   = UnityObjectToClipPos(v.vertex);
				o.uv       = TRANSFORM_TEX(v.uv, _MainTex);
				o.uvScroll = TRANSFORM_TEX(v.uv, _ScrollTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 colorBackground = tex2D(_MainTex,   i.uv)       * _Color;
				float4 colorScroll     = tex2D(_ScrollTex, i.uvScroll + (_Speed.xy * _Time.y)) * _ColorScroll;
				float4 color = colorBackground * colorScroll;
				return color;
			}
			ENDCG
		}
	}
}
