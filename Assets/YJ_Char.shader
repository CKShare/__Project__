Shader "Custom/YJ_Char" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Bumpmap ("Bump", 2D) = "Bump" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf YJ_Char fullforwardshadows

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Bumpmap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Bumpmap;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Normal = UnpackNormal(tex2D(_Bumpmap, IN.uv_Bumpmap));
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		float4 LightingYJ_Char (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten){

		//디퓨즈 연산
		float3 diff;
		float NdotL = saturate (dot(s.Normal, lightDir));
		diff = NdotL * _LightColor0.rgb * atten;
		diff *= s.Albedo;

		//최종연산
		float4 finalColor;
		finalColor.rgb = diff;
		finalColor.a = s.Alpha;

		return finalColor;

		}
		ENDCG
	}
	FallBack "Diffuse"
}
