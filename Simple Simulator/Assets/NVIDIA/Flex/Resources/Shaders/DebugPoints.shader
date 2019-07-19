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

Shader "Flex/DebugPoints"
{
    SubShader
    {
        Pass
        {
            Cull off

            //ZTest Less

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
            };

            struct FS_INPUT
            {
                float4 pos  : POSITION;
                float2 tex0 : TEXCOORD0;
                float3 vpos : TEXCOORD1;
                float mass : TEXCOORD2;
            };

            struct FS_OUTPUT
            {
                float4 c : COLOR0;
                float d : DEPTH0;
            };

            // **************************************************************
            // Vars                                                         *
            // **************************************************************

            StructuredBuffer<float4> _Points;
            StructuredBuffer<uint> _Indices;
            float _Radius;
            float3 _Color;

            // **************************************************************
            // Shader Programs                                              *
            // **************************************************************

            // Vertex Shader ------------------------------------------------
            GS_INPUT VS_Main(uint id : SV_VertexID)
            {
                GS_INPUT o = (GS_INPUT)0;
                o.id = id;
                return o;
            }

            // Geometry Shader -----------------------------------------------------
            [maxvertexcount(4)]
            void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
            {
                float4 pnt = _Points[_Indices[p[0].id]];
                float4 pos = float4(pnt.xyz, 1);

                float4x4 cam = transpose(unity_CameraToWorld);
                float3 up = cam[1];
                float3 right = cam[0];

                float halfS = _Radius * 1.5;

                float4 v[4];
                v[0] = float4(pos + halfS * right - halfS * up, 1.0f);
                v[1] = float4(pos + halfS * right + halfS * up, 1.0f);
                v[2] = float4(pos - halfS * right - halfS * up, 1.0f);
                v[3] = float4(pos - halfS * right + halfS * up, 1.0f);

                float4x4 vp = UNITY_MATRIX_VP;// mul(UNITY_MATRIX_MVP, unity_WorldToObject);

                FS_INPUT pIn;
                pIn.mass = pnt.w;

                pIn.pos = mul(vp, v[0]);
                pIn.tex0 = float2(1.0f, 0.0f);
                pIn.vpos = mul(unity_WorldToCamera, v[0]);
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

                float scale = 1.0 / 1.5;
                float iscale = 1.0 / scale;
                float3 p = float3(input.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0), 0) * 1.5;
                float3 d = normalize(input.vpos) / _Radius;

                float a = dot(d, d), b = 2 * dot(d, p), c = dot(p, p) - 1;

                float t = 0;
                if (a != 0 && b != 0)
                {
                    float D = b * b - 4 * a * c;
                    if (D < 0) discard;
                    t = (-b - sqrt(D)) / (2 * a);
                }
                else discard;

                float z = -d.z * t;

                float depth = input.vpos.z - z * _Radius;
                float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
                o.d = (1 / depth - pw) / pz;
                o.c = float4((z.xxx * 0.6 + 0.4) * _Color.xyz, 1) * (input.mass > 0 ? 1.0 : 0.5);

                return o;
            }

            ENDCG
        }
    }
}
