Shader "Custom/TerrainShader" {

	Properties {
		_MainTex ("Texture", 2D) = "white" {}
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
			float4 _MainTex_ST;
			sampler2D _MainTex;

			struct VertexData {
				float4 position : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float2 uv: TEXCOORD0;
			};
			
			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.worldPos = mul(unity_ObjectToWorld, v.position).xyz;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;;
				return i;
			}

			float4 FragmentProgram (Interpolators i) : SV_TARGET {				
				if (abs(round(i.worldPos.x / 100) * 100 - i.worldPos.x) < 2 || abs(round(i.worldPos.z / 100) * 100 - i.worldPos.z) < 2)
					return _WaterColor;
				return _LandColor;
			}
			ENDCG
		}
	}
}