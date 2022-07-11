Shader "UltimateXR/Video/Video RGB (Top) + Alpha (Bottom) AlphaBlend"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				float4 color  : COLOR;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				float4 color  : COLOR;
			};

			sampler2D _MainTex;
			float4    _MainTex_ST;
			float4    _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
#if UNITY_UV_STARTS_AT_TOP
				float4 rgb = tex2D(_MainTex, half2(i.uv.x, (i.uv.y * 0.5) + 0.5)) *_Color * i.color;
				float  a   = tex2D(_MainTex, half2(i.uv.x, i.uv.y * 0.5)).r * _Color.a * i.color.a;
#else
				float4 rgb = tex2D(_MainTex, half2(i.uv.x, i.uv.y * 0.5)) *_Color * i.color;
				float  a   = tex2D(_MainTex, half2(i.uv.x, (i.uv.y * 0.5) + 0.5)).r * _Color.a * i.color.a;
#endif
				return float4(rgb.rgb, a);
			}
			ENDCG
		}
	}
}
