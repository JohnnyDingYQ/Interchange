// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/RoadShader" {

	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader {

		Pass {
            CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "UnityCG.cginc"

			float4 _Color;

			struct Interpolators {
				float4 position : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};
			
			
			Interpolators VertexProgram (float4 position : POSITION) {
				Interpolators i;
				i.worldPos = mul(unity_ObjectToWorld, position).xyz;
				i.position = UnityObjectToClipPos(position);
				return i;
			}


			float4 FragmentProgram (Interpolators i) : SV_TARGET {
				// return _Color;
				return _Color + i.worldPos.y / 20;
			}
			ENDCG
		}
	}
}