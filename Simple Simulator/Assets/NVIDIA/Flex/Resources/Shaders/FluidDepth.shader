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

Shader "Flex/FluidDepth"
{
    //Properties
    //{
    //    //_SpriteTex("Base (RGB)", 2D) = "white" {}
    //    //_Size("Size", Range(0, 3)) = 0.5
    //    //_Size("Size", Range(0, 100)) = 0.5
    //}

    SubShader
    {
        Pass
        {
            //Tags { "RenderType" = "Transparent" }//"Opaque" }
            //LOD 200

            Cull off

            ZTest Less

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
                //float4 pos : POSITION;
                uint id : VERTEXID;
            };

            struct FS_INPUT
            {
                float4 pos  : POSITION;
                float2 tex0 : TEXCOORD0;
                float3 vpos : TEXCOORD1;
                float4 q1   : TEXCOORD2;
                float4 q2   : TEXCOORD3;
                float4 q3   : TEXCOORD4;
                float scale : TEXCOORD5;
            };

            struct FS_OUTPUT
            {
                float c : COLOR0;
                float d : DEPTH0;
            };

            // **************************************************************
            // Vars                                                         *
            // **************************************************************

            float _Size;
            StructuredBuffer<float3> _Particles;
            StructuredBuffer<float4> _Q1;
            StructuredBuffer<float4> _Q2;
            StructuredBuffer<float4> _Q3;
            StructuredBuffer<uint> _Phases;
            StructuredBuffer<uint> _Indices;

            // **************************************************************
            // Shader Programs                                              *
            // **************************************************************

            // Vertex Shader ------------------------------------------------
            GS_INPUT VS_Main(uint id : SV_VertexID)
            {
                GS_INPUT o = (GS_INPUT)0;
                //o.pos = float4(_Particles[id], 1);
                o.id = id;
                return o;
            }

            // Geometry Shader -----------------------------------------------------
            [maxvertexcount(4)]
            void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
            {
                if ((_Phases[_Indices[p[0].id]] & (1 << 26)) == 0) return;

                float4 pos = float4(_Particles[_Indices[p[0].id]], 1);

                float4 q1 = mul(unity_WorldToCamera, float4(_Q1[_Indices[p[0].id]].xyz, 0)); q1.w = _Q1[_Indices[p[0].id]].w + 0.001;
                float4 q2 = mul(unity_WorldToCamera, float4(_Q2[_Indices[p[0].id]].xyz, 0)); q2.w = _Q2[_Indices[p[0].id]].w + 0.001;
                float4 q3 = mul(unity_WorldToCamera, float4(_Q3[_Indices[p[0].id]].xyz, 0)); q3.w = _Q3[_Indices[p[0].id]].w + 0.001;

                float scale = max(0.01, max(q1.w, max(q2.w, q3.w))) * 1.1;
                //if (scale < 0.001) return;

                q1.w /= scale; q2.w /= scale; q3.w /= scale;

                float4x4 cam = transpose(unity_CameraToWorld);
                float3 up = cam[1];
                float3 right = cam[0];

                float halfS = scale;//1.0;//0.5f;// * _Size;

                float4 v[4];
                v[0] = float4(pos + halfS * right - halfS * up, 1.0f);
                v[1] = float4(pos + halfS * right + halfS * up, 1.0f);
                v[2] = float4(pos - halfS * right - halfS * up, 1.0f);
                v[3] = float4(pos - halfS * right + halfS * up, 1.0f);

                //float4 q1 = _Q1[p[0].id];
                //float4 q2 = _Q2[p[0].id];
                //float4 q3 = _Q3[p[0].id];

                float4x4 vp = UNITY_MATRIX_VP;// mul(UNITY_MATRIX_MVP, unity_WorldToObject);
                FS_INPUT pIn;
                pIn.pos = mul(vp, v[0]);
                pIn.tex0 = float2(1.0f, 0.0f);
                pIn.vpos = mul(unity_WorldToCamera, v[0]);
                pIn.q1 = q1; pIn.q2 = q2; pIn.q3 = q3;
                pIn.scale = scale;
                triStream.Append(pIn);

                pIn.pos = mul(vp, v[1]);
                pIn.tex0 = float2(1.0f, 1.0f);
                pIn.vpos = mul(unity_WorldToCamera, v[1]);
                triStream.Append(pIn);

                pIn.pos = mul(vp, v[2]);
                pIn.tex0 = float2(0.0f, 0.0f);
                pIn.vpos = mul(unity_WorldToCamera, v[2]);
                triStream.Append(pIn);

                pIn.pos = mul(vp, v[3]);
                pIn.tex0 = float2(0.0f, 1.0f);
                pIn.vpos = mul(unity_WorldToCamera, v[3]);
                triStream.Append(pIn);
            }

            // Fragment Shader -----------------------------------------------
            FS_OUTPUT FS_Main(FS_INPUT input)
            {
                FS_OUTPUT o = (FS_OUTPUT)0;
                //o.c = 0;
                //return o;

                //input.q1.w = 0.08;
                //input.q2.w = 0.02;
                //input.q3.w = 0.08;

////////////////////////

                //float3x3 R = float3x3(input.q1.xyz,
                //                      input.q2.xyz,
                //                      input.q3.xyz);
                //float3x3 S = float3x3(input.q1.w, 0, 0,
                //                      0, input.q2.w, 0,
                //                      0, 0, input.q3.w);
                //float3x3 iR = transpose(R);
                //float3x3 iS = float3x3(1 / input.q1.w, 0, 0,
                //                       0, 1 / input.q2.w, 0,
                //                       0, 0, 1 / input.q3.w);

                float3x3 T = float3x3(input.q1.xyz * input.q1.w,
                                      input.q2.xyz * input.q2.w,
                                      input.q3.xyz * input.q3.w);
                T = transpose(T);
                float3x3 iT = float3x3(input.q1.xyz / input.q1.w,
                                       input.q2.xyz / input.q2.w, 
                                       input.q3.xyz / input.q3.w);
                //iT = transpose(iT);

                float3 p = float3(input.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0), 0);
                float3 d = normalize(input.vpos);

                p = mul(iT, p);
                d = mul(iT, d);

                float a = dot(d, d), b = 2 * dot(d, p), c = dot(p, p) - 1;

                float t = 0;
                if (a != 0 && b != 0)
                {
                    float D = b * b - 4 * a * c;
                    if (D < 0) discard;
                    t = (-b - sqrt(D)) / (2 * a);
                }
                else discard;

                p = input.vpos + mul(T, d * t) * input.scale;// * 0.5 * _Size;

                float z = p.z;

/////////////////////////////

                //float3 i = input.q1.xyz;
                //float3 j = input.q2.xyz;
                //float3 k = input.q3.xyz;

                //float3 abc = float3(input.q1.w, input.q2.w, input.q3.w) * 1;
                ////float3 abc = float3(1, 1, 1);
                //float3 iabc = 1.0 / abc;

                //float2 xy = input.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0);
                ////float3 p = float3(i.x * xy.x + i.y * xy.y, j.x * xy.x + j.y * xy.y, k.x * xy.x + k.y * xy.y) * iabc;
                ////float3 d = float3(i.z, j.z, k.z) * iabc;
                //float3 p = float3(i.x * xy.x + j.x * xy.y, i.y * xy.x + j.y * xy.y, i.z * xy.x + j.z * xy.y) * iabc;
                //float3 d = float3(k.x, k.y, k.z) * iabc;

                //float a = dot(d, d), b = 2 * dot(d, p), c = dot(p, p) - 1;

                //float D = b * b - 4 * a * c;

                //if (D < 0) discard;

                //float t = -0.5 * (b - sqrt(D)) / a;

                //p += d * t;

                ////float z = abc.x * p.x * i.z + abc.y * p.y * j.z + abc.z * p.z * k.z;
                //float z = abc.x * p.x * k.x + abc.y * p.y * k.y + abc.z * p.z * k.z;
                ////float z = sqrt(1 - dot(xy, xy));

                //z = input.vpos.z - z;

//////////////////////

                //float2 n_xy;
                //n_xy = input.tex0.xy * float2(2.0, -2.0) + float2(-1.0, 1.0);
                //float mag = dot(n_xy, n_xy);
                //if (mag > 1.0) discard;   // kill pixels outside circle
                //float n_z = z;// * _Size;//sqrt(1.0 - mag) * 0.5 * _Size;
                o.c = z;//input.vpos.z - z;// * 0.5 * _Size;
                float n = _ProjectionParams.y;
                float f = _ProjectionParams.z;
                //o.d = o.c / f;//(0.5 * (f + n) - f * n / o.c) / (f - n) + 0.5;
                o.d = (0.5 * (f + n) - f * n / o.c) / (f - n) + 0.5;

                return o;
            }

            ENDCG
        }
    }
}
