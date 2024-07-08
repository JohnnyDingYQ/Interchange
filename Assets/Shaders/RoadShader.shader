// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RoadShader" {

	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader {

		Pass {
            CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "UnityCG.cginc"

			float4 _Color;
			float4 _MainTex_ST;
			sampler2D _MainTex;

			struct VertexData {
				float4 position : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv: TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};
			
			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;;
				return i;
			}


			float4 FragmentProgram (Interpolators i) : SV_TARGET {
				float4 tex = tex2D(_MainTex, i.uv);
				// if (all(tex == float4(1, 1, 1, 1)))
				// return tex;
				return _Color + i.worldPos.y / 20;
			}
			ENDCG
		}
	}
}