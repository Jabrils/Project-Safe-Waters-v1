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

Shader "Flex/FluidDepthBlur"
{
    SubShader
    {
        Cull Off
        ZWrite On
        ZTest LEqual

        Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0
            
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

                //float blurDepthFalloff = 5.5;
                //float maxBlurRadius = 5.0;

                //float blurRadiusWorld;
                //float blurScale;
                //float blurFalloff;
                //float2 invTexScale;
                //float3 invScreen = float3(1 / _ScreenParams.x, 1 / _ScreenParams.y, 0);


                //float radius = min(maxBlurRadius, blurScale * (blurRadiusWorld / -depth));
                //float radiusInv = 1.0 / radius;
                //float taps = ceil(radius);
                //float frac = taps - radius;

                float taps = 5.0;
                float2 invScreen = float2(1 / _ScreenParams.x, 1 / _ScreenParams.y);

                float sum = 0.0;
                float wsum = 0.0;
                float count = 0.0;

                [unroll(5)] for (float x = -taps; x < taps; x += 1)
                {
                    [unroll(5)] for (float y = -taps; y < taps; y += 1)
                    {
                        float4 uv = float4(i.uv + float2(x, y) * invScreen, 0, 0);
                        float d1 = tex2Dlod(_DepthTex, uv).x;

                        //if (d1 == _ProjectionParams.z) continue;
                        if (abs(d1 - depth) > 0.5) continue;

                        float r1 = length(float2(x, y)) / taps;
                        float w = exp(-(r1*r1));

                        sum += d1 * w;
                        wsum += w;
                        count += 1;
                    }
                }

                if (wsum > 0.0)
                    sum /= wsum;

                float blend = count / sqr(2.0 * taps + 1.0);
                return lerp(depth, sum, blend);
            }
            ENDCG
        }
    }
}
