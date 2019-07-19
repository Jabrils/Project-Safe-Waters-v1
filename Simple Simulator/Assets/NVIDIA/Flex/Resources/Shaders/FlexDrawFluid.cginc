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


#if !defined(UNITY_PASS_SHADOWCASTER)

struct v2g_flex
{
    appdata_full i2v;
};

struct g2f_flex
{
    v2f_surf v2f;
    float2 tex0 : TEX0;
};

struct f2o_flex
{
#if !defined(UNITY_PASS_DEFERRED)
    float4 c : SV_Target;
#else
    float4 c0 : SV_Target0;
    float4 c1 : SV_Target1;
    float4 c2 : SV_Target2;
    float4 c3 : SV_Target3;
#endif
    float d : SV_Depth;
};

StructuredBuffer<float4> _Points;
StructuredBuffer<uint> _Indices;
sampler2D _DepthTex;
sampler2D _DepthTex2;

v2g_flex vert_flex(appdata_full v)
{
    v2g_flex o = (v2g_flex)0;
    o.i2v = v;
    return o;
}

[maxvertexcount(4)]
void geom_flex(point v2g_flex IN[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f_flex> OUT)
{
    if (id != 0) return;

    float4 pos[4] = { float4( 1, 1, 0, 1), float4( 1, -1, 0, 1),
                      float4(-1, 1, 0, 1), float4(-1, -1, 0, 1), };

//#if !defined(UNITY_PASS_DEFERRED)
    //float2 tex0[4] = { float2(1.0f, 1.0f), float2(1.0f, 0.0f),
    //                   float2(0.0f, 1.0f), float2(0.0f, 0.0f) };
//#else
    float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
                       float2(0.0f, 0.0f), float2(0.0f, 1.0f) };
//#endif

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

float FLEX_FLIP_Y = 0;

float3 view_position(float2 uv)
{
    //float2 _uv = float2(uv.x, 1 - uv.y);
    float3 vdir = mul(unity_CameraInvProjection, float4(uv * 2.0 - 1.0, 0, 1));
    float2 dist2 = -tex2D(_DepthTex, FLEX_FLIP_Y ? uv : float2(uv.x, 1 - uv.y)).xy;
    float dist = dist2[unity_StereoEyeIndex];
    return (vdir / vdir.z) * dist;
}

f2o_flex frag_flex(g2f_flex IN)
{
    f2o_flex o = (f2o_flex)0;

    //IN.tex0.y = _ProjectionParams.x < 0 ? 1 - IN.tex0.y : IN.tex0.y;
    //IN.tex0.y = 1 - IN.tex0.y;

    float3 vpos = view_position(IN.tex0);
    if (-vpos.z == _ProjectionParams.z) discard;
    //vpos.y *= _ProjectionParams.x;

    float4 pos = mul(UNITY_MATRIX_P, float4(vpos, 1));
    o.d = pos.z / pos.w;

    float3 iscr = float3(1 / _ScreenParams.x, 1 / _ScreenParams.y, 0);
    float3 dX0 = view_position(IN.tex0 + iscr.xz); if (abs(dX0.z - vpos.z) > 0.3) dX0 = vpos;
    float3 dX1 = view_position(IN.tex0 - iscr.xz); if (abs(dX1.z - vpos.z) > 0.3) dX1 = vpos;
    float3 dY0 = view_position(IN.tex0 + iscr.zy); if (abs(dY0.z - vpos.z) > 0.3) dY0 = vpos;
    float3 dY1 = view_position(IN.tex0 - iscr.zy); if (abs(dY1.z - vpos.z) > 0.3) dY1 = vpos;
    float3 vnrm = normalize(cross(dX1 - dX0, dY1 - dY0));
    vnrm.y *= -_ProjectionParams.x;

    IN.v2f.pos = pos;
    IN.v2f.worldPos = mul(unity_MatrixInvV, float4(vpos, 1));
    IN.v2f.worldNormal = mul(unity_MatrixInvV, vnrm);
#if defined (SHADOWS_SCREEN)
    pos.y *= -_ProjectionParams.x;
    IN.v2f._ShadowCoord = ComputeScreenPos(pos);
#endif
    //IN.tex0.y = 1 - IN.tex0.y;
    IN.v2f.pack0 = IN.tex0 + vnrm.xy * -0.025;
//#if UNITY_SINGLE_PASS_STEREO
//    float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
//    IN.v2f.pack0 = (IN.v2f.pack0 - scaleOffset.zw) / scaleOffset.xy;
//#endif
#if !defined(UNITY_PASS_DEFERRED)
    o.c = frag_surf(IN.v2f);
#else
    frag_surf(IN.v2f, o.c0, o.c1, o.c2, o.c3);
#endif

    return o;
}

#else

struct v2g_flex
{
    appdata_full i2v;
};

struct g2f_flex
{
    v2f_surf v2f;
    float2 tex0 : TEX0;
    float3 vpos : VIEWPOS;
    float4 ani1 : ANI1;
    float4 ani2 : ANI2;
    float4 ani3 : ANI3;
    float scale : SCALE;
};

struct f2o_flex
{
    float4 c : SV_Target;
    float d : SV_Depth;
};

StructuredBuffer<uint> _Indices;
StructuredBuffer<float4> _Points;
StructuredBuffer<float4> _Anisotropy1;
StructuredBuffer<float4> _Anisotropy2;
StructuredBuffer<float4> _Anisotropy3;
//sampler2D _DepthTex;

v2g_flex vert_flex(appdata_full v)
{
    v2g_flex o = (v2g_flex)0;
    o.i2v = v;
    return o;
}

[maxvertexcount(4)]
void geom_flex(point v2g_flex IN[1], uint id : SV_PrimitiveId, inout TriangleStream<g2f_flex> OUT)
{
    //if (id >= _IndexCount) return;

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

    float halfS = scale;

    float4 v[4] = { float4(pos + halfS * right - halfS * up, 1.0f), float4(pos + halfS * right + halfS * up, 1.0f),
                    float4(pos - halfS * right - halfS * up, 1.0f), float4(pos - halfS * right + halfS * up, 1.0f) };

    float2 tex0[4] = { float2(1.0f, 0.0f), float2(1.0f, 1.0f),
                       float2(0.0f, 0.0f), float2(0.0f, 1.0f) };

    appdata_full i2v = IN[0].i2v;

    g2f_flex o = (g2f_flex)0;

    o.ani1 = q1; o.ani2 = q2; o.ani3 = q3;
    o.scale = scale;

    for (int i = 0; i < 4; ++i)
    {
        o.v2f = vert_surf(i2v);
        o.v2f.pos = mul(UNITY_MATRIX_VP, v[i]);
        //o.pos = mul(UNITY_MATRIX_VP, v[i]);
        o.tex0 = tex0[i];
        o.vpos = mul(UNITY_MATRIX_V, v[i]);
        OUT.Append(o);
    }
}

//float3 view_position(float2 uv)
//{
//    float3 vdir = mul(unity_CameraInvProjection, float4(uv * 2.0 - 1.0, 0, 1));
//    float dist = -tex2D(_DepthTex, uv).x;
//    return (vdir / vdir.z) * dist;
//}

f2o_flex frag_flex(g2f_flex IN)
{
    f2o_flex o = (f2o_flex)0;

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

    //IN.v2f.pos = pos;
    IN.v2f.worldPos = mul(unity_MatrixInvV, float4(vpos, 1));
    //IN.v2f.worldNormal = mul(unity_MatrixInvV, vnrm);
    o.c = frag_surf(IN.v2f);

    float4 pos = UnityClipSpaceShadowCasterPos(mpos, mnrm);
    pos = UnityApplyLinearShadowBias(pos);

    o.d = pos.z / pos.w;
    //o.c = 0;

    return o;
}


#endif
