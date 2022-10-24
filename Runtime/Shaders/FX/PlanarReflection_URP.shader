Shader "UltimateXR/FX/Stereo Planar Reflection (URP)"
{
    Properties
    {
        _Color("Albedo", Color) = (1,1,1,1)
		_ReflectionTexLeft("ReflectionTextureLeft",   2D) = "white" {}
		_ReflectionTexRight("ReflectionTextureRight", 2D) = "white" {}
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex    : POSITION;
				float2 uv        : TEXCOORD0;
				float4 screenPos : TEXCOORD1;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			half4     _Color;
			sampler2D _ReflectionTexLeft;
			sampler2D _ReflectionTexRight;

			v2f vert (appdata v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex    = UnityObjectToClipPos(v.vertex);
				o.uv        = v.uv;
				o.screenPos = ComputeScreenPos(o.vertex); 
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				if(unity_StereoEyeIndex == 0)
				{
					return _Color * tex2D(_ReflectionTexLeft, i.screenPos.xy / i.screenPos.w);
				}
				
				return _Color * tex2D(_ReflectionTexRight, i.screenPos.xy / i.screenPos.w);
			}
			
			ENDCG
		}
    }
}