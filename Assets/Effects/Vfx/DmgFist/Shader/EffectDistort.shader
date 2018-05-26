Shader "TY_Shader/Effect/EffectDistort" {
	Properties {
		_MainTex ("디스토트 이미지(흑백) ", 2D) = "white" {}
		_DistPow ("굴절 강도",Range(0,0.1) ) = 0.05
	}
	SubShader {
		Tags { "RenderType"="Transparent"  "Queue"="Transparent"}
		zwrite off
		GrabPass{}
		cull off
		CGPROGRAM
		#pragma surface surf nolight  noshadow nolightmap novertexlights noforwardadd noambient

		sampler2D _MainTex;
		sampler2D _GrabTexture;
		float _DistPow;
		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};


		void surf (Input IN, inout SurfaceOutput o) {
			float4 ref = tex2D (_MainTex,IN.uv_MainTex);
			float3 screenUV = IN.screenPos.rgb / IN.screenPos.a;
			o.Emission = tex2D (_GrabTexture,(screenUV.xy + (ref * _DistPow  ) ) );
		}

		float4 Lightingnolight (SurfaceOutput s, float3 lightDir, float atten){
			return float4(0,0,0,1);
		}

		ENDCG
	}
	FallBack "Transparent/VertexLit"
}
