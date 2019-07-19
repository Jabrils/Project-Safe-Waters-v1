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

// UNITY_SHADER_NO_UPGRADE

Shader "Flex/SourcePreview"
 {
     Properties 
     {
         _WireColor("WireColor", Color) = (0.2,0.2,0.2,1)
         _Color("Color", Color) = (1,1,1,1)
     }
     SubShader 
     {
         Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

         Pass 
         {
             Blend SrcAlpha OneMinusSrcAlpha
         
             CGPROGRAM
             #include "UnityCG.cginc"
             #pragma target 5.0
             #pragma vertex vert
             #pragma geometry geom
             #pragma fragment frag
             
             
             half4 _WireColor, _Color;
         
             struct v2g 
             {
                 float4  pos : SV_POSITION;
                 float2  uv : TEXCOORD0;
                 float3  normal : NORMAL;
             };
             
             struct g2f 
             {
                 float4  pos : SV_POSITION;
                 float2  uv : TEXCOORD0;
                 float3 dist : TEXCOORD1;
                 float3  normal : NORMAL;
             };
 
             v2g vert(appdata_base v)
             {
                 v2g OUT;
                 OUT.pos = UnityObjectToClipPos(v.vertex);
                 OUT.uv = v.texcoord; //the uv's arent used in this shader but are included in case you want to use them
                 OUT.normal = mul(UNITY_MATRIX_MV, v.normal);
                 return OUT;
             }
             
             [maxvertexcount(3)]
             void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
             {
             
                 float2 WIN_SCALE = float2(_ScreenParams.x/2.0, _ScreenParams.y/2.0);
                 
                 //frag position
                 float2 p0 = WIN_SCALE * IN[0].pos.xy / IN[0].pos.w;
                 float2 p1 = WIN_SCALE * IN[1].pos.xy / IN[1].pos.w;
                 float2 p2 = WIN_SCALE * IN[2].pos.xy / IN[2].pos.w;
                 
                 //barycentric position
                 float2 v0 = p2-p1;
                 float2 v1 = p2-p0;
                 float2 v2 = p1-p0;
                 //triangles area
                 float area = abs(v1.x*v2.y - v1.y * v2.x);
             
                 g2f OUT;
                 OUT.pos = IN[0].pos;
                 OUT.uv = IN[0].uv;
                 OUT.dist = float3(area/length(v0),0,0);
                 OUT.normal = IN[0].normal;
                 triStream.Append(OUT);
 
                 OUT.pos = IN[1].pos;
                 OUT.uv = IN[1].uv;
                 OUT.dist = float3(0,area/length(v1),0);
                 OUT.normal = IN[1].normal;
                 triStream.Append(OUT);
 
                 OUT.pos = IN[2].pos;
                 OUT.uv = IN[2].uv;
                 OUT.dist = float3(0,0,area/length(v2));
                 OUT.normal = IN[2].normal;
                 triStream.Append(OUT);
                 
             }
             
             half4 frag(g2f IN) : COLOR
             {
                 //distance of frag from triangles center
                 float d = min(IN.dist.x, min(IN.dist.y, IN.dist.z));
                 //fade based on dist from center
                  float I = exp2(-4.0*d*d);
                  half4 c = lerp(_Color, _WireColor, I);
                  c.xyz *= normalize(IN.normal).z * 0.7 + 0.3;
                  return c;
                  //return lerp(_Color, _WireColor, I) * (IN.normal.z * 0.7 + 0.3);            
             }
             
             ENDCG
 
         }
     }
 }






//Shader "Flex/SourcePreview"
//{
//    SubShader
//    {
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            
//            #include "UnityCG.cginc"
//
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float3 normal : NORMAL;
//            };
//
//            struct v2f
//            {
//                float4 vertex : SV_POSITION;
//                float3 normal : TEXCOORD0;
//            };
//
//            v2f vert (appdata v)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.normal = mul(UNITY_MATRIX_MV, float4(v.normal, 0));
//                return o;
//            }
//            
//            float4 frag (v2f i) : COLOR
//            {
//                float3 normal = normalize(i.normal);
//                return (normal.z * 0.7 + 0.3).xxxx;
//            }
//            ENDCG
//        }
//    }
//}
