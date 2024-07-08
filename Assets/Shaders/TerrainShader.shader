Shader "Custom/TerrainShader" {

	Properties {
		_LandColor ("Land Color", Color) = (0, 0, 0, 0)
		_WaterColor ("Water Color", Color) = (0, 0, 0, 0)
		_ClipHeight ("Clip Height", float) = 1000
	}

	SubShader {

		Pass {
            CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "UnityCG.cginc"

			float4 _LandColor;
			float4 _WaterColor;
			float _ClipHeight;

			struct VertexData {
				float4 position : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};
			
			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
				i.position = UnityObjectToClipPos(v.position);
				return i;
			}

			float4 FragmentProgram (Interpolators i) : SV_TARGET {				
				if (i.worldPos.y < _ClipHeight)
					return _WaterColor;
				return _LandColor;
			}
			ENDCG
		}
	}
}