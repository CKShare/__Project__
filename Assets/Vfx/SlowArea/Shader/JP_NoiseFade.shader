Shader "jpshader/JP_NoiseFade" {
	Properties {
		[HDR]_MainColor("기본색" , color) = (1,1,1,1)
		_MainTex ("기본 텍스쳐(RGB)", 2D) = "white" {}
		_EmitTex("셀프 일루미네이션 텍스쳐" , 2D ) =  "black" {} 
		_BumpTex("범프맵" , 2D ) = "blue" {} 
		_NoiseTex ("알파 텍스쳐 (R)", 2D) = "white"{}
		[Toggle]_IsSpecular("스페큘러가 있는가?_IsSpecular" ,float) = 0
		_SpecColor ( "스페큘러 칼라_SpecColor" , color) = (1,1,1,1)
		_Shiness("샤이니스_Shiness", float ) = 1
		[HideInInspector]_Cutoff ("알파테스트", float)= 0.5
		_Hide ("노이즈 한계치(1 ~ -1)_Hide" , Range(-1,1)) = 1 
		_AlphaColor ("알파 경계선 칼라_AlphaColor" , color ) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="alphatest" "Queue"="alphatest" }
//		LOD 200
		cull off
		
		CGPROGRAM
		#pragma surface surf BlinnPhong alphatest:_Cutoff
		#pragma target 3.0

		float4 _MainColor;
		sampler2D _MainTex;
		sampler2D _NoiseTex;
		sampler2D _EmitTex;
		sampler2D _BumpTex;
		float _IsSpecular;
		float _Hide;
		float _Shiness;
		float4 _AlphaColor;

		struct Input {
			float2 uv_MainTex;
			float2 uv_EmitTex;
			float2 uv_NoiseTex;
			float2 uv_BumpTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			half4 d = tex2D (_NoiseTex, IN.uv_NoiseTex);
			half4 emittex = tex2D (_EmitTex, IN.uv_EmitTex);
			o.Normal  = UnpackNormal(tex2D (_BumpTex, IN.uv_BumpTex));
			o.Albedo = c.rgb * _MainColor.rgb;
			float alphalevel = d.r + _Hide;
			o.Alpha = alphalevel;
			o.Specular = _Shiness;
			o.Gloss = _IsSpecular;
			o.Emission = emittex.rgb + (step (0.48, 1-alphalevel)*_AlphaColor.rgb*3) ;
//			o.Emission = step (0.48, 1-alphalevel);
		}
		ENDCG
	} 
	FallBack "Transparent/Diffuse"
}
