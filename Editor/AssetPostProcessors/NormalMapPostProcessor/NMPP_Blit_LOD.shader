Shader "Hidden/NormalMapPPBlitLOD"
{
    Properties
    {
        _MipSource("Mip Source", 2D) = "white" {}
        _Mip ("Texture Dimensions", Vector) = (2048, 2048, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
        LOD 100

        Pass
        {
            Name "Blit"
            ZWrite Off ZTest Always Blend Off Cull Off
            

            HLSLPROGRAM
            #pragma editor_sync_compilation
#pragma exclude_renderers gles gles3
            #pragma vertex FullscreenVert
            #pragma fragment Fragment
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
            
           

            TEXTURE2D(_MipSource);
            SAMPLER(sampler_Point_Clamp);
            float4 _Mip;
            float4 _ScaleBias;
            float4x4 unity_MatrixVP;

            CBUFFER_START(UnityPerDraw)
                float4x4 unity_ObjectToWorld;
            CBUFFER_END

#if _USE_DRAW_PROCEDURAL
            void GetProceduralQuad(in uint vertexID, out float4 positionCS, out float2 uv)
            {
                positionCS = GetQuadVertexPosition(vertexID);
                positionCS.xy = positionCS.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f);
                uv = GetQuadTexCoord(vertexID) * _ScaleBias.xy + _ScaleBias.zw;
            }
#endif

            struct Attributes
            {
#if _USE_DRAW_PROCEDURAL
                uint vertexID     : SV_VertexID;
#else
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
#endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };



            Varyings FullscreenVert(Attributes input)
            {
                Varyings output;
#if _USE_DRAW_PROCEDURAL
                output.positionCS = GetQuadVertexPosition(input.vertexID);
                output.positionCS.xy = output.positionCS.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f); //convert to -1..1
                output.uv = GetQuadTexCoord(input.vertexID) * _ScaleBias.xy + _ScaleBias.zw;
#else

                output.positionCS = mul(unity_MatrixVP, mul(unity_ObjectToWorld, float4(input.positionOS.xyz, 1)));
                output.uv = input.uv;
#endif
                return output;
            }

            Varyings Vert(Attributes input)
            {
                return FullscreenVert(input);
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
#ifdef UNITY_UV_STARTS_AT_TOP 
                clip(uv.y - 0.5);
#else
                clip(0.5 - uv.y);
#endif
                int uvXInt = uv.x * _Mip.x;
                int2 startCoord = int2(0, 0);
                int2 mipSize = int2(_Mip.xy) >> 1;
                int mipLevel = 1;
                int2 mipCoord = int2(0,0); 
                int maxMip = min(14, _Mip.z);
                for (int i = 0; i < maxMip; i++)
                {
                    mipCoord = int2(uv * float2(_Mip.xy));
                    if ((mipCoord.x - startCoord.x) < mipSize.x)
                    {
                        break;
                    }
                    startCoord.x += mipSize.x;
                    mipSize = max(mipSize >> 1, int2(1,1));
                    mipLevel++;
                }
                #ifdef UNITY_UV_STARTS_AT_TOP
                startCoord.y += _Mip.y - mipSize.y;
                #endif
                int2 uvCoord = mipCoord - startCoord;
                if (uvCoord.y < 0 || uvCoord.y >= mipSize.y)
                {
                    discard;
                }
                half4 col = LOAD_TEXTURE2D_LOD(_MipSource, uvCoord, mipLevel);
                #ifdef _LINEAR_TO_SRGB_CONVERSION
                col = LinearToSRGB(col);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
