// https://willweissman.com/unity-outlines
Shader "Custom/Post Outline copy"
{
    Properties
    {
        //Graphics.Blit() sets the "_MainTex" property to the source texture
        _MainTex("Main Texture", 2D) = "black"{}
        _KernelSize("Kernel Size", float) = 10
        _SceneTex("Scene Texture", 2D) = "black"{}
    }
    SubShader 
    {
        Pass 
        {
            Name "Vertical Gaussian Blur"
            CGPROGRAM
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _KernelSize;
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
             
            v2f vert (appdata_base v) 
            {
                v2f o;
                 
                //Despite only drawing a quad to the screen from -1,-1 to 1,1, Unity altered our verts, and requires us to use UnityObjectToClipPos.
                o.pos = UnityObjectToClipPos(v.vertex);
                 
                //Also, the uv show up in the top right corner for some reason, let's fix that.
                o.uv = o.pos.xy/2 + 0.5;
                 
                return o;
            }
             
             
            half4 frag(v2f i) : COLOR 
            {
                // Gaussian kernel
                half4 sum = 0;
                int samples = 2 * _KernelSize + 1;
                for (float y = 0; y < samples; y++)
                {
                    float2 offset =  float2(0, y - _KernelSize);
                    sum += tex2D(_MainTex, i.uv + offset * _MainTex_TexelSize.xy);
                }

                //return the texture we just looked up
                return sum / samples;
            }
            ENDCG
        }
        GrabPass{}

        Pass {
            Name "Horizontal Gaussian Blur"
            CGPROGRAM
            sampler2D _GrabTexture;
            sampler2D _SceneTex;
            sampler2D _MainTex;
            float2 _GrabTexture_TexelSize;
            float _KernelSize;
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            v2f vert (appdata_base v) 
            {
                v2f o;
                 
                //Despite only drawing a quad to the screen from -1,-1 to 1,1, Unity altered our verts, and requires us to use UnityObjectToClipPos.
                o.pos = UnityObjectToClipPos(v.vertex);
                 
                //Also, the uv show up in the top right corner for some reason, let's fix that.
                o.uv = o.pos.xy/2 + 0.5;
                 
                return o;
            }
             
             
            float4 frag(v2f i) : SV_Target 
            {
                return tex2D(_GrabTexture, i.uv);
                // Gaussian kernel
                half4 sum = 0;
                int samples = 2 * _KernelSize + 1;
                for (float x = 0; x < samples; x++)
                {
                    float2 offset =  float2(x - _KernelSize, 0);
                    sum += tex2D(_GrabTexture, i.uv + offset * _GrabTexture_TexelSize.xy);
                }

                //return the texture we just looked up
                // if (tex2D(_MainTex, i.uv).r > 0)
                //     sum = 0;
                    // return tex2D(_SceneTex, i.uv);
                return sum / samples;
                // return tex2D(_SceneTex,i.uv) + sum / samples;
            }
            ENDCG
        }
        //end pass
    }
    //end subshader
}
//end shader