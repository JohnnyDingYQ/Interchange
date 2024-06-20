// https://willweissman.com/unity-outlines
Shader "Custom/Post Outline"
{
    Properties
    {
        //Graphics.Blit() sets the "_MainTex" property to the source texture
        _MainTex("Main Texture", 2D) = "black"{}
        _KernelSize("Kernel Size", float) = 15
        _SceneTex("Scene Texture", 2D) = "black"{}
    }
    SubShader 
    {
        Pass 
        {
            Name "Vertical Gaussian Blur"
            CGPROGRAM
            #pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "UnityCG.cginc"

            float _KernelSize;
			sampler2D _MainTex;
            float4 _MainTex_TexelSize;
             
            struct VertexData {
				float4 position : POSITION;
				float2 uv: TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv: TEXCOORD0;
			};
			
			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = v.uv;
				return i;
			}

			float4 FragmentProgram (Interpolators i) : SV_TARGET {
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

        GrabPass
        {
            "_GrabTexture"
        }

        Pass
        {
            Name "Horizontal Gaussian Blur"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            float _KernelSize;
			sampler2D _MainTex;
			float4 _MainTex_ST;
            sampler2D _SceneTex;
            float2 _GrabTexture_TexelSize;
            float4 _MainTex_TexelSize;
            sampler2D _GrabTexture;
            #pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "UnityCG.cginc"

            struct VertexData {
				float4 position : POSITION;
                float2 uv: TEXCOORD0;
			};

			struct Interpolators {
				float4 position : SV_POSITION;
                float2 uv: TEXCOORD2;
				float4 grabPos : TEXCOORD1;
			};
			
			Interpolators VertexProgram (VertexData v) {
				Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.grabPos = ComputeGrabScreenPos(i.position);
                i.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return i;
			}

			float4 FragmentProgram (Interpolators i) : SV_TARGET {
                float4 invertedGrabPos = i.grabPos;
                invertedGrabPos.y = 1.0 - invertedGrabPos.y;

                float4 sum = 0;
                int samples = 2 * _KernelSize + 1;
                for (float x = 0; x < samples; x++)
                {
                    float2 offset = float2(x - _KernelSize, 0);
                    sum += tex2D(_GrabTexture, i.grabPos.xy / i.grabPos.w + offset * _GrabTexture_TexelSize.xy);
                } 
                //return the texture we just looked up
                if (tex2Dproj(_MainTex, invertedGrabPos).g != 0)
                    return tex2Dproj(_SceneTex, invertedGrabPos);
                return sum / samples + tex2Dproj(_SceneTex, invertedGrabPos);
			}
            ENDCG
        }
    }
    //end subshader
}
//end shader