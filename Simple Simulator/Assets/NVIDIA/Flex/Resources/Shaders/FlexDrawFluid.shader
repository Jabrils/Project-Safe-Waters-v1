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

Shader "Flex/FlexDrawFluid" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 1.0
        _Metallic ("Metallic", Range(0,1)) = 1.0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }//AlphaTest//Transparent//Overlay//Geometry
        LOD 200
        
        
    // ------------------------------------------------------------
    // Surface shader code generated out of a CGPROGRAM block:

    //GrabPass { "_MainTex" }

    // ---- forward rendering base pass:
    Pass {
        Name "FORWARD"
        Tags { "LightMode" = "ForwardBase" }
        Cull off

CGPROGRAM
// compile directives
#pragma vertex vert_flex
#pragma geometry geom_flex
#pragma fragment frag_flex
//#pragma vertex vert_surf
//#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 1 texcoords actually used
//   float2 _MainTex
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 10 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            //o.Emission = c.rgb;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            //o.Occlusion = 0.5;
        }
        

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0; // _MainTex
  half3 worldNormal : TEXCOORD1;
  float3 worldPos : TEXCOORD2;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD3; // SH
  #endif
  SHADOW_COORDS(4)
  UNITY_FOG_COORDS(5)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD6;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0; // _MainTex
  half3 worldNormal : TEXCOORD1;
  float3 worldPos : TEXCOORD2;
  float4 lmap : TEXCOORD3;
  SHADOW_COORDS(4)
  UNITY_FOG_COORDS(5)
  #ifdef DIRLIGHTMAP_COMBINED
  fixed3 tSpace0 : TEXCOORD6;
  fixed3 tSpace1 : TEXCOORD7;
  fixed3 tSpace2 : TEXCOORD8;
  #endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf(appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  #endif
  #if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #endif
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
  #ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

// flex vertex/geometry shader
struct v2g_flex
{
    appdata_full i2v;
};

v2g_flex vert_flex(appdata_full v)
{
    v2g_flex o = (v2g_flex)0;
    o.i2v = v;
    return o;
}

StructuredBuffer<float4> _Points;
StructuredBuffer<uint> _Indices;
//sampler2D _FrameTex;
sampler2D _DepthTex;
//float _Radius;

struct g2f_flex
{
    v2f_surf v2f;
    float2 tex0 : TEX0;
};

[maxvertexcount(4)]
void geom_flex(point v2g_flex IN[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f_flex> OUT)
{
    if (id != 0) return;

    float4 pos[4] = { float4( 1,  1, 0, 1), float4( 1, -1, 0, 1), 
                      float4(-1,  1, 0, 1), float4(-1, -1, 0, 1), };

    float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
                       float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

    appdata_full i2v = IN[0].i2v;

    g2f_flex o = (g2f_flex)0;

    for (int i = 0; i < 4; ++i)
    {
        o.v2f = vert_surf(i2v);
        o.v2f.pos = pos[i];
        o.tex0 = tex0[i];
        OUT.Append(o);
    }
}

// fragment shader
fixed4 frag_surf(v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  #if !defined(LIGHTMAP_ON)
      gi.light.color = _LightColor0.rgb;
      gi.light.dir = lightDir;
  #endif
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #if UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}

// flex fragment shader
struct f2o_flex
{
    float4 c : SV_Target;
    float d : SV_Depth;
};

float3 view_position(float2 uv)
{
    float3 vdir = mul(unity_CameraInvProjection, float4(uv * 2.0 - 1.0, 0, 1));
    float dist = -tex2D(_DepthTex, uv).x;
    return (vdir / vdir.z) * dist;
}

f2o_flex frag_flex(g2f_flex IN)
{
    f2o_flex o = (f2o_flex)0;

    float3 vpos = view_position(IN.tex0);
    if (-vpos.z == _ProjectionParams.z) discard;

    float4 pos = mul(UNITY_MATRIX_P, float4(vpos, 1));
    o.d = pos.z / pos.w;

    float3 iscr = float3(1 / _ScreenParams.x, 1 / _ScreenParams.y, 0);
    float3 dX0 = view_position(IN.tex0 + iscr.xz); if (-dX0.z == _ProjectionParams.z) dX0 = vpos;
    float3 dX1 = view_position(IN.tex0 - iscr.xz); if (-dX1.z == _ProjectionParams.z) dX1 = vpos;
    float3 dY0 = view_position(IN.tex0 + iscr.zy); if (-dY0.z == _ProjectionParams.z) dY0 = vpos;
    float3 dY1 = view_position(IN.tex0 - iscr.zy); if (-dY1.z == _ProjectionParams.z) dY1 = vpos;
    float3 vnrm = normalize(cross(dX1 - dX0, dY1 - dY0));
    vnrm.y *= -_ProjectionParams.x;

    IN.v2f.pos = pos;
    IN.v2f.worldPos = mul(unity_MatrixInvV, float4(vpos, 1));
    IN.v2f.worldNormal = mul(unity_MatrixInvV, vnrm);
#if defined (SHADOWS_SCREEN)
    pos.y *= -_ProjectionParams.x;
    IN.v2f._ShadowCoord = ComputeScreenPos(pos);
#endif
    IN.v2f.pack0 = IN.tex0 + vnrm.xy * 0.025;
    o.c = frag_surf(IN.v2f);
    //o.c = float4(vnrm.zzz * 0.5 + 0.5, 1);

    //float3 frame = tex2D(_MainTex, IN.tex0).xyz;
    //o.c = float4(frame * 0.5, 1);

    return o;
}

ENDCG

}

    // ---- forward rendering additive lights pass:
    Pass {
        Name "FORWARD"
        Tags { "LightMode" = "ForwardAdd" }
        ZWrite Off Blend One One
        Cull off

CGPROGRAM
// compile directives
#pragma vertex vert_flex
#pragma geometry geom_flex
#pragma fragment frag_flex
//#pragma vertex vert_surf
//#pragma fragment frag_surf
#pragma target 5.0
#pragma multi_compile_fog
#pragma multi_compile_fwdadd_fullshadows
#pragma skip_variants INSTANCING_ON
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 1 texcoords actually used
//   float2 _MainTex
#define UNITY_PASS_FORWARDADD
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 10 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0; // _MainTex
  half3 worldNormal : TEXCOORD1;
  float3 worldPos : TEXCOORD2;
  SHADOW_COORDS(3)
  UNITY_FOG_COORDS(4)
};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;

  TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  return o;
}

// flex vertex/geometry shader
struct v2g_flex
{
    appdata_full i2v;
    //uint id : VERTEXID;
};

v2g_flex vert_flex(appdata_full v/*, uint id : SV_VertexID*/)
{
    v2g_flex o = (v2g_flex)0;
    o.i2v = v;
    //o.id = id;
    return o;
}

StructuredBuffer<float4> _Points;
StructuredBuffer<uint> _Indices;
float _Radius;

struct g2f_flex
{
    v2f_surf v2f;
    //float4 pos  : POS;
    float2 tex0 : TEX0;
    float3 vpos : VIEWPOS;
    float mass : MASS;
};

[maxvertexcount(4)]
void geom_flex(point v2g_flex IN[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f_flex> OUT)
{
    float4 pnt = _Points[_Indices[/*IN[0].*/id]];
    float4 pos = float4(pnt.xyz, 1);

    float4x4 cam = transpose(unity_CameraToWorld);
    float3 up = cam[1], right = cam[0], forward = cam[2];

    float halfS = _Radius * 1.5;

    float4 v[4];
    v[0] = float4(pos + halfS * right - halfS * up, 1.0f);
    v[1] = float4(pos + halfS * right + halfS * up, 1.0f);
    v[2] = float4(pos - halfS * right - halfS * up, 1.0f);
    v[3] = float4(pos - halfS * right + halfS * up, 1.0f);

    float4x4 vp = UNITY_MATRIX_VP;// mul(UNITY_MATRIX_MVP, unity_WorldToObject);
    appdata_full i2v = IN[0].i2v;
    i2v.normal = mul(unity_WorldToObject, forward);

    g2f_flex o = (g2f_flex)0;
    o.mass = pnt.w;

    float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f), float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

    for (int i = 0; i < 4; ++i)
    {
        i2v.vertex = mul(unity_WorldToObject, v[i]);
        o.v2f = vert_surf(i2v);
        //o.pos = mul(vp, v[i]);
        o.tex0 = tex0[i];
        o.vpos = mul(unity_WorldToCamera, v[i]);
        OUT.Append(o);
    }
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  #if !defined(LIGHTMAP_ON)
      gi.light.color = _LightColor0.rgb;
      gi.light.dir = lightDir;
  #endif
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}

// flex fragment shader
struct f2o_flex
{
    float4 c : SV_Target;
    float d : SV_Depth;
};

f2o_flex frag_flex(g2f_flex IN)
{
    f2o_flex o = (f2o_flex)0;

    float scale = 1.0 / 1.5;
    float iscale = 1.0 / scale;
    float3 p = float3(IN.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0), 0) * 1.5;
    float3 d = normalize(IN.vpos) / _Radius;

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

    float depth = IN.vpos.z - z * _Radius;
    float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
    o.d = (1 / depth - pw) / pz;

    IN.v2f.worldNormal = mul(unity_CameraToWorld, normalize(p - float3(0, 0, z)));
    o.c = 0;//frag_surf(IN.v2f);

    return o;
}

ENDCG

}

    // ---- deferred shading pass:
    Pass {
        Name "DEFERRED"
        Tags { "LightMode" = "Deferred" }
        Cull off

CGPROGRAM
// compile directives
#pragma vertex vert_flex
#pragma geometry geom_flex
#pragma fragment frag_flex
//#pragma vertex vert_surf
//#pragma fragment frag_surf
#pragma target 5.0
#pragma exclude_renderers nomrt
#pragma multi_compile_prepassfinal
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: YES
// 1 texcoords actually used
//   float2 _MainTex
#define UNITY_PASS_DEFERRED
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 10 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0; // _MainTex
  half3 worldNormal : TEXCOORD1;
  float3 worldPos : TEXCOORD2;
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD3;
#endif
  float4 lmap : TEXCOORD4;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
    half3 sh : TEXCOORD5; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD5;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir = viewDirForLight;
  #endif
#ifdef DYNAMICLIGHTMAP_ON
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// flex vertex/geometry shader
struct v2g_flex
{
    appdata_full i2v;
};

v2g_flex vert_flex(appdata_full v)
{
    v2g_flex o = (v2g_flex)0;
    o.i2v = v;
    return o;
}

StructuredBuffer<float4> _Points;
StructuredBuffer<float4> _Anisotropy1;
StructuredBuffer<float4> _Anisotropy2;
StructuredBuffer<float4> _Anisotropy3;
StructuredBuffer<uint> _Indices;
float _Radius;
sampler2D _DepthTex;

struct g2f_flex
{
    v2f_surf v2f;
    float2 tex0 : TEX0;
    //float3 vpos : VIEWPOS;
    //float mass : MASS;
    //float4 ani1 : ANI1;
    //float4 ani2 : ANI2;
    //float4 ani3 : ANI3;
    //float scale : SCALE;
};

[maxvertexcount(4)]
void geom_flex(point v2g_flex IN[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f_flex> OUT)
{
    if (id != 0) return;

    float4 pos[4] = { float4( 1, -1, 1, 1), float4( 1,  1, 1, 1),
                      float4(-1, -1, 1, 1), float4(-1,  1, 1, 1) };

    float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
                       float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

    //float4x4 MVP = unity_CameraToWorld;//unity_CameraInvProjection;//mul(unity_CameraToWorld, unity_CameraInvProjection);
    //float4x4 MVP = mul(mul(unity_WorldToObject, unity_CameraToWorld), unity_CameraInvProjection);

    appdata_full i2v = IN[0].i2v;

    g2f_flex o = (g2f_flex)0;

    for (int i = 0; i < 4; ++i)
    {
        //i2v.vertex = (float4)0;//mul(MVP, pos[i]);
        o.v2f = vert_surf(i2v);
        o.v2f.pos = pos[i];
        o.tex0 = tex0[i];
        //o.vpos = mul(unity_WorldToCamera, v[i]);
        OUT.Append(o);
    }

    //float4 pnt = _Points[_Indices[id]];
    //float4 pos = float4(pnt.xyz, 1);

    //float4 q1 = mul(unity_WorldToCamera, float4(_Anisotropy1[_Indices[id]].xyz, 0)); q1.w = _Anisotropy1[_Indices[id]].w + 0.001;
    //float4 q2 = mul(unity_WorldToCamera, float4(_Anisotropy2[_Indices[id]].xyz, 0)); q2.w = _Anisotropy2[_Indices[id]].w + 0.001;
    //float4 q3 = mul(unity_WorldToCamera, float4(_Anisotropy3[_Indices[id]].xyz, 0)); q3.w = _Anisotropy3[_Indices[id]].w + 0.001;
    //float scale = max(0.01, max(q1.w, max(q2.w, q3.w))) * 1.5;
    //q1.w /= scale; q2.w /= scale; q3.w /= scale;

    //float4x4 cam = transpose(unity_CameraToWorld);
    //float3 up = cam[1], right = cam[0], forward = cam[2];

    //float halfS = scale;//_Radius * 1.5;

    //float4 v[4] = { float4(pos + halfS * right - halfS * up, 1.0f), float4(pos + halfS * right + halfS * up, 1.0f),
    //                float4(pos - halfS * right - halfS * up, 1.0f), float4(pos - halfS * right + halfS * up, 1.0f) };

    //appdata_full i2v = IN[0].i2v;
    //i2v.normal = mul(unity_WorldToObject, forward);

    //g2f_flex o = (g2f_flex)0;
    //o.mass = pnt.w;

    //o.ani1 = q1; o.ani2 = q2; o.ani3 = q3;
    //o.scale = scale;

    //float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
    //                   float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

    //for (int i = 0; i < 4; ++i)
    //{
    //    i2v.vertex = mul(unity_WorldToObject, v[i]);
    //    o.v2f = vert_surf(i2v);
    //    o.tex0 = tex0[i];
    //    o.vpos = mul(unity_WorldToCamera, v[i]);
    //    OUT.Append(o);
    //}
}

// fragment shader
void frag_surf(v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #if UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}

// flex fragment shader
struct f2o_flex
{
    float d : SV_Depth;
    float4 c0 : SV_Target0;
    float4 c1 : SV_Target1;
    float4 c2 : SV_Target2;
    float4 c3 : SV_Target3;
};

f2o_flex frag_flex(g2f_flex IN)
{
    f2o_flex o = (f2o_flex)0;

    //float3x3 T = transpose(float3x3(IN.ani1.xyz * IN.ani1.w, IN.ani2.xyz * IN.ani2.w, IN.ani3.xyz * IN.ani3.w));
    //float3x3 iT = float3x3(IN.ani1.xyz / IN.ani1.w, IN.ani2.xyz / IN.ani2.w, IN.ani3.xyz / IN.ani3.w);
    //float3x3 R = transpose(float3x3(IN.ani1.xyz, IN.ani2.xyz, IN.ani3.xyz));

    //float3 p = float3(IN.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0), 0);
    //float3 d = normalize(IN.vpos);

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

    //float3 vpos = IN.vpos + mul(T, d * t) * IN.scale;

    //float depth = vpos.z;

    float depth = tex2D(_DepthTex, IN.tex0.xy).x;
    //float depth = LinearEyeDepth(tex2D(_DepthTex, IN.tex0.xy).x);
    //if (depth == _ProjectionParams.z) discard;

    float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
    o.d = (1 / depth - pw) / pz;

    //o.d = IN.v2f.pos.z / IN.v2f.pos.w;

    //IN.v2f.worldNormal = mul(unity_CameraToWorld, normalize(mul(R, p + d * t)));
    frag_surf(IN.v2f, o.c0, o.c1, o.c2, o.c3);

    //o.c3.xyz = o.d.xxx;

    return o;
}

ENDCG

}

    // ---- meta information extraction pass:
    Pass {
        Name "Meta"
        Tags { "LightMode" = "Meta" }
        Cull Off

CGPROGRAM
// compile directives
#pragma vertex vert_flex
#pragma geometry geom_flex
#pragma fragment frag_flex
//#pragma vertex vert_surf
//#pragma fragment frag_surf
#pragma target 5.0
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma skip_variants INSTANCING_ON
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 1 texcoords actually used
//   float2 _MainTex
#define UNITY_PASS_META
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 10 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        
#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float2 pack0 : TEXCOORD0; // _MainTex
  float3 worldPos : TEXCOORD1;
};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  return o;
}

// flex vertex/geometry shader
struct v2g_flex
{
    appdata_full i2v;
    //uint id : VERTEXID;
};

v2g_flex vert_flex(appdata_full v/*, uint id : SV_VertexID*/)
{
    v2g_flex o = (v2g_flex)0;
    o.i2v = v;
    //o.id = id;
    return o;
}

StructuredBuffer<float4> _Points;
StructuredBuffer<uint> _Indices;
float _Radius;

struct g2f_flex
{
    v2f_surf v2f;
    float2 tex0 : TEX0;
    float3 vpos : VIEWPOS;
    float mass : MASS;
};

[maxvertexcount(4)]
void geom_flex(point v2g_flex IN[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f_flex> OUT)
{
    float4 pnt = _Points[_Indices[/*IN[0].*/id]];
    float4 pos = float4(pnt.xyz, 1);

    float4x4 cam = transpose(unity_CameraToWorld);
    float3 up = cam[1], right = cam[0], forward = cam[2];

    float halfS = _Radius * 1.5;

    float4 v[4] = { float4(pos + halfS * right - halfS * up, 1.0f), float4(pos + halfS * right + halfS * up, 1.0f),
        float4(pos - halfS * right - halfS * up, 1.0f), float4(pos - halfS * right + halfS * up, 1.0f) };

    appdata_full i2v = IN[0].i2v;
    i2v.normal = mul(unity_WorldToObject, forward);

    g2f_flex o = (g2f_flex)0;
    o.mass = pnt.w;

    float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
        float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

    for (int i = 0; i < 4; ++i)
    {
        i2v.vertex = mul(unity_WorldToObject, v[i]);
        o.v2f = vert_surf(i2v);
        o.tex0 = tex0[i];
        o.vpos = mul(unity_WorldToCamera, v[i]);
        OUT.Append(o);
    }
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
  return UnityMetaFragment(metaIN);
}

// flex fragment shader
struct f2o_flex
{
    float4 c : SV_Target;
    float d : SV_Depth;
};

f2o_flex frag_flex(g2f_flex IN)
{
    f2o_flex o = (f2o_flex)0;

    float scale = 1.0 / 1.5;
    float iscale = 1.0 / scale;
    float3 p = float3(IN.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0), 0) * 1.5;
    float3 d = normalize(IN.vpos) / _Radius;

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

    float depth = IN.vpos.z - z * _Radius;
    float pz = _ZBufferParams.z, pw = _ZBufferParams.w;
    o.d = (1 / depth - pw) / pz;

    float3 viewPos = IN.vpos - float3(0, 0, z * _Radius);

    //IN.v2f.pos = mul(UNITY_MATRIX_VP, viewPos);
    //IN.v2f.worldPos = mul(unity_CameraToWorld, viewPos);
    //IN.v2f.worldNormal = mul(unity_CameraToWorld, normalize(p - float3(0, 0, z)));
    o.c = frag_surf(IN.v2f);

    return o;
}

ENDCG

}

    // ---- shadowcaster pass:
    Pass {
        Name "SHADOWCASTER"
        Tags { "LightMode" = "ShadowCaster" }
        Cull off

            CGPROGRAM

            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            //#pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc" 

            struct v2g {};

            struct g2f
            {
                float4 pos  : POSITION;
                float2 tex0 : TEXCOORD0;
                float3 vpos : TEXCOORD1;
                float4 ani1 : TEXCOORD2;
                float4 ani2 : TEXCOORD3;
                float4 ani3 : TEXCOORD4;
                float scale : TEXCOORD5;
            };

            struct f2o
            {
                float4 c : COLOR0;
                float d : DEPTH0;
            };

            StructuredBuffer<float4> _Points;
            StructuredBuffer<float4> _Anisotropy1;
            StructuredBuffer<float4> _Anisotropy2;
            StructuredBuffer<float4> _Anisotropy3;
            StructuredBuffer<uint> _Indices;
            float _Radius;

            void vert() {}

            [maxvertexcount(4)]
            void geom(point v2g p[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f> triStream)
            {
                uint index = _Indices[id];

                float4 pnt = _Points[index];
                float4 pos = float4(pnt.xyz, 1);

                float4 ani1 = _Anisotropy1[index];
                float4 ani2 = _Anisotropy2[index];
                float4 ani3 = _Anisotropy3[index];

                float4 q1 = mul(UNITY_MATRIX_V, float4(ani1.xyz, 0)); q1.w = ani1.w + 0.001;
                float4 q2 = mul(UNITY_MATRIX_V, float4(ani2.xyz, 0)); q2.w = ani2.w + 0.001;
                float4 q3 = mul(UNITY_MATRIX_V, float4(ani3.xyz, 0)); q3.w = ani3.w + 0.001;
                float scale = max(0.01, max(q1.w, max(q2.w, q3.w))) * 1.5;
                q1.w /= scale; q2.w /= scale; q3.w /= scale;

                float4x4 cam = transpose(UNITY_MATRIX_I_V);
                float3 up = cam[1], right = cam[0];

                float radius = _Radius;

                float halfS = scale;

                float4 v[4] = { float4(pos + halfS * right - halfS * up, 1.0f), float4(pos + halfS * right + halfS * up, 1.0f),
                                float4(pos - halfS * right - halfS * up, 1.0f), float4(pos - halfS * right + halfS * up, 1.0f) };

                float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
                                   float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

                g2f o = (g2f)0;

                o.ani1 = q1; o.ani2 = q2; o.ani3 = q3;
                o.scale = scale;

                for (int i = 0; i < 4; ++i)
                {
                    o.pos = mul(UNITY_MATRIX_VP, v[i]);
                    o.tex0 = tex0[i];
                    o.vpos = mul(UNITY_MATRIX_V, v[i]);
                    triStream.Append(o);
                }
            }

            f2o frag(g2f IN)
            {
                f2o o = (f2o)0;

                float3x3 T = transpose(float3x3(IN.ani1.xyz * IN.ani1.w, IN.ani2.xyz * IN.ani2.w, IN.ani3.xyz * IN.ani3.w));
                float3x3 iT = float3x3(IN.ani1.xyz / IN.ani1.w, IN.ani2.xyz / IN.ani2.w, IN.ani3.xyz / IN.ani3.w);
                float3x3 R = transpose(float3x3(IN.ani1.xyz, IN.ani2.xyz, IN.ani3.xyz));

                float3 p = float3(IN.tex0.xy * float2(2.0, 2.0) - float2(1.0, 1.0), 0);
                float3 d = normalize(IN.vpos);

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

                float3 vpos = IN.vpos + mul(T, d * t) * IN.scale;
                float3 vnrm = normalize(mul(R, p + d * t));
                float4x4 viewToModel = mul(unity_WorldToObject, unity_MatrixInvV);
                float3 mpos = mul(viewToModel, float4(vpos, 1));
                float3 mnrm = mul(viewToModel, float4(vnrm, 0));
                float4 pos = UnityClipSpaceShadowCasterPos(mpos, mnrm);
                pos = UnityApplyLinearShadowBias(pos);

                o.d = pos.z / pos.w;
                o.c = 0;

                return o;
            }

            ENDCG
        }

    // ---- end of surface shader generated code

#LINE 38

    }
    FallBack "Diffuse"
}
