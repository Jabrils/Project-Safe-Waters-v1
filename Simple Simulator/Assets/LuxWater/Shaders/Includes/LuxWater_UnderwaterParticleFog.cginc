    CBUFFER_START(LuxUnderwater)
    //  Lighting    
        fixed3 _Lux_UnderWaterSunColor;
        float3 _Lux_UnderWaterSunDir;
        fixed3 _Lux_UnderWaterAmbientSkyLight;

        float4 _Lux_UnderWaterSunDirViewSpace;
        
    //  Depth Attenuation and Fog
        float  _Lux_UnderWaterWaterSurfacePos;
        float  _Lux_UnderWaterDirLightingDepth;
        float  _Lux_UnderWaterFogLightingDepth;

    //  Fog
        fixed3 _Lux_UnderWaterFogColor;
        float3 _Lux_UnderWaterFogDepthAtten;
        float  _Lux_UnderWaterFogDensity;
        float  _Lux_UnderWaterFinalFogDensity;
        float  _Lux_UnderWaterFogAbsorptionCancellation;

    //  Absorption
        float _Lux_UnderWaterAbsorptionHeight;
        float _Lux_UnderWaterAbsorptionMaxHeight;
        float _Lux_UnderWaterAbsorptionDepth;
        float _Lux_UnderWaterAbsorptionColorStrength;
        float _Lux_UnderWaterAbsorptionStrength;

    //  Scattering
        float _Lux_UnderWaterUnderwaterScatteringPower;
        float _Lux_UnderWaterUnderwaterScatteringIntensity;
        fixed3 _Lux_UnderWaterUnderwaterScatteringColor;
        float _Lux_UnderwaterScatteringOcclusion;

    //  Caustics
        float _Lux_UnderWaterCausticsScale;
        float _Lux_UnderWaterCausticsSpeed;
        float _Lux_UnderWaterCausticsTiling;
        float _Lux_UnderWaterCausticsSelfDistortion;
        float2 _Lux_UnderWaterFinalBumpSpeed01;
    CBUFFER_END


    //#define SETUP_VERTEX_FOGLIGHTING \

    void SetupVertexFoglightingAdditive( in appdata_t v, inout v2f o) {
    //  Fog Lighting
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.worldPos = worldPos;
        o.fogLighting.xyz = 0;
        
    //  Along view
        float fogDensity = (1.0 - saturate( exp( -o.projPos.z * _Lux_UnderWaterFogDensity ) ) );
    //  Depth along the y axis
        float depthAtten = saturate( (_Lux_UnderWaterWaterSurfacePos - worldPos.y - _Lux_UnderWaterFogDepthAtten.x)  / (_Lux_UnderWaterFogDepthAtten.y) );
        depthAtten = saturate( 1.0 - exp( -depthAtten * 8.0)  ) * saturate(_Lux_UnderWaterFogDepthAtten.z); 
        fogDensity = max(fogDensity, depthAtten);
        
        o.fogLighting.w = fogDensity;
    }

    void SetupVertexFoglighting( in appdata_t v, inout v2f o) {
    //  Fog Lighting
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.worldPos = worldPos;

        float3 worldViewDir = normalize( _WorldSpaceCameraPos.xyz - worldPos);
        float3 fogLighting = _Lux_UnderWaterSunColor.rgb;

        float fCos = saturate( dot( _Lux_UnderWaterSunDir, worldViewDir ) ); // no sign here?!
        // Old: float viewScatter = fCos * fCos * _Lux_UnderWaterUnderwaterScatteringPower;
        // Old: fogLighting *= viewScatter * _Lux_UnderWaterUnderwaterScatteringIntensity + 1.0f;
        float viewScatter = exp2(saturate(fCos) * _Lux_UnderWaterUnderwaterScatteringPower - _Lux_UnderWaterUnderwaterScatteringPower);
        
    //  Add ambient lighting to fog
        fogLighting += _Lux_UnderWaterAmbientSkyLight.rgb;

        float3 fogPos = _WorldSpaceCameraPos;
        float depthBelowSurface1 = saturate( ( _Lux_UnderWaterWaterSurfacePos - fogPos.y) / _Lux_UnderWaterFogLightingDepth);
        float depthBelowSurface2 = exp2(-depthBelowSurface1 * depthBelowSurface1 * 8.0);
        fogLighting *= saturate( depthBelowSurface2);
        
        o.fogLighting.xyz = _Lux_UnderWaterFogColor * fogLighting;
        
    //  Along view
        float fogDensity = (1.0 - saturate( exp( -o.projPos.z * _Lux_UnderWaterFogDensity ) ) );
    //  Depth along the y axis
        float depthAtten = saturate( (_Lux_UnderWaterWaterSurfacePos - worldPos.y - _Lux_UnderWaterFogDepthAtten.x)  / (_Lux_UnderWaterFogDepthAtten.y) );
        depthAtten = saturate( 1.0 - exp( -depthAtten * 8.0)  ) * saturate(_Lux_UnderWaterFogDepthAtten.z); 
        fogDensity = max(fogDensity, depthAtten);
        
        o.fogLighting.w = fogDensity;

    //  Pass Scattering - no occlusion here!
        o.scatter = viewScatter * _Lux_UnderWaterUnderwaterScatteringColor * _Lux_UnderWaterUnderwaterScatteringIntensity * fogLighting;
    }


    fixed3 LuxWater_ParticleFog( fixed3 col, float4 fogLighting, float3 worldPos, float4 projPos, fixed3 scattering ) {
    //  Underwater fog
        col.rgb = lerp(col.rgb, fogLighting.xyz, fogLighting.www);
    //  Absorption
        float3 ColorAbsortion = float3(0.45f, 0.029f, 0.018f);
    //  Calculate Depth Attenuation
        float depthBelowSurface = saturate( (_Lux_UnderWaterWaterSurfacePos - worldPos.y) / _Lux_UnderWaterAbsorptionMaxHeight);
        depthBelowSurface = exp2(-depthBelowSurface * depthBelowSurface * _Lux_UnderWaterAbsorptionHeight);
    //  Calculate Attenuation along viewDirection
        float d = exp2( -projPos.z * _Lux_UnderWaterAbsorptionDepth);
    //  Combine and apply strength
        d = lerp (1, saturate( d * depthBelowSurface), _Lux_UnderWaterAbsorptionStrength );
    //  Cancel absorption by fog 
        d = saturate(d + fogLighting.w * _Lux_UnderWaterFogAbsorptionCancellation);

        ColorAbsortion = lerp( d, -ColorAbsortion, _Lux_UnderWaterAbsorptionColorStrength * (1.0 - d));
        ColorAbsortion = saturate(ColorAbsortion);  
    //  Apply absorption
        col.rgb *= ColorAbsortion;

    //  Apply Scattering
        return col.rgb + scattering;
    }