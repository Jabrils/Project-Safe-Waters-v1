// --------------------------------------------------------
// Lux lighting data calculation (direction, attenuation, ...)

float _Lux_UnderWaterWaterSurfacePos;
float _Lux_UnderWaterDirLightingDepth;
fixed3 _Lux_UnderWaterFogColor;
float _Lux_UnderWaterFogDensity;
float _Lux_UnderWaterAbsorptionDepth;
float _Lux_UnderWaterFogAbsorptionCancellation;
float _Lux_UnderWaterAbsorptionColorStrength;

void LuxWater_DeferredCalculateLightParams (
    unity_v2f_deferred i,
    out float3 outWorldPos,
    out float2 outUV,
    out half3 outLightDir,
    out float outAtten,
    out float outFadeDist,
    out float outDirLightAtten
    )
{

//  Lux Water:
    float DirLightAtten = 1;

    i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
    float2 uv = i.uv.xy / i.uv.w;

    // read depth and reconstruct world position
    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
    depth = Linear01Depth (depth);
    float4 vpos = float4(i.ray * depth,1);
    float3 wpos = mul (unity_CameraToWorld, vpos).xyz;

    float fadeDist = UnityComputeShadowFadeDistance(wpos, vpos.z);

    // spot light case
    #if defined (SPOT)
        float3 tolight = _LightPos.xyz - wpos;
        half3 lightDir = normalize (tolight);

        float4 uvCookie = mul (unity_WorldToLight, float4(wpos,1));
        // negative bias because http://aras-p.info/blog/2010/01/07/screenspace-vs-mip-mapping/
        float atten = tex2Dbias (_LightTexture0, float4(uvCookie.xy / uvCookie.w, 0, -8)).w;
        atten *= uvCookie.w < 0;
        float att = dot(tolight, tolight) * _LightPos.w;
        atten *= tex2D (_LightTextureB0, att.rr).UNITY_ATTEN_CHANNEL;

        atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);

    // directional light case
    #elif defined (DIRECTIONAL) || defined (DIRECTIONAL_COOKIE)
        half3 lightDir = -_LightDir.xyz;
        float atten = 1.0;
        atten *= UnityDeferredComputeShadow (wpos, fadeDist, uv);
        #if defined (DIRECTIONAL_COOKIE)
            atten *= tex2Dbias (_LightTexture0, float4(mul(unity_WorldToLight, half4(wpos,1)).xy, 0, -8)).w;
        #endif //DIRECTIONAL_COOKIE

    //  Lux Water: Attenuate directional light according to depth below the water surface
        #if defined (LUXWATER_DEEPWATERLIGHTING)
            float depthBelowSurface = saturate( ( _Lux_UnderWaterWaterSurfacePos - wpos.y) / _Lux_UnderWaterDirLightingDepth);
            float depthBelowSurface1 = exp2(-depthBelowSurface * depthBelowSurface * 8.0);
            DirLightAtten = saturate( depthBelowSurface1 - depthBelowSurface * 0.01);
            atten *= DirLightAtten;
        #endif

    // point light case
    #elif defined (POINT) || defined (POINT_COOKIE)
        float3 tolight = wpos - _LightPos.xyz;
        half3 lightDir = -normalize (tolight);

        float att = dot(tolight, tolight) * _LightPos.w;
        float atten = tex2D (_LightTextureB0, att.rr).UNITY_ATTEN_CHANNEL;

        atten *= UnityDeferredComputeShadow (tolight, fadeDist, uv);

        #if defined (POINT_COOKIE)
        atten *= texCUBEbias(_LightTexture0, float4(mul(unity_WorldToLight, half4(wpos,1)).xyz, -8)).w;
        #endif //POINT_COOKIE
    #else
        half3 lightDir = 0;
        float atten = 0;
    #endif

    outWorldPos = wpos;
    outUV = uv;
    outLightDir = lightDir;
    outAtten = atten;
    outFadeDist = fadeDist;

    outDirLightAtten = DirLightAtten;
}