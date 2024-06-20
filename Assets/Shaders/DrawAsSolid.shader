// https://willweissman.com/unity-outlines
Shader "Custom/DrawAsSolid"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            #include "UnityCG.cginc"

            struct VertexData
            {
                float4 vertex : POSITION;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
            };

            Interpolators VertexProgram (VertexData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 FragmentProgram (Interpolators i) : SV_Target
            {
                return fixed4(0.4,1,1,1);
            }
            ENDCG
        }
    }
}