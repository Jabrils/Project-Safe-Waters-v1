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

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Flex/FlexPrepareFluid"
{
    SubShader
    {
        Pass
        {
            Name "FluidThickness"

            Cull off
            Blend One One
            ZWrite Off

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            StructuredBuffer<uint> _Indices;
            StructuredBuffer<float4> _Positions;
            float4x4 _ViewMatrix;
            float4x4 _ProjMatrix;
            float _Scale;

            struct v2g {};

            struct g2f
            {
                float4 pos : SV_Position;
                float3 vpos : VIEWPOS;
                float2 tex0 : TEX0;
                float scale : SCALE;
            };

            struct f2o
            {
                float color : SV_Target;
                //float depth : SV_Depth;
            };

            v2g vert(uint _id : SV_VertexId)
            {
                return (v2g)0;
            }

            [maxvertexcount(4)]
            void geom(point v2g _p[1], uint _id : SV_PrimitiveId, inout TriangleStream<g2f> _stream)
            {
                uint index = _Indices[_id];
                float3 pos = mul(_ViewMatrix, float4(_Positions[index].xyz, 1));

                //float4 ani1 = _Anisotropy1[index];
                //float4 ani2 = _Anisotropy2[index];
                //float4 ani3 = _Anisotropy3[index];
                //ani1 = float4(mul(_ViewMatrix, ani1.xyz).xyz, ani1.w + 0.001);
                //ani2 = float4(mul(_ViewMatrix, ani2.xyz).xyz, ani2.w + 0.001);
                //ani3 = float4(mul(_ViewMatrix, ani3.xyz).xyz, ani3.w + 0.001);
                //float scale = max(0.01, max(ani1.w, max(ani2.w, ani3.w))) * 1.5;
                //ani1.w /= scale; ani2.w /= scale; ani3.w /= scale;

                float scale = _Scale;

                float3 up = float3(0, 1, 0), right = float3(1, 0, 0);
                float3 vpos[4] = { pos + ( right - up) * scale, pos + ( right + up) * scale,
                                   pos + (-right - up) * scale, pos + (-right + up) * scale };

                float2 tex0[4] = { float2(1, 0), float2(1, 1),
                                   float2(0, 0), float2(0, 1) };

                g2f o = (g2f)0;
                //o.ani1 = ani1; o.ani2 = ani2; o.ani3 = ani3;
                o.scale = scale;

                for (int i = 0; i < 4; ++i)
                {
                    o.pos = mul(_ProjMatrix, float4(vpos[i], 1));
                    o.vpos = vpos[i];
                    o.tex0 = tex0[i];
                    _stream.Append(o);
                }
            }

            f2o frag(g2f _i)
            {
                f2o o = (f2o)0;

                float3 normal;
                normal.xy = _i.tex0.xy * float2(2.0, -2.0) + float2(-1.0, 1.0);
                float mag = dot(normal.xy, normal.xy);
                if (mag > 1.0) discard;   // kill pixels outside circle
                normal.z = sqrt(1.0 - mag);
                float r = mag;
                float g = r * r * r * (r * (r * 6 - 15) + 10);
                float k = _Scale * 0.5;

                o.color = (1 - g) * k;


                //float3x3 T = transpose(float3x3(_i.ani1.xyz * _i.ani1.w, _i.ani2.xyz * _i.ani2.w, _i.ani3.xyz * _i.ani3.w));
                //float3x3 iT = float3x3(_i.ani1.xyz / _i.ani1.w, _i.ani2.xyz / _i.ani2.w, _i.ani3.xyz / _i.ani3.w);

                //float3 p = float3(_i.tex0 * float2(2.0, 2.0) - float2(1.0, 1.0), 0);
                //float3 d = normalize(_i.vpos);

                //p = mul(iT, p);
                //d = mul(iT, d);

                //float a = dot(d, d), b = 2 * dot(d, p), c = dot(p, p) - 1;

                //float t = 0;
                //if (a != 0 && b != 0)
                //{
                //    float D = b * b - 4 * a * c;
                //    if (D < 0) discard;
                //    t = (-b - sqrt(D)) / (2 * a);
                //}
                //else discard;

                //float3 vpos = _i.vpos + mul(T, d * t) * _i.scale;
                //float4 pos = mul(_ProjMatrix, float4(vpos, 1));

                //o.color = vpos.z;
                //o.depth = pos.z / pos.w;

                return o;
            }

            ENDCG
        }

        Pass
        {
            Name "FluidDepth"

            Cull Off
            ZTest Always
            Blend One One
            BlendOp Min

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            StructuredBuffer<uint> _Indices;
            StructuredBuffer<float4> _Positions;
            StructuredBuffer<float4> _Anisotropy1;
            StructuredBuffer<float4> _Anisotropy2;
            StructuredBuffer<float4> _Anisotropy3;
            float4x4 _ViewMatrix[2];
            float4x4 _ProjMatrix[2];
            int _EyeCount;

            struct v2g {};

            struct g2f
            {
                float4 pos : SV_Position;
                float3 vpos : VIEWPOS;
                float2 tex0 : TEX0;
                float4 ani1 : ANI1;
                float4 ani2 : ANI2;
                float4 ani3 : ANI3;
                float scale : SCALE;
                int eye : EYE;
            };

            struct f2o
            {
                float2 color : SV_Target;
                //float depth : SV_Depth;
            };

            v2g vert(uint _id : SV_VertexId)
            {
                return (v2g)0;
            }

            [maxvertexcount(8)]
            void geom(point v2g _p[1], uint _id : SV_PrimitiveId, inout TriangleStream<g2f> _stream)
            {
                uint index = _Indices[_id];
                float3 mpos = _Positions[index].xyz;

                float4 ani01 = _Anisotropy1[index];
                float4 ani02 = _Anisotropy2[index];
                float4 ani03 = _Anisotropy3[index];

                for (int eye = 0; eye < _EyeCount; ++eye)
                {
                    float4x4 viewMatrix = _ViewMatrix[eye];
                    float3 pos = mul(viewMatrix, float4(mpos, 1));
                    float4 ani1 = float4(mul(viewMatrix, ani01.xyz).xyz, ani01.w + 0.001);
                    float4 ani2 = float4(mul(viewMatrix, ani02.xyz).xyz, ani02.w + 0.001);
                    float4 ani3 = float4(mul(viewMatrix, ani03.xyz).xyz, ani03.w + 0.001);
                    float scale = max(0.01, max(ani1.w, max(ani2.w, ani3.w))) * 1.5;
                    ani1.w /= scale; ani2.w /= scale; ani3.w /= scale;

                    float3 up = float3(0, 1, 0), right = float3(1, 0, 0);
                    float3 vpos[4] = { pos + (right - up) * scale, pos + (right + up) * scale,
                                       pos + (-right - up) * scale, pos + (-right + up) * scale };

                    float2 tex0[4] = { float2(1, 0), float2(1, 1),
                                       float2(0, 0), float2(0, 1) };

                    g2f o = (g2f)0;
                    o.ani1 = ani1; o.ani2 = ani2; o.ani3 = ani3;
                    o.scale = scale;
                    o.eye = eye;

                    for (int i = 0; i < 4; ++i)
                    {
                        o.pos = mul(_ProjMatrix[eye], float4(vpos[i], 1));
                        o.vpos = vpos[i];
                        o.tex0 = tex0[i];
                        _stream.Append(o);
                    }

                    _stream.RestartStrip();
                }
            }

            f2o frag(g2f _i)
            {
                f2o o = (f2o)0;

                float3x3 T = transpose(float3x3(_i.ani1.xyz * _i.ani1.w, _i.ani2.xyz * _i.ani2.w, _i.ani3.xyz * _i.ani3.w));
                float3x3 iT = float3x3(_i.ani1.xyz / _i.ani1.w, _i.ani2.xyz / _i.ani2.w, _i.ani3.xyz / _i.ani3.w);

                float3 p = float3(_i.tex0 * float2(2.0, 2.0) - float2(1.0, 1.0), 0);
                float3 d = normalize(_i.vpos);

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

                float3 vpos = _i.vpos + mul(T, d * t) * _i.scale;
                float3 vpos1 = _i.vpos - mul(T, d * t) * _i.scale;
                //float4 pos = mul(_ProjMatrix, float4(vpos, 1));

                float2 mask = _i.eye == 0 ? float2(1, 1e38) : float2(1e38, 1);

                o.color = -vpos.z * mask;
                //o.color = float2(-vpos.z, vpos1.z) * mask;
                //o.depth = pos.z / pos.w;

                return o;
            }

            ENDCG
        }

        Pass
        {
            Name "FluidDepthBlur"

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct i2v
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct f2o
            {
                float2 color : SV_Target;
            };

            float2 _InvScreen;
            float _FarPlane;
            sampler2D _DepthTex;

            float sqr(float x) { return x * x; }

            v2f vert(i2v _i)
            {
                v2f o = (v2f)0;
                o.pos = UnityObjectToClipPos(_i.pos);
                o.uv = _i.uv;
                return o;
            }

            f2o frag(v2f _i)
            {
                f2o o = (f2o)0;

                float2 depth = tex2D(_DepthTex, _i.uv).xy;
                if (depth[0] == _FarPlane && depth[1] == _FarPlane) discard;
                //float thickness = 1;//tex2D(_ThicknessTex, i.uv).x;

                //float2 depthSign = (float2)1;// sign(depth);
                //depth *= depthSign;

                float blurDepthFalloff = 0.0;
                float maxBlurRadius = 20.0;
                float blurScale = 20.0;
                float blurRadiusWorld = 2.0;

                //discontinuities between different tap counts are visible. to avoid this we 
                //use fractional contributions between #taps = ceil(radius) and floor(radius) 
                float minDepth = min(depth[0], depth[1]);
                float radius = min(maxBlurRadius, blurScale * (blurRadiusWorld / minDepth));
                float radiusInv = 1.0 / radius;
                float taps = ceil(radius);
                //float frac = taps - radius;

                float2 invScreen = _InvScreen;

                float2 sum = 0.0;
                float2 wsum = 0.0;
                float2 count = 0.0;

                for (float x = -taps; x < taps; x += 1)
                {
                    for (float y = -taps; y < taps; y += 1)
                    {
                        float4 uv = float4(_i.uv + float2(x, y) * invScreen, 0, 0);
                        float2 d1 = tex2Dlod(_DepthTex, uv).xy;// *depthSign;

                        //if (d1.x == _FarPlane && d1.y == _FarPlane) continue;
                        //if (abs(d1.x - depth.x) > 0.3 && abs(d1.y - depth.y) > 0.3) continue;

                        float r1 = length(float2(x, y)) / taps;
                        float w = exp(-sqr(r1));

                        [unroll]
                        for (int e = 0; e < 2; ++e)
                        {
                            if (depth[e] < _FarPlane && d1[e] < _FarPlane && abs(d1[e] - depth[e]) < 0.3)
                            {
                                sum[e] += d1[e] * w;
                                wsum[e] += w;
                                count[e] += 1;
                            }
                        }

                        ////// range domain (based on depth difference)
                        ////float r2 = (d1 - depth) * blurDepthFalloff;
                        ////float g = 1;//exp(-sqr(r2));

                        //////fractional radius contributions
                        ////float wBoundary = step(radius, max(abs(x), abs(y)));
                        ////float wFrac = 1;//1.0 - wBoundary * frac;

                        //sum += d1 * w;// * g * wFrac;
                        //wsum += w;// * g * wFrac;
                        //count += 1;//g * wFrac;

                        ////sum += d1 * w;
                        ////wsum += w;
                        ////count += 1;
                    }
                }

                if (wsum[0] > 0.0) sum[0] /= wsum[0];
                if (wsum[1] > 0.0) sum[1] /= wsum[1];

                float2 blend = count / sqr(2.0 * taps + 1.0);
                o.color = lerp(depth, sum, blend);// *depthSign; // @@@
                //o.color = sum;// *depthSign; // @@@

                return o;
            }

            ENDCG
        }
    }
}
