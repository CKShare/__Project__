
Shader "Hidden/XRay" 
{
	Properties 
	{
		_XRayColor("XRay Color", Color) = (1, 1, 1, 1)
		_Thickness("Thickness", Range(0, 3)) = 1.5
		_Radius("Radius", Range(0.01, 1000)) = 300
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"XRay" = "XRay"
		}

		ZWrite Off
		ZTest Always
		Blend One Zero

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			float4 _XRayColor;
			float _Thickness;
			float _Radius;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD0;
				float3 world : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.world = mul(unity_ObjectToWorld, v.vertex);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - o.world);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float len = length(_WorldSpaceCameraPos.xyz - i.world);
				len = 1 - floor(saturate(len / _Radius));
				float NdotV = 1 - dot(i.normal, i.viewDir) * (3 - _Thickness);
				return _XRayColor * NdotV * len;
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}