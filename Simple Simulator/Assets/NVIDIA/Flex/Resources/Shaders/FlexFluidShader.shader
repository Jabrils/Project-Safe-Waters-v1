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

Shader "Flex/FlexFluidShader"
{
    Properties
    {
        _FluidColor("Fluid Color", Color) = (.34, .85, .92, 1)
    }

    SubShader
    {
        Tags{ "Queue" = "Overlay" }

        Pass
        {
            Name "FluidThickness"

            Cull off
            Blend One One
            ZWrite Off

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex VS_Main
            #pragma geometry GS_Main
            #pragma fragment FS_Main

            #include "UnityCG.cginc" 

            struct GS_INPUT
            {
                uint id : VERTEXID;
            };

            struct FS_INPUT
            {
                float4 pos  : POSITION;
                float2 tex0 : TEXCOORD0;
                float3 vpos : TEXCOORD1;
            };

            struct FS_OUTPUT
            {
                float c : COLOR0;
                float d : DEPTH0;
            };

            float _Size;
            StructuredBuffer<float4> _Particles;
            StructuredBuffer<uint> _Indices;

            GS_INPUT VS_Main(uint id : SV_VertexID)
            {
                GS_INPUT output = (GS_INPUT)0;
                output.id = id;
                return output;
            }

            [maxvertexcount(4)]
            void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
            {
                uint particle = p[0].id;

                float3 pos = _Particles[_Indices[particle]];
                float4x4 cam = transpose(unity_CameraToWorld);
                float3 up = cam[1];
                float3 right = cam[0];

                float halfS = 0.5f * _Size;

                float4 v[4] = { float4(pos + halfS * right - halfS * up, 1.0f),
                                float4(pos + halfS * right + halfS * up, 1.0f),
                                float4(pos - halfS * right - halfS * up, 1.0f),
                                float4(pos - halfS * right + halfS * up, 1.0f) };

                float4x4 vp = UNITY_MATRIX_VP;// mul(UNITY_MATRIX_MVP, unity_WorldToObject);
                FS_INPUT pIn;
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

            FS_OUTPUT FS_Main(FS_INPUT input)
            {
                FS_OUTPUT o = (FS_OUTPUT)0;

                float3 normal;
                normal.xy = input.tex0.xy * float2(2.0, -2.0) + float2(-1.0, 1.0);
                float mag = dot(normal.xy, normal.xy);
                if (mag > 1.0) discard;   // kill pixels outside circle
                normal.z = sqrt(1.0 - mag);
                float r = mag;
                float g = r * r * r * (r * (r * 6 - 15) + 10);
                float k = 0.5;

                o.c = (1 - g) * k;
                //return float4(((1 - g) * k).xxx, 1.0);//(1 - normal.z * 0.1);
                //return _SpriteTex.Sample(sampler_SpriteTex, input.tex0);

                float3 p = input.vpos - normal * _Size;// * 0.5 * _Size;

                float z = p.z;
                float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
                o.d = (1 / z - pw) / pz;


                return o;
            }

            ENDCG
        }


        Pass
        {
            Name "FluidDepth"

            Cull off
            ZTest Less

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex VS_Main
            #pragma geometry GS_Main
            #pragma fragment FS_Main

            #include "UnityCG.cginc" 

            struct GS_INPUT
            {
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

            float _Size;
            StructuredBuffer<float4> _Particles;
            StructuredBuffer<float4> _Q1;
            StructuredBuffer<float4> _Q2;
            StructuredBuffer<float4> _Q3;
            StructuredBuffer<uint> _Indices;

            GS_INPUT VS_Main(uint id : SV_VertexID)
            {
                GS_INPUT o = (GS_INPUT)0;
                o.id = id;
                return o;
            }

            [maxvertexcount(4)]
            void GS_Main(point GS_INPUT p[1], uint id : SV_PrimitiveId, inout TriangleStream<FS_INPUT> triStream)
            {
                float4 pos = float4(_Particles[_Indices[id]].xyz, 1);

                float4 q1 = mul(unity_WorldToCamera, float4(_Q1[_Indices[id]].xyz, 0)); q1.w = _Q1[_Indices[id]].w + 0.001;
                float4 q2 = mul(unity_WorldToCamera, float4(_Q2[_Indices[id]].xyz, 0)); q2.w = _Q2[_Indices[id]].w + 0.001;
                float4 q3 = mul(unity_WorldToCamera, float4(_Q3[_Indices[id]].xyz, 0)); q3.w = _Q3[_Indices[id]].w + 0.001;

                float scale = max(0.01, max(q1.w, max(q2.w, q3.w))) * 1.5;

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

            FS_OUTPUT FS_Main(FS_INPUT input)
            {
                FS_OUTPUT o = (FS_OUTPUT)0;

                float3x3 T = float3x3(input.q1.xyz * input.q1.w,
                                      input.q2.xyz * input.q2.w,
                                      input.q3.xyz * input.q3.w);
                T = transpose(T);
                float3x3 iT = float3x3(input.q1.xyz / input.q1.w,
                                       input.q2.xyz / input.q2.w, 
                                       input.q3.xyz / input.q3.w);

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
                o.c = z;//input.vpos.z - z;// * 0.5 * _Size;
                //float n = _ProjectionParams.y;
                //float f = _ProjectionParams.z;
                ////o.d = o.c / f;//(0.5 * (f + n) - f * n / o.c) / (f - n) + 0.5;
                //o.d = (0.5 * (f + n) - f * n / o.c) / (f - n) + 0.5;
                float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
                o.d = (1 / z - pw) / pz;

                return o;
            }

            ENDCG
        }

        Pass
        {
            Name "FluidDepthBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 5.0
            
            #include "UnityCG.cginc"

            struct VS_INPUT
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FS_INPUT
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            FS_INPUT vert (VS_INPUT v)
            {
                FS_INPUT o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _ThicknessTex;
            sampler2D _DepthTex;

            float sqr(float x) { return x*x; }

            float4 frag(FS_INPUT i) : COLOR0
            {
                float depth = tex2D(_DepthTex, i.uv).x;
                if (depth == _ProjectionParams.z) discard;
                float thickness = tex2D(_ThicknessTex, i.uv).x;

                //return depth.xxxx;

                float blurDepthFalloff = 0.0;
                float maxBlurRadius = 20.0;
                float blurScale = 20.0;
                float blurRadiusWorld = 2.0;

                //discontinuities between different tap counts are visible. to avoid this we 
                //use fractional contributions between #taps = ceil(radius) and floor(radius) 
                float radius = min(maxBlurRadius, blurScale * (blurRadiusWorld / depth));
                float radiusInv = 1.0 / radius;
                float taps = ceil(radius);
                float frac = taps - radius;

                //float taps = 50.0;
                float2 invScreen = float2(1 / _ScreenParams.x, 1 / _ScreenParams.y);

                float sum = 0.0;
                float wsum = 0.0;
                float count = 0.0;

                for (float x = -taps; x < taps; x += 1)
                {
                    for (float y = -taps; y < taps; y += 1)
                    {
                        float4 uv = float4(i.uv + float2(x, y) * invScreen, 0, 0);
                        float d1 = tex2Dlod(_DepthTex, uv).x;

                        if (d1 == _ProjectionParams.z) continue;
                        if (abs(d1 - depth) > 0.3) continue;

                        float r1 = length(float2(x, y)) / taps;
                        float w = exp(-(r1*r1));

                        // range domain (based on depth difference)
                        float r2 = (d1 - depth) * blurDepthFalloff;
                        float g = exp(-(r2*r2));

                        //fractional radius contributions
                        float wBoundary = step(radius, max(abs(x), abs(y)));
                        float wFrac = 1.0 - wBoundary*frac;

                        sum += d1 * w * g * wFrac;
                        wsum += w * g * wFrac;
                        count += g * wFrac;

                        //sum += d1 * w;
                        //wsum += w;
                        //count += 1;
                    }
                }

                if (wsum > 0.0)
                    sum /= wsum;

                float blend = count / sqr(2.0 * taps + 1.0);
                return lerp(depth, sum, blend);
            }
            ENDCG
        }

        Pass
        {
            Name "FluidCompose"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct VS_INPUT
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FS_INPUT
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct FS_OUTPUT
            {
                float4 c : COLOR0;
                float d : DEPTH0;
            };

            FS_INPUT vert (VS_INPUT v)
            {
                FS_INPUT o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 _FluidColor;

            sampler2D _FrameTex;
            sampler2D _ThicknessTex;
            sampler2D _DepthTex;
            UNITY_DECLARE_TEXCUBE(_ReflectionTex);

            float3 viewPosition(float2 uv, float d)
            {
                float2 unProject = float2(unity_CameraInvProjection[0][0], unity_CameraInvProjection[1][1]);
                float2 vp = unProject * (uv * 2.0 - 1.0);
                return float3(vp.xy * d, d);
            }

            float sqr(float x) { return x*x; }
            float cube(float x) { return x*x*x; }

            FS_OUTPUT frag(FS_INPUT i)
            {
                FS_OUTPUT o;
                float depth = tex2D(_DepthTex, i.uv).x;
                if (depth == _ProjectionParams.z) discard;
                //float4 main = tex2D(_FrameTex, i.uv);
                //float thickness = tex2D(_ThicknessTex, i.uv).x;
                if (tex2D(_ThicknessTex, i.uv).x < 0.001) discard;
                float3 fluid = float3(0.2, 0.7, 0.9);
                float z0 = depth;
                //float n = _ProjectionParams.y;
                //float f = _ProjectionParams.z;
                //float z = (0.5 * (f + n) - f * n / z0) / (f - n) + 0.5;
                //o.d = z;
                float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
                o.d = (1 / z0 - pw) / pz;
                float3 invScreen = float3(1 / _ScreenParams.x, 1 / _ScreenParams.y, 0);
                float3 dX0 = viewPosition(i.uv + invScreen.xz, tex2D(_DepthTex, i.uv + invScreen.xz).x);
                float3 dX1 = viewPosition(i.uv - invScreen.xz, tex2D(_DepthTex, i.uv - invScreen.xz).x);
                float3 dY0 = viewPosition(i.uv + invScreen.zy, tex2D(_DepthTex, i.uv + invScreen.zy).x);
                float3 dY1 = viewPosition(i.uv - invScreen.zy, tex2D(_DepthTex, i.uv - invScreen.zy).x);
                float3 normal = normalize(cross(dX1 - dX0, dY1 - dY0));
                //o.c = float4(normal.zzz*0.5+0.5, 1);
                //o.c = float4(main.xyz * (1 - thickness) + fluid * thickness, 1);
                //float3 vpos = viewPosition(i.uv, depth);
                //o.c = vpos.y;
                //o.c = float4(z, 0, 0, 1);

                float3 viewPos = viewPosition(i.uv, depth);

                float3 worldPos = mul(unity_CameraToWorld, float4(viewPos, 1));
                float3 worldNormal = mul(unity_CameraToWorld, float4(normal, 0));
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 worldRefl = reflect(-worldViewDir, worldNormal);

                //float3 reflection = reflect(normalize(viewPos), normal);

                // sample the default reflection cubemap, using the reflection vector
                //half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRefl);
                // decode cubemap data into actual color
                //half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);

                float refractScale = 0.025;
                float reflectScale = 0.1;

                float4 color = _FluidColor;// float4(0.2, 0.7, 0.8, 1.0);

                float2 refractCoord = i.uv + normal.xy * refractScale;
                float thickness = max(tex2D(_ThicknessTex, refractCoord).x * 0.5, 0.0) * color.w;
                float3 transmission = (1.0 - (1.0 - color.xyz) * thickness * 0.8);// *color.w;
                //float3 main = tex2D(_FrameTex, refractCoord).xyz * transmission;
                float3 main = tex2D(_FrameTex, refractCoord).xyz * transmission;

                // attenuate refraction near ground (hack)
                refractScale *= smoothstep(0.1, 0.4, worldPos.y);

                float3 reflColor = UNITY_SAMPLE_TEXCUBE(_ReflectionTex, worldRefl);

                float fresnel = 0.2 + 0.8 * cube(1.0 - max(dot(worldNormal, -worldViewDir), 0.5));

                //fresnel = 1;// saturate(fresnel) * 0.8;

                //float3 mixColor = main + reflColor * fresnel;// *color.w;
                float3 mixColor = lerp(main, reflColor, fresnel);// *color.w;
                //float3 mixColor = main * (1 - min(thickness * 0.6, 0.3)) + reflColor * fresnel;

                float3 lightDirection = normalize(float3(1,1,-1)); normalize(_WorldSpaceLightPos0.xyz);
                float attenuation = 1.0;

                //if (0.0 == _WorldSpaceLightPos0.w) // directional light?
                //{
                //    attenuation = 1.0; // no attenuation
                //    lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                //}
                //else // point or spot light
                //{
                //    float3 vertexToLightSource = _WorldSpaceLightPos0.xyz
                //        - mul(modelMatrix, input.vertex).xyz;
                //    float distance = length(vertexToLightSource);
                //    attenuation = 1.0 / distance; // linear attenuation 
                //    lightDirection = normalize(vertexToLightSource);
                //}

                //if (dot(worldNormal, lightDirection) > 0.0)
                {
                    float _Shininess = 20.0;
                    float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos);
                    float3 specularReflection = attenuation * float3(1,1,1) * pow(max(0.0, dot(reflect(-lightDirection, worldNormal), viewDirection)), _Shininess);
                    mixColor += specularReflection * 10.0;
                }

                //o.c = float4(worldRefl*0.5+0.5, 1);
                //o.c = skyData;
                //o.c = float4(normal.zzz * 0.5 + 0.5, 1);//
                o.c = float4(mixColor, 1);

                return o;
            }
            ENDCG
        }
    }

    CustomEditor "FlexFluidMaterialEditor"
}
