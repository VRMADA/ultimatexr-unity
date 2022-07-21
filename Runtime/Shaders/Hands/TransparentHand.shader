Shader "UltimateXR/Hands/Transparent Hand"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1.0, 0.4)
		_RimPower ("Rim Power", Range(1.0, 32.0)) = 10.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"  }
		LOD 100

		Pass
		{
			Blend SrcAlpha One
			Cull Off
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : TEXCOORD0;
				float3 viewDir : TEXCOORD1;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			half4 _Color;
			half _RimPower;
			
			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				col.a = cos(dot(i.normal, i.viewDir));
				col.a = pow(col.a, _RimPower) * _Color.a;
				return col;
			}
			ENDCG
		}
	}
}
