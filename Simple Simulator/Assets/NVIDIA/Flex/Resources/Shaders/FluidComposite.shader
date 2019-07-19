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

Shader "Flex/FluidComposite"
{
    //Properties
    //{
    //    //_FrameTex ("Texture", 2D) = "white" {}
    //    //_ThicknessTex ("Thickness", 2D) = "white" {}
    //    //_DepthTex ("Depth", 2D) = "white" {}
    //}

    SubShader
    {
        Cull Off
        ZWrite On
        ZTest LEqual

        //Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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
                float3 fluid = float3(0.2, 0.7, 0.9);
                float z0 = depth;
                float n = _ProjectionParams.y;
                float f = _ProjectionParams.z;
                float z = (0.5 * (f + n) - f * n / z0) / (f - n) + 0.5;
                o.d = z;
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

                float4 color = float4(0.2, 0.7, 0.8, 1.0);

                float2 refractCoord = i.uv + normal.xy * refractScale;
                float thickness = max(tex2D(_ThicknessTex, refractCoord).x * 0.5, 0.0);
                float3 transmission = (1.0 - (1.0 - color.xyz)*thickness*0.8)*color.w;
                float3 main = tex2D(_FrameTex, refractCoord).xyz * transmission;

                // attenuate refraction near ground (hack)
                refractScale *= smoothstep(0.1, 0.4, worldPos.y);

                float3 reflColor = UNITY_SAMPLE_TEXCUBE(_ReflectionTex, worldRefl);

                float fresnel = 0.1 + (1.0 - 0.1)*cube(1.0 - max(dot(worldNormal, -worldViewDir), 0.0));

                float3 mixColor = (lerp(main, reflColor, fresnel))*color.w;
                //float3 mixColor = main * float3(1 - min(thickness * 0.6, 0.3), 1, 1) + reflColor * fresnel;

                //o.c = float4(worldRefl*0.5+0.5, 1);
                //o.c = skyData;
                //o.c = float4(normal.zzz * 0.5 + 0.5, 1);
                o.c = float4(mixColor, 1);

                return o;
            }
            ENDCG
        }
    }
}
