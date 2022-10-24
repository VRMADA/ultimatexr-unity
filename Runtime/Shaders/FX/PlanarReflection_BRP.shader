Shader "UltimateXR/FX/Stereo Planar Reflection (BRP)"
{
    Properties
    {
        _Color("Albedo", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
	    _ReflectionColor("Reflection Color", Color) = (1,1,1,1)
		_ReflectionBlurMask("Reflection Blur (R), Mask (G)", 2D) = "white" {}
        _Smoothness ("Smoothness", range(0.0, 1.0)) = 1.0
		_BlurAmount("Blur Amount", range(0.0, 1.0)) = 1.0
        _Metallic ("Metallic (RGB) + Smoothness (A)", 2D) = "white" {}
		[Normal]
        _Normal ("Normal", 2D) = "bump" {}
        _NormalIntensity("Normal Intensity", Range(0, 1)) = 1
		_ReflectionTexLeft("ReflectionTextureLeft", 2D) = "white" {}
		_ReflectionTexRight("ReflectionTextureRight", 2D) = "white" {}
		_ReflectionIntensity("ReflectionIntensity", range(0.0, 1.0)) = 0.5
		_DistortionIntensity("DistortionIntensity", range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Metallic;
            float2 uv_Normal;
            float2 uv_DistortionTex;
            float4 screenPos;
        };

        int _Stereo;
		half4 _Color;
		half4 _ReflectionColor;
        sampler2D _MainTex;
		sampler2D _ReflectionBlurMask;
        float _Smoothness;
		float _BlurAmount;
        float _NormalIntensity;
        float _ReflectionMaxLODBias;
        sampler2D _Metallic;
        sampler2D _Normal;
        sampler2D _ReflectionTexLeft;
		sampler2D _ReflectionTexRight;
        half _ReflectionIntensity;
        half _DistortionIntensity;

        #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

		float4 GetPixelBlurredLeft(float2 coord, float bias)
		{
			float bias1 = floor(bias * _ReflectionMaxLODBias);
			float bias2 = ceil (bias * _ReflectionMaxLODBias);

			float4 value1 = tex2Dbias(_ReflectionTexLeft, float4(coord.x, coord.y, 0, bias1));
			float4 value2 = tex2Dbias(_ReflectionTexLeft, float4(coord.x, coord.y, 0, bias2));

			float t = (bias * _ReflectionMaxLODBias) - bias1;

			return (value1 * (1 - t)) + (value2 * t);
		}

		half4 GetPixelBlurredRight(float2 coord, float bias)
		{
			return tex2Dbias(_ReflectionTexLeft, float4(coord.x, coord.y, 0, bias * _ReflectionMaxLODBias));
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 mainColor = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float3 normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));

			float4 reflectionBlurMask = tex2D(_ReflectionBlurMask, IN.uv_MainTex);

		    float2 distortion   = normal.xy * _DistortionIntensity;
            float2 screenCoords = IN.screenPos.xy / IN.screenPos.w;

#if UNITY_SINGLE_PASS_STEREO

			float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
			screenCoords = (screenCoords - scaleOffset.zw) / scaleOffset.xy;
            screenCoords = screenCoords + distortion;

            float4 reflectionBlur;
			float4 reflectionNoBlur;

			if(unity_StereoEyeIndex == 0 && _Stereo > 0)
			{
				reflectionNoBlur = GetPixelBlurredLeft(screenCoords, 0.0);
				reflectionBlur   = GetPixelBlurredLeft(screenCoords, 1.0);
			}
			else if(unity_StereoEyeIndex == 1 && _Stereo > 0)
			{
				reflectionNoBlur = GetPixelBlurredRight(screenCoords, 0.0);
				reflectionBlur   = GetPixelBlurredRight(screenCoords, 1.0);
			}

#else

            screenCoords = screenCoords + distortion;

			float4 reflectionNoBlur = GetPixelBlurredLeft(screenCoords, 0.0);
			float4 reflectionBlur   = GetPixelBlurredLeft(screenCoords, 1.0);

#endif

			float4 metallicSmoothness = tex2D(_Metallic, IN.uv_Metallic);
			float blurAmount = reflectionBlurMask.r * _BlurAmount;

            float4 reflection = _ReflectionIntensity * ((reflectionNoBlur * (1.0 - blurAmount)) + (reflectionBlur * blurAmount));
			float4 albedo     = (reflectionBlurMask.g * reflection) + ((1 - reflectionBlurMask.g) * mainColor);

			o.Albedo     = albedo.rgb;
			o.Normal     = lerp(half3(0, 0, 1), normal, _NormalIntensity);
            o.Alpha      = mainColor.a;
			o.Metallic   = metallicSmoothness.r * (1.0 - reflectionBlurMask.g);
			o.Smoothness = metallicSmoothness.g * (1.0 - reflectionBlurMask.g);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
