
Shader "Hidden/XRay" 
{
	Properties 
	{
		_XRayColor("XRay Color", Color) = (1, 1, 1, 1)
		_Thickness("Thickness", Range(0, 3)) = 1.5
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		Stencil
		{
			ReadMask 1
			Ref 0
			Comp Equal
		}

		ZWrite Off
		ZTest Always
		Blend One One

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

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float NdotV = 1 - dot(i.normal, i.viewDir) * (3 - _Thickness);
				return _XRayColor * NdotV;
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
