Shader "Custom/SquareSelector"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline", Color) = (0,0,0,1)
        _Transparency ("Transparency", Range(0,1)) = 0.7
        _OutlineWidth ("Outline Width", Range(0, 0.025)) = 0.01
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        fixed4 _Color;
        float _Transparency;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = _Transparency; // Apply the transparency factor
        }
        ENDCG

        Pass
        {
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
            }
            CGPROGRAM

            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            half _OutlineWidth;

            // uv contains data for mesh extrusion
            float4 VertexProgram( float4 position : POSITION, float2 uv : TEXCOORD0) : SV_POSITION {
                float4 clipPosition = UnityObjectToClipPos(position);
                float3 clipNormal = mul((float3x3) UNITY_MATRIX_VP, mul((float3x3) UNITY_MATRIX_M, float3(uv.x, 0, uv.y)));
            
                clipPosition.xyz += normalize(clipNormal) * _OutlineWidth;
            
                return clipPosition;
            }

            half4 _OutlineColor;

            half4 FragmentProgram() : SV_TARGET {
                return _OutlineColor;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}