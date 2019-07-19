// This code contains NVIDIA Confidential Information and is disclosed to you
// under a form of NVIDIA software license agreement provided separately to you.
//
// Notice
// NVIDIA Corporation and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from NVIDIA Corporation is strictly prohibited.
//
// ALL NVIDIA DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". NVIDIA MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, NVIDIA Corporation assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of NVIDIA Corporation. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// NVIDIA Corporation products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// NVIDIA Corporation.
//
// Copyright (c) 2018 NVIDIA Corporation. All rights reserved.

Shader "Flex/FluidThickness"
{
    //Properties
    //{
    //    //_SpriteTex("Base (RGB)", 2D) = "white" {}
    //    //_Size("Size", Range(0, 3)) = 0.5
    //}

    SubShader
    {
        Pass
        {
            //Tags { "RenderType" = "Transparent" }//"Opaque" }
            //LOD 200

            Cull off

            //Blend DstColor Zero // Multiplicative
            Blend One One // Additive

            ZWrite Off

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex VS_Main
            #pragma geometry GS_Main
            #pragma fragment FS_Main

            #include "UnityCG.cginc" 

            // **************************************************************
            // Data structures                                              *
            // **************************************************************
            struct GS_INPUT
            {
                uint id : VERTEXID;
                //float4 pos : POSITION;
                //float2 tex0 : TEXCOORD0;
            };

            struct FS_INPUT
            {
                float4 pos  : POSITION;
                float2 tex0 : TEXCOORD0;
            };

            // **************************************************************
            // Vars                                                         *
            // **************************************************************

            float _Size;
            StructuredBuffer<float3> _Particles;
            StructuredBuffer<uint> _Phases;
            StructuredBuffer<uint> _Indices;

            // **************************************************************
            // Shader Programs                                              *
            // **************************************************************

            // Vertex Shader ------------------------------------------------
            GS_INPUT VS_Main(uint id : SV_VertexID)
            {
                GS_INPUT output = (GS_INPUT)0;

                output.id = id;
                //output.pos = float4(_Particles[_Indices[id]], 1);
                //output.pos = mul(unity_ObjectToWorld, v.vertex);
                //output.normal = v.normal;
                //output.tex0 = float2(0, 0);

                return output;
            }

            // Geometry Shader -----------------------------------------------------
            [maxvertexcount(4)]
            void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
            {
                if ((_Phases[_Indices[p[0].id]] & (1 << 26)) == 0) return;

                float3 pos = _Particles[_Indices[p[0].id]];
                //float3 up = float3(0, 1, 0);
                //float3 look = _WorldSpaceCameraPos - p[0].pos;
                //look.y = 0;
                //look = normalize(look);
                //float3 right = cross(up, look);
                float4x4 cam = transpose(unity_CameraToWorld);
                float3 up = cam[1];
                float3 right = cam[0];

                float halfS = 0.5f * _Size;

                float4 v[4];
                v[0] = float4(pos + halfS * right - halfS * up, 1.0f);
                v[1] = float4(pos + halfS * right + halfS * up, 1.0f);
                v[2] = float4(pos - halfS * right - halfS * up, 1.0f);
                v[3] = float4(pos - halfS * right + halfS * up, 1.0f);

                float4x4 vp = UNITY_MATRIX_VP;// mul(UNITY_MATRIX_MVP, unity_WorldToObject);
                FS_INPUT pIn;
                pIn.pos = mul(vp, v[0]);
                pIn.tex0 = float2(1.0f, 0.0f);
                triStream.Append(pIn);

                pIn.pos = mul(vp, v[1]);
                pIn.tex0 = float2(1.0f, 1.0f);
                triStream.Append(pIn);

                pIn.pos = mul(vp, v[2]);
                pIn.tex0 = float2(0.0f, 0.0f);
                triStream.Append(pIn);

                pIn.pos = mul(vp, v[3]);
                pIn.tex0 = float2(0.0f, 1.0f);
                triStream.Append(pIn);
            }

            // Fragment Shader -----------------------------------------------
            float FS_Main(FS_INPUT input) : COLOR
            {
                float3 normal;
                normal.xy = input.tex0.xy * float2(2.0, -2.0) + float2(-1.0, 1.0);
                float mag = dot(normal.xy, normal.xy);
                if (mag > 1.0) discard;   // kill pixels outside circle
                normal.z = sqrt(1.0 - mag);
                float r = mag;
                float g = r * r * r * (r * (r * 6 - 15) + 10);
                float k = 0.0025;

                return (1 - g) * k;
                //return float4(((1 - g) * k).xxx, 1.0);//(1 - normal.z * 0.1);
                //return _SpriteTex.Sample(sampler_SpriteTex, input.tex0);
            }

            ENDCG
        }
    }
}
