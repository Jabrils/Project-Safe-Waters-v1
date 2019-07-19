// ------------------------------------------------------------------
//  Inputs

sampler2D _UnderWaterMask;

float _GerstnerElevationFactor;


    #if defined(SHADER_API_METAL)
        float   _Culling;
    #endif


float _HeightBasedScatteringPower;
float _HeightBasedScatteringMax;
float4 _GerstnerSecondaryWaves;

//float2 _WaterWorldShift;

    float   _LuxWater_Extrusion;

    float   _MaxDirLightDepth;
    float   _MaxFogLightDepth;
    float   _Lux_UnderWaterDirLightingDepth;
    float   _Lux_UnderWaterFogLightingDepth;

    
//  Basic Properties
    sampler2D _MainTex;
    float4  _MainTex_TexelSize;
    half    _Glossiness;
    half    _InvFade;

    half    _DetailDistance;
    half    _DetailFadeRange;

//  Reflections
    half    _ReflectionBumpScale;
    half    _ReflectionGlossiness;

//  Lighting
    half    _WaterIOR;
    half    _FresnelPower;
    fixed4  _Lux_UnderWaterAmbientSkyLight;
    fixed   _ReflectionStrength;
    half4   _UnderWaterReflCol;

//  Translucency
    fixed3  _TranslucencyColor;
    half    _ScatteringPower;
    half    _ScatteringNormalInfluence;
    half    _ScatterOcclusion;
    half    _TranslucencyIntensity;
    half    _FoamTranslucencyIntensity;
    half    _UnderwaterScatteringIntensity;

//  Planar reflections
#if defined(GEOM_TYPE_MESH)
    sampler2D _LuxWater_ReflectionTex;
    half _LuxWater_ReflectionTexMip;
    float4 _LuxWater_ReflectionTex_TexelSize;
#endif

//  Water Volume
#if defined (USINGWATERVOLUME)
    float   _WaterSurfaceYPos;
    float   _Lux_UnderWaterWaterSurfacePos;
#endif

//  Underwater Fog
    fixed4  _Color;
    half3   _DepthAtten;
    half    _Density;
    half    _FinalFogDensity;
    half    _FogAbsorptionCancellation;

//  Absorption
    half    _AbsorptionHeight;
    half    _AbsorptionMaxHeight;
    float   _AbsorptionDepth;
    fixed   _AbsorptionColorStrength;
    fixed   _AbsorptionStrength;

//  Normals
    sampler2D _BumpMap;
    half    _Refraction;
    float4  _FarBumpSampleParams;
    float4  _BumpTiling;
    float4  _BumpScale;
    float4  _FinalBumpSpeed01;
    float2  _FinalBumpSpeed23;

//  Foam
#if defined(GEOM_TYPE_BRANCH_DETAIL)
    fixed4  _FoamColor;
    fixed   _FoamSmoothness;
    half    _FoamScale;
    float   _FoamSpeed;
    half    _FoamParallax;
    half    _UnderwaterFoamParallax;
    half    _FoamSoftIntersectionFactor;
    float   _FoamTiling;
    float   _FoamNormalScale;
    half    _FoamNormalMaskScale;
#endif

//  Caustics
#if defined (GEOM_TYPE_FROND)
    sampler2D _CausticTex;
#if defined(GEOM_TYPE_LEAF)
    sampler2D _CameraGBufferTexture2; //Deferred Normals
#endif
    half    _CausticsScale;
    half    _CausticsSpeed;
    half    _CausticsTiling;
    half    _CausticsSelfDistortion;
#endif

    sampler2D _GrabTexture;
    float4  _GrabTexture_TexelSize; 

    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
    float4  _CameraDepthTexture_TexelSize;

//  Water Projectors
#if defined(USINGWATERPROJECTORS)
    sampler2D _LuxWater_FoamOverlay;
    sampler2D _LuxWater_NormalOverlay;
#endif

//  Gerstner Waves
#if defined(_GERSTNERDISPLACEMENT)
    float3 _GerstnerVertexIntensity;
    float _GerstnerNormalIntensity;
    uniform float4 _GAmplitude;
    uniform float4 _GFinalFrequency;
    uniform float4 _GSteepness;
    uniform float4 _GFinalSpeed;
    uniform float4 _GDirectionAB;
    uniform float4 _GDirectionCD;

    half    _FoamCaps;
    half    _DeepFoamCoverage;
    float   _DeepFoamTiling;
    half    _DeepFoamBlur;
    fixed4  _DeepFoamColor;
#endif


#if defined (LUXWATERMETALDEFERRED)
    sampler2D _Lux_GrabbedDepth;
#endif


// ------------------------------------------------------------------


    struct appdata_water {
        float4 vertex : POSITION;
        float4 tangent : TANGENT;
        float3 normal : NORMAL;
        float4 texcoord : TEXCOORD0;
        half4 color : COLOR;
        //UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        #if UNITY_VERSION >= 201711
            UNITY_POSITION(pos);
        #else
            float4 pos : SV_POSITION;
        #endif

        half4 tspace0 : TEXCOORD0;
        half4 tspace1 : TEXCOORD1;
        half4 tspace2 : TEXCOORD2;

        float4 BumpUVs : TEXCOORD3;
        float4 BumpSmallAndFoamUVs : TEXCOORD4;

        float4 grabUV : TEXCOORD5;
        float3 ViewRay : TEXCOORD6;

        float4 color : COLOR;

    //  Actually needed...
        UNITY_SHADOW_COORDS(7)

        float4 projectorScreenPos : TEXCOORD8;

        //UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };


    #include "Includes/LuxWater_Utils.cginc"
    #include "Includes/LuxWater_GerstnerWaves.cginc"


// ------------------------------------------------------------------
// Lighting helper functions

    //  https://seblagarde.wordpress.com/2013/04/29/memo-on-fresnel-equations/
    inline half3 LuxFresnelLerpUnderwater( half3 F90, half cosA) {
        half3 F0 = 0.02037;
        half n1 = 1.000293;   // Air
        half n2 = _WaterIOR;  // 1.333333; // Water at rooom temperatur...
        half eta = (n2 / n1);
        half k = eta * eta * (1.0 - cosA * cosA);
    //  As we do not have any real internal reflections
    //  k = saturate(k);
    //  >= 1 tp prevent NaNs!
        if (k >= 1.0 ) {     // Total internal Reflection
            return 1.0;
        }
        cosA = sqrt(1.0 - k);
        return lerp(F0, F90, Pow5(1.0 - cosA));
    }

    inline half3 LuxFresnelLerp (half3 F0, half3 F90, half cosA) {
        //half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
        half t = pow(1 - cosA, _FresnelPower); 
        return lerp (F0, F90, t);
    }


// ------------------------------------------------------------------
// Helper functions to handle orthographic / perspective projection    

    inline float GetOrthoDepthFromZBuffer (float rawDepth) {
        #if defined(UNITY_REVERSED_Z)
            rawDepth = 1.0f - rawDepth;
        #endif
            return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
    }

    inline float GetSceneDepth (float rawDepth) {
        #if defined(ORTHO_SUPPORT)
            float perspectiveSceneDepth = LinearEyeDepth(rawDepth);
            float orthoSceneDepth = GetOrthoDepthFromZBuffer(rawDepth);
            return lerp(perspectiveSceneDepth, orthoSceneDepth, unity_OrthoParams.w);
        #else
            return LinearEyeDepth(rawDepth);
        #endif
    }

// ------------------------------------------------------------------
// Noise function

    float3 nrand3(float2 seed) {
        return frac(sin(dot(seed.xy, float2(12.9898, 78.233))) * float3(43758.5453, 28001.8384, 50849.4141));
    }


// ------------------------------------------------------------------
// Vertex shader

    v2f vert (appdata_water v) {
        UNITY_SETUP_INSTANCE_ID(v);
        v2f o;
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    //  Calculate wpos up front as we need it anyway and it allows us to optimize other calculations
        float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));


        float viewDepth = length(_WorldSpaceCameraPos - worldPos);
        float fade = saturate((_DetailDistance - viewDepth) / _DetailFadeRange); 
        
        #if defined(_GERSTNERDISPLACEMENT)
            _GerstnerVertexIntensity = lerp(0, _GerstnerVertexIntensity, fade );
        #endif

    //  In case we use water projector and gerstnerwaves we need the undistored screenUVs
        #if defined(USINGWATERPROJECTORS) && defined(_GERSTNERDISPLACEMENT)
            float4 hposOrig = mul(UNITY_MATRIX_VP, worldPos);
            o.projectorScreenPos = ComputeScreenPos(hposOrig);
        #endif

    //  Calculate ClipPos (optimized)
        o.pos = mul(UNITY_MATRIX_VP, worldPos);

        #if defined(EFFECT_BUMP)
        //  uv
            float2 BaseUVs = v.texcoord.xy * _BumpTiling.ww;
        #else
        //  world space texure mapping
            float2 BaseUVs = worldPos.xz * _BumpTiling.ww;
        #endif

        o.BumpUVs.xy = BaseUVs * _BumpTiling.x + _Time.xx * _FinalBumpSpeed01.xy;
        o.BumpUVs.zw = BaseUVs * _BumpTiling.y + _Time.xx * _FinalBumpSpeed01.zw;
        o.BumpSmallAndFoamUVs.xy = BaseUVs * _BumpTiling.z + _Time.xx * _FinalBumpSpeed23.xy;


    //  Gerstner Displacement
        #if defined(_GERSTNERDISPLACEMENT)
            half3 vtxForAni = worldPos.xzz;                    //    + _WaterWorldShift.xyy;
            half3 nrml;
            half3 offsets;
            Gerstner (
                offsets, nrml, v.vertex.xyz, vtxForAni,        // offsets, nrml will be written
                _GAmplitude,                                   // amplitude
                _GFinalFrequency,                              // frequency
                _GSteepness,                                   // steepness
                _GFinalSpeed,                                  // speed
                _GDirectionAB,                                 // direction # 1, 2
                _GDirectionCD                                  // direction # 3, 4
            );

            worldPos.xyz += offsets * v.color.r;

            if(_GerstnerSecondaryWaves.x > 0) {
            //  Store t normal and offsets
                half3 tnormal = nrml;
                half3 toffsets = offsets;
            //  Not sure if we should feed in the displaced position here. Looks fine tho.
                vtxForAni = worldPos.xzz; 
                Gerstner (
                    offsets, nrml, v.vertex.xyz, vtxForAni,
                    _GAmplitude * _GerstnerSecondaryWaves.x,
                    _GFinalFrequency * _GerstnerSecondaryWaves.y,
                    _GSteepness * _GerstnerSecondaryWaves.z,
                    _GFinalSpeed * _GerstnerSecondaryWaves.w,
                    _GDirectionAB.zwxy,
                    _GDirectionCD.zwxy
                );
            //  Combine and normalize normals - shitty hacky...
                v.normal = normalize(nrml * _GerstnerSecondaryWaves.x * 2 + tnormal);

            //  https://www.gamedev.net/forums/topic/678043-how-to-blend-world-space-normals/
                float3 s = float3(0, 1, 0);
                float3 u = normalize(nrml);
                float3 t = normalize(tnormal);
            //  Build the shortest-arc quaternion
                float4 q = float4(cross(s, t), dot(s, t) + 1) / sqrt(2 * (dot(s, t) + 1));
            //  Rotate the normal
                // v.normal = u * (q.w * q.w - dot(q.xyz, q.xyz)) + 2 * q.xyz * dot(q.xyz, u) + 2 * q.w * cross(q.xyz, u);

            //  Add secondary displacement
                worldPos.xyz += offsets * v.color.r;
            //  Combine offsets
                offsets += toffsets;
            }
            else {
                v.normal = normalize(nrml);
            }


        //  Foam Caps

        //  Using abs(offsets.y) reveals foam in valleys as well.
            float vFactor = (offsets.y < 0) ? 0.45 : 1;
            float JD = ( abs(offsets.x * offsets.z) * -0.5  - abs(offsets.y * vFactor)    );
            JD = saturate( (1.0 - JD  ) * 0.1);

            if (_GerstnerElevationFactor > 0) {
                float mask = offsets.y + length(offsets) * 0.1875;
                mask = smoothstep(0.0, 1.0, saturate(mask));
                JD *= lerp(1, mask, _GerstnerElevationFactor);
            }


        //  From fragment shader
            JD = saturate(JD * _FoamCaps); // * 0.5);
            //JD *= JD; // smoother without
            v.color.a = JD;
            // v.color.b = max(0.0, offsets.y); // store height displacement
            
        #endif

    //  Projector Displacment
        #if defined(USINGWATERPROJECTORS)
            if(_LuxWater_Extrusion > 0) {
                float2 projectionUVs = o.projectorScreenPos.xy / o.projectorScreenPos.w;
                fixed4 projectedNormal = tex2Dlod(_LuxWater_NormalOverlay, float4(projectionUVs, 0, 0));
                worldPos.xyz += v.normal * (projectedNormal.b) * _LuxWater_Extrusion;
            }
        #endif

    //  Calculate new ClipPos (optimized)
        #if defined(USINGWATERPROJECTORS) || defined(_GERSTNERDISPLACEMENT)
            o.pos = mul(UNITY_MATRIX_VP, worldPos);
        #endif

    //  Normals
        half3 worldNormal = UnityObjectToWorldNormal(v.normal);
        #if defined(EFFECT_BUMP)
            half3 worldTangent = UnityObjectToWorldNormal(v.tangent.xyz);
        #else
    //  In case we use world projection we must NOT rotate the tangent.
            half3 worldTangent = v.tangent.xyz;
        #endif
        half3 worldBinormal = cross(worldTangent, worldNormal);
        o.tspace0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
        o.tspace1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
        o.tspace2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

    //  Grab UVs
        o.grabUV = ComputeGrabScreenPos(o.pos);

    //  Orthographic support // make sure worldPos is float4 and w is seto to 1.0
        #if defined(ORTHO_SUPPORT)
            o.grabUV.w = lerp(o.pos.w, -mul(UNITY_MATRIX_V, worldPos).z, unity_OrthoParams.w);
        #endif

    //  Calculate ViewRay by transforming WorldPos to ViewPos (optimized)
        o.ViewRay.xyz = mul(UNITY_MATRIX_V, worldPos).xyz * float3(-1, -1, 1);

        //v.color.g = GetLightAttenuation(worldPos);

        v.color.r = fade;

        o.color = v.color;

        //UNITY_TRANSFER_SHADOW(o, v.texcoord.xy);
        return o;
    }

//  ------------------------------------------------------------------
//  Fragment shader

    half4 frag(v2f i, float facing : VFACE) : SV_Target {

    //  Get noise
        float2 seed = i.grabUV.xy * _ScreenParams.zw;
        float3 noise = nrand3(seed);

        float surfaceEyeDepth = i.grabUV.w; // = LinearEyeDepth(i.grabUV.z / i.grabUV.w);

        #if defined(ORTHO_SUPPORT)
            float orthoSurfaceEyeDepth = i.grabUV.w;
        //  We have to reset i.grabUV.w as otherwise texture projection does not work
            i.grabUV.w = lerp(i.grabUV.w, 1.0f, unity_OrthoParams.w);
            const float orthoTestFactor = 2.0f * unity_OrthoParams.w;
        #endif
        
        float3 worldPos = float3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
        float3 worldViewDir = -normalize (worldPos - _WorldSpaceCameraPos.xyz);
        fixed3 worldNormalFace = fixed3(i.tspace0.z, i.tspace1.z, i.tspace2.z);

        half Smoothness = _Glossiness;
        half3 Specular = _SpecColor;
        half Translucency = 1;
        half Caustics = 0;
        
        #if defined(GEOM_TYPE_BRANCH_DETAIL)
            half4 Foam = _FoamColor;
        #else
            half4 Foam = 1;
        #endif
        Foam.a = 0;
        
        half ReflectionSmoothness = _ReflectionGlossiness;

        half4 c = fixed4(1,0,0,1);

    //  -----------------------------------------------------------------------
    //  Water Projectors: Get screen UVs and vignette   
        #if defined(USINGWATERPROJECTORS) 
            float2 projectionUVs;
            #if defined(_GERSTNERDISPLACEMENT)
                projectionUVs = i.projectorScreenPos.xy / i.projectorScreenPos.w;

                float2 strength = abs(projectionUVs - 0.5); // 0 - 0.5 range
                strength = saturate ((float2(0.5, 0.5) - strength) * 2);
                float vignette = min(strength.x, strength.y);
                vignette = saturate(vignette * 4); // sharpen
            #else
                projectionUVs = i.grabUV.xy / i.grabUV.w;
            #endif
        #endif

    //  -----------------------------------------------------------------------   
    //  Init backside rendering
        #if UNITY_VFACE_FLIPPED
            facing = -facing;
        #endif
        #if UNITY_VFACE_AFFECTED_BY_PROJECTION
            facing *= _ProjectionParams.x; // take possible upside down rendering into account
        #endif

    //  Metal may inversed facingSign if culling is off which is not handled by Unity (Unity 5.6.3 at least)
        #if defined(SHADER_API_METAL) && UNITY_VERSION < 201710
            float fsign = (_Culling == 0) ? -1 : 1;
            facing *= fsign;
        #endif

    //  In case we use displacement we may look at front faces (of adjacent tiles) - even when underwater.
    //  These frontfaces cause "cracks" in underwater lighting. So we force all pixels to use backside rendering
    //  if they are markes as underwater by the underwatermask.
        #if defined(USINGWATERVOLUME) && defined(_GERSTNERDISPLACEMENT)
            half4 underwatermask = tex2D(_UnderWaterMask, i.grabUV.xy/i.grabUV.w);
            facing = (underwatermask.g > 0) ? -1 : facing;
        #endif

    //  backside changed to float due to Metal
        float backside = (facing < 0) ? 1 : 0;

    //  -----------------------------------------------------------------------
    //  Animate and blend normals

    //  Sample and blend far and 1st detail normal
        fixed4 farSample = tex2D(_BumpMap, i.BumpUVs.xy * _FarBumpSampleParams.x + _Time.x * _FinalBumpSpeed01.xy * _FarBumpSampleParams.w);
    //  Scale farSample
        farSample = lerp(fixed4(0, 0.5, 0, 0.5), farSample, saturate(_FarBumpSampleParams.z));
        fixed4 normalSample = tex2D(_BumpMap, i.BumpUVs.xy + (farSample.ag * 2.0 - 1.0 ) * 0.01 );

        float farNormalStrength = saturate(surfaceEyeDepth /* 0.001 * */ / _FarBumpSampleParams.y);
        normalSample = lerp( normalSample, farSample, farNormalStrength ); 

        half3 refractNormal;
        #if defined(UNITY_NO_DXT5nm)
            refractNormal = (normalSample.rgb * 2 - 1) * _BumpScale.x;
        #else
            refractNormal = (normalSample.agg * 2 - 1) * _BumpScale.x;
        #endif
    //  refracted 2nd detail normal sample
        fixed4 normalSampleSmall = tex2D(_BumpMap, i.BumpUVs.zw + refractNormal.xy * 0.05 );
    //  3rd detail normal sample
        fixed4 normalSampleSmallest = tex2D(_BumpMap, i.BumpSmallAndFoamUVs.xy);
        fixed3 tangentNormal = UnpackAndBlendNormals (refractNormal, normalSampleSmall, normalSampleSmallest);

    //  -----------------------------------------------------------------------
    //  Normal Projectors
        #if defined (USINGWATERPROJECTORS)

            fixed4 projectedNormal = tex2D(_LuxWater_NormalOverlay, projectionUVs);
            // Using regular ARGB rt
            // fixed3 pNormal = projectedNormal.rgb * 2 - 1;
            // Using ARGBHalf and additibve blending
            fixed3 pNormal = projectedNormal.rgb;
            pNormal.b = sqrt(1 - saturate(dot(pNormal.xy, pNormal.xy)));
            // pNormal.xy *= -1; // proper tangent space - moved to normal projector shader
            pNormal = lerp( half3(0,0,1), pNormal, projectedNormal.a
                #if defined(_GERSTNERDISPLACEMENT)
                    * vignette
                #endif
            );
            tangentNormal = normalize(half3(tangentNormal.xy + pNormal.xy, tangentNormal.z * pNormal.z));
        #endif

    //  Final normal
        tangentNormal *= facing;
        fixed3 worldNormal = WorldNormal(i.tspace0.xyz, i.tspace1.xyz, i.tspace2.xyz, tangentNormal);

    //  -----------------------------------------------------------------------
    //  Edgeblendfactor - in view space  
    //  This does not work on metal if we are using deferred rendering and enable ZWrite... - > force DepthNormalTexture?
        
        #if defined(SHADER_API_METAL) && defined(LUXWATERMETALDEFERRED)
            half origDepth = tex2Dproj(_Lux_GrabbedDepth, UNITY_PROJ_COORD(i.grabUV)).r;
        #else
            half origDepth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.grabUV));
        #endif

        float sceneDepth = GetSceneDepth(origDepth);
        float viewDepth = sceneDepth - surfaceEyeDepth;
        viewDepth = viewDepth;
        
        float edgeBlendFactor = saturate (_InvFade * viewDepth);
        float origEdgeBlendFactor = edgeBlendFactor;

    //  -----------------------------------------------------------------------
    //  Refraction - calculate distorted grab UVs   
    //  Calculate fade factor for refraction according to depth // not needed any more due to our strange depth handling...

        #if defined(UNITY_REVERSED_Z)
            float2 perspectiveFadeFactor = -1;
        #else
            float2 perspectiveFadeFactor = float2(1, -1);
        #endif

    //  Somehow handle orthographic projection
        #if defined(ORTHO_SUPPORT)
            perspectiveFadeFactor = (unity_OrthoParams.w) ? 
              1 / unity_OrthoParams.x
            : perspectiveFadeFactor;
            #if !defined(UNITY_REVERSED_Z)
                perspectiveFadeFactor *= -1;
            #endif
        #endif

        float2 offsetFactor = _GrabTexture_TexelSize.xy * _Refraction * perspectiveFadeFactor * edgeBlendFactor;
        float2 offset = worldNormal.xz * offsetFactor;
        // ViewSpace normals - these would handle all this perspectiveFadeFactor stuff automatically but create some nasty artifacts
        // offset = mul((float3x3)UNITY_MATRIX_VP, worldNormal).xz * offsetFactor;
        float4 distortedGrabUVs = i.grabUV;
        distortedGrabUVs.xy += offset;

    //  Snap distortedGrabUVs to pixels as otherwise the depth texture lookup will return
    //  a false depth which leads to a 1 pixel error (caused by fog and absorption) at high depth and color discontinuities (e.g. ship above ground).
        float2 snappedDistortedGrabUVs = distortedGrabUVs.xy / distortedGrabUVs.w;
        #if defined(_PIXELSNAP_POINT)
    //  As proposed by bgolus:
            snappedDistortedGrabUVs = (floor(snappedDistortedGrabUVs * _GrabTexture_TexelSize.zw) + 0.5) / _GrabTexture_TexelSize.zw;
        #endif

    //  -----------------------------------------------------------------------
    //  Do not grab pixels from foreground 
        #if defined(SHADER_API_METAL) && defined(LUXWATERMETALDEFERRED)
            float refractedRawDepth = tex2Dlod(_Lux_GrabbedDepth, float4(snappedDistortedGrabUVs, 0, 0) ).r;
        #else
            float refractedRawDepth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(snappedDistortedGrabUVs, 0, 0));
        #endif
        #if defined(_PIXELSNAP_MSAA_4X)
            float refractedSceneDepth = GetSceneDepth(refractedRawDepth);
            refractedRawDepth = MSSADepth(surfaceEyeDepth, refractedSceneDepth, refractedRawDepth, snappedDistortedGrabUVs);
        #endif

        float origSceneDepth = sceneDepth;
        #if defined(_PIXELSNAP_MSAA_4X)
            sceneDepth = refractedSceneDepth;
        #else
            sceneDepth = GetSceneDepth(refractedRawDepth);
        #endif

        if ( sceneDepth <= surfaceEyeDepth) {
            distortedGrabUVs = i.grabUV;
            refractedRawDepth = origDepth;
            sceneDepth = origSceneDepth;
            snappedDistortedGrabUVs = i.grabUV / i.grabUV.w;
        }

    //  Get final scene 01 and eye depth
        float refractedScene01Depth = Linear01Depth (refractedRawDepth);
        viewDepth = sceneDepth - surfaceEyeDepth;

    //  We have to store viewDepth as it might get tweaked by absorption but is needed by foam.
        float finalViewDepth = viewDepth;        

    //  Adjust edgeBlendFactor according to the final refracted depth sample
        edgeBlendFactor = saturate (_InvFade * viewDepth);

    //  -----------------------------------------------------------------------
    //  Fog and Absorption

    //  Reconstruct world position of refracted pixel
        float3 ray = i.ViewRay.xyz;

        ray = ray * (_ProjectionParams.z / ray.z);
    //  This is only an estimation as the view vector is not correct
        float4 vpos = float4(ray * refractedScene01Depth, 1);

        #if defined(ORTHO_SUPPORT)
        //  https://github.com/keijiro/DepthInverseProjection/blob/master/Assets/InverseProjection/Resources/InverseProjection.shader
            float2 rayOrtho = float2( unity_OrthoParams.xy * ( i.grabUV.xy - 0.5) * 2 /* to clipspace */);
            rayOrtho *= float2(-1, -1);
            float4 vposOrtho = float4(rayOrtho, -sceneDepth, 1);
            vpos = lerp(vpos, vposOrtho, unity_OrthoParams.w);
        #endif

        float3 wpos = mul (unity_CameraToWorld, vpos).xyz;

        #if defined(ORTHO_SUPPORT)
            wpos -= _WorldSpaceCameraPos * orthoTestFactor;
            // ortho wpos.y has wrong sign?! 
            wpos.y *= 1 - orthoTestFactor;
        #endif
 
        #if defined(USINGWATERVOLUME)
            #define waterYPos _WaterSurfaceYPos
        #else
            #define waterYPos worldPos.y
        #endif

    //  for foam / caustics in forward we need unrefracted wpos as well
        float4 vposUnrefracted = float4(ray * Linear01Depth(origDepth), 1);

        #if defined(ORTHO_SUPPORT)
            float4 vposUnrefractedOrtho = float4(rayOrtho, -origSceneDepth, 1);
            vposUnrefracted = lerp(vposUnrefracted, vposUnrefractedOrtho, unity_OrthoParams.w);
        #endif

        float3 wposUnrefracted = mul(unity_CameraToWorld, vposUnrefracted).xyz;    

        #if defined(ORTHO_SUPPORT)
            wposUnrefracted -= _WorldSpaceCameraPos * orthoTestFactor;
            // ortho wpos.y has wrong sign?! 
            wposUnrefracted.y *= 1 - orthoTestFactor;
        #endif

    //  Calculate Depth Attenuation based on world position and water y
        float depthAtten = saturate( (waterYPos - wpos.y - _DepthAtten.x) / (_DepthAtten.y) );
        depthAtten = saturate( 1.0 - exp( -depthAtten * 8.0) )  * saturate(_DepthAtten.z);
        viewDepth = (backside) ? surfaceEyeDepth : viewDepth;

    //  Noise 1
        viewDepth += viewDepth * (noise.x - 0.5) * 2 * 0.05;

    //  Calculate Attenuation along viewDirection
        float viewAtten = saturate( 1.0 - exp( -viewDepth * _Density) );
    //  Store final fog density
        half FogDensity = saturate( max(depthAtten, viewAtten));

    //  Absorption  
        float3 ColorAbsorption = float3(0.45f, 0.029f, 0.018f);
    //  Calculate Depth Attenuation
        float depthBelowSurface = saturate( (waterYPos - wpos.y)  / _AbsorptionMaxHeight);
        depthBelowSurface = exp2(-depthBelowSurface * depthBelowSurface * _AbsorptionHeight);
    //  Calculate Attenuation along viewDirection
        float d = exp2( -viewDepth * _AbsorptionDepth );
    //  Combine and apply strength
        d = lerp (1, saturate( d * depthBelowSurface), _AbsorptionStrength );
    //  Cancel absorption by fog
        d = saturate(d + FogDensity * _FogAbsorptionCancellation);
    //  Add color absorption
        ColorAbsorption = lerp( d, -ColorAbsorption, _AbsorptionColorStrength * (1.0 - d));    
        
    //  Noise 2
        #if !defined(SHADER_API_D3D9)
            float3 noffset = noise * 2 - 1;
            float offsetLum = dot(noffset, float3(0.2126f, 0.7152f, 0.0722f));
            offsetLum = offsetLum / 255.0f * 0.5f;
            ColorAbsorption = saturate(ColorAbsorption + offsetLum);
            FogDensity = saturate(FogDensity + offsetLum.x);
        #else
            ColorAbsorption = saturate(ColorAbsorption);
        #endif

    //  -----------------------------------------------------------------------
    //  Front face rendering only

        #if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_GLCORE) || defined(SHADER_API_METAL)
            UNITY_BRANCH
            if (!backside) {
            //if (!backside && i.color.r > 1.0) {
        #endif

                //  -----------------------------------------------------------------------
                //  Caustics
                    #if defined(GEOM_TYPE_FROND)
                        float CausticsTime = _Time.x * _CausticsSpeed;

                        #if defined(GEOM_TYPE_LEAF)
                            half3 gNormal = tex2Dproj(_CameraGBufferTexture2, UNITY_PROJ_COORD(distortedGrabUVs)).rgb;
                            gNormal = gNormal * 2 - 1;
                        #else
                            half3 gNormal = normalize(cross(ddx(wposUnrefracted), ddy(wposUnrefracted))); // this produces gaps
                            //half3 gNormal = normalize(cross(ddx(wpos), ddy(wpos))); // This of course would be correct but shows up crazy discontinueties.
                            gNormal.y = -gNormal.y;
                        #endif

                        float2 cTexUV = wpos.xz * _CausticsTiling       + offset;
                        float2 mainDir = _FinalBumpSpeed01.xy;
                    //  Make caustics distort themselves by adding gb
                        fixed4 causticsSample = tex2D(_CausticTex, cTexUV + CausticsTime.xx * mainDir);
                        causticsSample += tex2D(_CausticTex, cTexUV * 0.78 + float2(-CausticsTime, -CausticsTime * 0.87) * mainDir + causticsSample.gb * _CausticsSelfDistortion );
                        causticsSample += tex2D(_CausticTex, cTexUV * 1.13 + float2(CausticsTime, 0.36) * mainDir - causticsSample.gb * _CausticsSelfDistortion );

                        //causticsSample = tex2D(_CausticTex, cTexUV + CausticsTime.xx * mainDir);
                        //fixed4 causticsSample1 = tex2D(_CausticTex, cTexUV * 0.78 + float2(-CausticsTime, -CausticsTime * 0.87) * mainDir + causticsSample.gb*0.2 );
                        //causticsSample = tex2D(_CausticTex, cTexUV + CausticsTime.xx * mainDir + float2(causticsSample.g, causticsSample1.b) * 0.1);

                        //Caustics = causticsSample.r * saturate( (gNormal.y - 0.125) * 2);
                        Caustics = causticsSample.r * saturate(gNormal.y - 0.15) * saturate( (gNormal.y - 0.15) * 2);
                        Caustics *= _CausticsScale * edgeBlendFactor * edgeBlendFactor;
                        //Caustics *= i.color.r;
                    #endif

            #if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_GLCORE) || defined(SHADER_API_METAL)
                }
            #endif
    //  End of front face rendering only
    //  -----------------------------------------------------------------------


    //  -----------------------------------------------------------------------
    //  Foam
        #if defined(GEOM_TYPE_BRANCH_DETAIL)
            const half FoamSoftIntersectionFactor = .75;

            float height = _FoamParallax * worldNormal.z;

            float2 foamViewDir;
        
        //  Compute parallax offset based on texture mapping
            #if defined(EFFECT_BUMP)
                float3 tangentSpaceViewDir = i.tspace0.xyz * worldViewDir.x + i.tspace1.xyz * worldViewDir.y + i.tspace2.xyz * worldViewDir.z;
                foamViewDir = normalize(tangentSpaceViewDir.xy);
            #else
                foamViewDir = worldViewDir.xz;
            #endif
            float2 parallaxOffset = foamViewDir * height;

        //  float2 foamUVS = IN.worldPos.xz * _FoamTiling + _Time.xx * _FinalBumpSpeed01.xy * _FoamSpeed + worldNormal.xz*0.05 + parallaxOffset;
        //  We want the distortion from the Gerstner waves, so we have to use IN.BumpUVs
            float2 foamUVS = i.BumpUVs.xy * _FoamTiling + _Time.xx * _FoamSpeed * _FinalBumpSpeed01.xy  + parallaxOffset;
            
            half4 rawFoamSample = tex2D(_MainTex, foamUVS );
            half FoamSample = 1;
            half FoamThreshold = tangentNormal.z * 2 - 1;

        //  SceneDepth looks totally boring...
            //half FoamSoftIntersection = saturate( _FoamSoftIntersectionFactor * (min(sceneDepth,refractedSceneEyeDepth) - surfaceEyeDepth ) );
            
        //  TOOD: Why is this not the same? At least not when looking from below the surface?
        //  Well, we flip it: viewDepth = (backside) ? surfaceEyeDepth : viewDepth;
            //half FoamSoftIntersection = saturate( _FoamSoftIntersectionFactor * viewDepth);

            half FoamSoftIntersection = saturate( _FoamSoftIntersectionFactor * finalViewDepth );

            //  This introduces ghosting:
            //  FoamSoftIntersection = min(FoamSoftIntersection, saturate( _FoamSoftIntersectionFactor *   (waterYPos - wposUnrefracted.y) ) );
            //  This does not really help:
            //  FoamSoftIntersection = min(FoamSoftIntersection, saturate( _FoamSoftIntersectionFactor *   (waterYPos - wpos.y) ) );

        //  Get shoreline foam mask
            half shorelineFoam = saturate(-FoamSoftIntersection * (1 + FoamThreshold.x) + 1 );
            shorelineFoam = shorelineFoam * saturate(FoamSoftIntersection - FoamSoftIntersection * FoamSoftIntersection );
            FoamSample *= shorelineFoam;

            FoamSample *= (backside) ? 0.5 : 1;

        //  Get foam caps
            half underWaterFoam = 0;
            #if defined(_GERSTNERDISPLACEMENT)
                half foamCaps = i.color.a;
            //  Moved to vertex shader         
                //foamCaps = saturate(foamCaps * _FoamCaps);
                //foamCaps *= foamCaps;
                FoamSample = max(FoamSample, foamCaps);
            #endif

        //  Mask foam by the water's normals
            //half FoamMaskFromNormal = saturate(0.8h + (tangentNormal.x * tangentNormal.z) * 2.0h);
            half FoamMaskFromNormal = abs(tangentNormal.x * tangentNormal.z);
            FoamMaskFromNormal = saturate(1.0h - FoamMaskFromNormal * _FoamNormalMaskScale);            

FoamSample *= FoamMaskFromNormal;
FoamSample = saturate(FoamSample * _FoamScale);
            
        //  Add Foam Projectors
            #if defined (USINGWATERPROJECTORS)
                fixed4 projectedFoam = tex2D(_LuxWater_FoamOverlay, projectionUVs);
            //  This way we get "regular" foam
                FoamSample = max(FoamSample, projectedFoam.r * FoamMaskFromNormal
                #if defined(_GERSTNERDISPLACEMENT)
                    * vignette
                #endif
                );
            #endif



        //  Deep Foam
            #if defined(_GERSTNERDISPLACEMENT)
                half4 underwaterFoamSample = tex2Dbias(_MainTex, float4( (foamUVS - foamViewDir * _UnderwaterFoamParallax * (1.0 - 2.0 * backside) ) * _DeepFoamTiling, 0, _DeepFoamBlur ) );
                
                float uw = saturate(1.0 - saturate(foamCaps * _FoamScale * _DeepFoamCoverage));
                underWaterFoam = saturate(smoothstep(uw, /*saturate*/(uw + 0.9975h), underwaterFoamSample.a )) * _DeepFoamColor.a;
            //  Add Deep Foam from projectors
                #if defined (USINGWATERPROJECTORS)
                    underWaterFoam = saturate (underWaterFoam + projectedFoam.r * FoamMaskFromNormal * vignette * underwaterFoamSample.a );
                #endif
            //  Break up the vertex only foam distribution a bit.
                FoamSample -= (underWaterFoam) * 0.1;                

            #endif

            FoamSample = saturate(1.0h - FoamSample);
//            FoamSample = saturate(smoothstep( FoamSample, /*saturate*/(FoamSample + 0.75h) , rawFoamSample.a) );
 FoamSample = saturate(smoothstep( FoamSample, /*saturate*/(FoamSample + 0.375h) , rawFoamSample.a) );

            Foam.a = saturate(FoamSample * _FoamColor.a);
            Foam.rgb = _FoamColor.rgb;

            half3 FoamNormal = UnpackScaleNormal(rawFoamSample.rgbr, Foam.a * _FoamNormalScale);
            FoamNormal.z *= facing;
        
        //  Add simple Foam Projectors
            #if defined (USINGWATERPROJECTORS)
                Foam.a = saturate(Foam.a + projectedFoam.g
                    #if defined(_GERSTNERDISPLACEMENT)
                        * vignette
                    #endif
                );
            #endif

        //  Tweak all other outputs
            tangentNormal = lerp(tangentNormal, FoamNormal, Foam.a);
            Smoothness = lerp(Smoothness, _FoamSmoothness, Foam.a);
            half FoamReflectionSmoothness = Smoothness;

            ReflectionSmoothness = lerp(ReflectionSmoothness, FoamReflectionSmoothness, Foam.a);
            Caustics *= (1.0 - Foam.a);

        //  Recalculate worldNormal
            worldNormal = WorldNormal(i.tspace0.xyz, i.tspace1.xyz, i.tspace2.xyz, tangentNormal);

        #endif

worldNormal = normalize(worldNormal);


    //  Calculate ReflectionNormal
        half3 ReflectionNormal = lerp( worldNormalFace * facing, worldNormal, _ReflectionBumpScale);

    //  -----------------------------------------------------------------------
    //  Reflections

        half3 Reflections;

    //  Planar reflections
        #if defined (GEOM_TYPE_MESH)
            #if defined (UNITY_PASS_FORWARDBASE)
                float2 reflOffset = ReflectionNormal.xz * offsetFactor;
                float2 refluv = (i.grabUV.xy / i.grabUV.w) + reflOffset;
                Reflections = tex2D(_LuxWater_ReflectionTex, refluv.xy);
            #else
                Reflections = 0;
            #endif
        #endif

    //  -----------------------------------------------------------------------
    //  Set missing data for BRDF
        
        #if defined (UNITY_PASS_FORWARDBASE)
            half3 Refraction = tex2Dlod(_GrabTexture, float4(snappedDistortedGrabUVs,0,0) ).rgb;
        #else
            half3 Refraction = 0;
        #endif


        half Occlusion = 1;
        half3 OrigRefraction = Refraction;

    //  -----------------------------------------------------------------------
    //  Init Lighting

    //  World LightDir
        #ifndef USING_DIRECTIONAL_LIGHT
            half3 lightDir = normalize(UnityWorldSpaceLightDir( worldPos ));
        #else
            half3 lightDir = _WorldSpaceLightPos0.xyz;
        #endif
    //  Light Attenuation and color
        UNITY_LIGHT_ATTENUATION(atten, i, worldPos);
        half3 lightColor = _LightColor0.rgb * atten;

        //lightColor *= i.color.g;


    //  GI lighting (ambient sh and reflections)
        #if defined (UNITY_PASS_FORWARDBASE)
            UnityGI gi;
            UnityGIInput giInput;
            UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);

            giInput.worldPos = worldPos;
            giInput.worldViewDir = worldViewDir;
            //giInput.ambient.rgb = 0.0;

            giInput.probeHDR[0] = unity_SpecCube0_HDR;
            giInput.probeHDR[1] = unity_SpecCube1_HDR;
            #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
                giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
            #endif
            #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
            #endif
        //  Planar reflections
            #if defined (GEOM_TYPE_MESH)
                gi = UnityGlobalIllumination(giInput, Occlusion, worldNormal);
                gi.indirect.specular = Reflections;
        //  Cubemap reflections
            #else
                Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(ReflectionSmoothness, worldViewDir, ReflectionNormal, Specular);
                gi = UnityGlobalIllumination(giInput, Occlusion, worldNormal, g);
            #endif
        #endif

    //  -----------------------------------------------------------------------
    //  Direct and indirect Lighting

        half oneMinusReflectivity = 1 - SpecularStrength(Specular);

        half perceptualRoughness = SmoothnessToPerceptualRoughness (Smoothness);
        half3 halfDir = Unity_SafeNormalize (lightDir + worldViewDir);
        half nv = abs(dot(worldNormal, worldViewDir));
        half nl = saturate(dot(worldNormal, lightDir));
        half nh = saturate(dot(worldNormal, halfDir));
        half lv = saturate(dot(lightDir, worldViewDir));
        half lh = saturate(dot(lightDir, halfDir));

    //  Prevent NaNs !?
        nv = saturate(nv);

        half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
        half D = GGXTerm (nh, roughness);
        half specularTerm = V*D * UNITY_PI;
        specularTerm = max(0, specularTerm * nl);

        half grazingTerm = saturate(Smoothness + (1 - oneMinusReflectivity));

    //  Calculate ambient fresnel
        half surfaceReduction;
        #ifdef UNITY_COLORSPACE_GAMMA
            surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
        #else
            surfaceReduction = 1.0 / (roughness*roughness + 1.0);
        #endif

        half3 aFresnel;
        half fogAbsorptionAttenBackside = 1;
        half fogAbsorptionAttenBacksideTrans = 1;

        UNITY_BRANCH
        if(backside) {
            aFresnel = LuxFresnelLerpUnderwater (grazingTerm, nv);
            fogAbsorptionAttenBackside = (1 - FogDensity) * ColorAbsorption;
            fogAbsorptionAttenBacksideTrans = fogAbsorptionAttenBackside * 4;
        }
        else {
            aFresnel = LuxFresnelLerp (Specular, grazingTerm, nv);
        }
        half3 inversFresnel = half3(1,1,1) - aFresnel;

    //  Light cancellation on fog realtive to camera below water surface
        float3 fogPos = _WorldSpaceCameraPos;
        #if defined(LUXWATER_DEEPWATERLIGHTING) && defined (USINGWATERVOLUME)
            float depthBelowSurface1 = saturate( ( _Lux_UnderWaterWaterSurfacePos - fogPos.y) / _Lux_UnderWaterFogLightingDepth);
        #else
            float depthBelowSurface1 = saturate( ( waterYPos - fogPos.y) / _MaxFogLightDepth);
        #endif
        float fogLightCancellation = saturate( exp2(-depthBelowSurface1 * depthBelowSurface1 * 8.0) );

    //  Shortcuts
        half MaskByFogIfBackside = 1 - FogDensity * backside;
        half CancelByAbsortionAndFogLightAtten = ColorAbsorption * fogLightCancellation;

    //  Calculate translucent lighting - moved up as we need it in diffuse lighting as well for deep foam
    //  https://colinbarrebrisebois.com/2012/04/09/approximating-translucency-revisited-with-simplified-spherical-gaussian/
        half3 transLightDir = normalize(lightDir + (noise - 0.5) * 4 * 0.0075) 
            - worldNormal * _ScatteringNormalInfluence 
            * /* flip normal*/ half3(1, (1 - 2 * backside), 1) 
            * /* scale normal*/ ( 1 - backside * 0.5   ) 
            * /* fade normal*/ ( 1 - saturate(surfaceEyeDepth * 0.02)
        );

        half transDot = dot( -transLightDir, worldViewDir);
    //  To get some fluffy lighting on foam we tweak the ScatteringPower
        // half ScatteringPower = lerp( _ScatteringPower, 6.0, Foam.a * backside * (1 - FogDensity) );
        half ScatteringPower = _ScatteringPower;
        transDot = exp2(saturate(transDot) * ScatteringPower - ScatteringPower);
    //  Occlude lightscattering by scenedepth 
        transDot *= saturate(sceneDepth * _ScatterOcclusion);

        half lightScatteringBase = transDot;

    //  -----------------------------------------------------------------------
    //  Diffuse lighting for underwater fog
        half diffuse_nl = saturate(dot(half3(0,1,0), lightDir));
        half3 WaterCol = _Color;

    //  Factor in deep foam and mask by fog when rendering underwater
        #if defined(GEOM_TYPE_BRANCH_DETAIL) && defined(_GERSTNERDISPLACEMENT)
            WaterCol = lerp(WaterCol, _DeepFoamColor.rgb, underWaterFoam * MaskByFogIfBackside );
        #endif

        #if defined (UNITY_PASS_FORWARDBASE)
        //  gi.indirect.diffuse is not equal _Lux_AmbientSkyLight
            half3 diffuseUnderwaterLighting = lightColor * diffuse_nl;

            #if defined(USINGWATERVOLUME)
                diffuseUnderwaterLighting = WaterCol * (diffuseUnderwaterLighting + _Lux_UnderWaterAmbientSkyLight);
            #else
                diffuseUnderwaterLighting = WaterCol * (diffuseUnderwaterLighting + gi.indirect.diffuse);
            #endif
        //  Add deep foam scattering
            //#if defined (GEOM_TYPE_BRANCH_DETAIL) && defined(_GERSTNERDISPLACEMENT)
            //diffuseUnderwaterLighting += lightScattering * _DeepFoamColor.rgb * _FoamTranslucencyIntensity * underWaterFoam * 0.5 * MaskByFogIfBackside;
            //#endif

        #else
    //  Handle additional lights and fade them out according to fog when looking from below
            half3 diffuseUnderwaterLighting = WaterCol * (lightColor * diffuse_nl ) * MaskByFogIfBackside;
        #endif

        diffuseUnderwaterLighting *= fogLightCancellation; 


    //  Light cancellation on caustics relative to the water surface
        #if defined(LUXWATER_DEEPWATERLIGHTING)
            float depthBelowSurface2 = saturate( ( waterYPos - wpos.y ) / _Lux_UnderWaterDirLightingDepth);
        #else
            float depthBelowSurface2 = saturate( ( waterYPos - wpos.y ) / _MaxDirLightDepth);
        #endif
        float directionalLightCancellation = saturate( exp(-depthBelowSurface2 * depthBelowSurface2 * 8.0) );
        
    //  Add caustics - but do not cancel them by fog as we lerp towards fog!
        Refraction += Caustics * lightColor * diffuse_nl * directionalLightCancellation; // * (1 - FogDensity);

    //  Add underwater fog
        Refraction = lerp(Refraction, diffuseUnderwaterLighting, FogDensity);

    //  Apply absorption
        #if defined (GEOM_TYPE_BRANCH_DETAIL) && defined(_GERSTNERDISPLACEMENT)
        //  Do not apply ColorAbsorption if there is underWaterFoam and we do rendering from above water
            Refraction *= lerp(ColorAbsorption, 1, underWaterFoam * ( 1 - backside) );
        #else
            Refraction *= ColorAbsorption;
        #endif
    
    

//  //////////////////////
        Refraction *= inversFresnel; //(backside) ? inversFresnel : 1;
//  //////////////////////


    //  -----------------------------------------------------------------------
    //  Diffuse lighting at the surface = foam
        #if defined (GEOM_TYPE_BRANCH_DETAIL)
            // As it is just foam here, we skip DisneyDiffuse and use Lambert (NdotL) – roughly +3%
            half diffuseTerm = 
                #if defined(DISNEYDIFFUSE)
                    DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
                #else
                    nl;
                #endif
            half3 diffuseLighting = Foam.rgb * (
                #if defined (UNITY_PASS_FORWARDBASE)
                    gi.indirect.diffuse + 
                #endif
                lightColor * diffuseTerm 
            );
// here it gets translighting twice...
// diffuseLighting += lightScattering * Foam.rgb * _FoamTranslucencyIntensity;

            UNITY_BRANCH
            if(backside) {
            //  Add underwater fog
                diffuseLighting = lerp(diffuseLighting, diffuseUnderwaterLighting, FogDensity) ;
            //  Apply absorption
                diffuseLighting *= ColorAbsorption;
            }
        //  Blend between water and foam
            Refraction = lerp(Refraction, diffuseLighting, Foam.a);
        #endif

    //  -----------------------------------------------------------------------
    //  Specular Lighting

        half3 specularLighting = specularTerm * lightColor * FresnelTerm (Specular, lh);
        specularLighting *= edgeBlendFactor;
    //  Add reflections
        #if defined (UNITY_PASS_FORWARDBASE)
    //  Mask reflections by foam added (backside)
            specularLighting += surfaceReduction * gi.indirect.specular * aFresnel * _ReflectionStrength * ( (backside) ? (_UnderWaterReflCol.rgb) * (1 - Foam.a) : 1.0);
        #endif
    //  Cancel reflections by fog and absorption ( if (backside) )
        specularLighting = lerp(specularLighting, diffuseUnderwaterLighting, FogDensity * backside);
        specularLighting = lerp(specularLighting, specularLighting * ColorAbsorption, backside);

    //  -----------------------------------------------------------------------
    //  Combine Lighting
    //  We have to reduce specularLighting by foam when looking from below as otherwise they sum up.
        c.rgb = Refraction + specularLighting * (1 - Foam.a * backside);

    //  Apply underwater scattering
    //  TODO: This still contains some bugs.

        #if defined (UNITY_PASS_FORWARDBASE)
        //  Handle water and foam
            half3 transcol = _TranslucencyColor;
            #if defined(GEOM_TYPE_BRANCH_DETAIL)
            //  Foam produces "local scattering". So we attenuate it over distance (fog and absorption are not reliable).
                transcol = lerp(
                    _TranslucencyColor,
                    _FoamColor * _FoamTranslucencyIntensity,
                //  This may produce NaNs (in forward only?): Saturate added.
                    saturate( Foam.a * backside * exp(-(surfaceEyeDepth * 0.2)) )
                );
            #endif
        //  Handle front and backside
            half3 fogLighting = lerp(
                //  from above
                    lightColor  * _TranslucencyIntensity,
                //  from below: We only have lightColor * diffuse_nl in underwaterpost - may change tho.
                    _UnderwaterScatteringIntensity * (lightColor * diffuse_nl + _Lux_UnderWaterAmbientSkyLight) * fogLightCancellation,
                    backside
                );
            c.rgb += lightScatteringBase * transcol * fogLighting;
        #endif


    //  -----------------------------------------------------------------------
    //  Custom fog

        #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)

            float ClipSpaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.grabUV.z);
        //  Dither
            ClipSpaceDepth += ClipSpaceDepth * (noise.x - 0.5) * 2 * 0.025;

            #if defined (UNITY_PASS_FORWARDADD)
                unity_FogColor = 0;
            #endif
            if (!backside) {
                float unityFogFactor = 1;
                #if defined(FOG_LINEAR)
                    unityFogFactor = (ClipSpaceDepth) * unity_FogParams.z + unity_FogParams.w;
                #elif defined(FOG_EXP)
                    unityFogFactor = unity_FogParams.y * (ClipSpaceDepth);
                    unityFogFactor = exp2(-unityFogFactor);
                #elif defined(FOG_EXP2)
                    unityFogFactor = unity_FogParams.x * (ClipSpaceDepth);
                    unityFogFactor = exp2(-unityFogFactor * unityFogFactor);
                #endif
                c.rgb = lerp(unity_FogColor.rgb, c.rgb, saturate(unityFogFactor));
            }

        #elif defined(FOG_AZUR)
            if (!backside) {
                c = ApplyAzureFog( float4(c.rgb, 1), float4(i.grabUV.xy, surfaceEyeDepth, i.grabUV.w), worldPos);
            }
        
        #elif defined(FOG_ENVIRO)
            if (!backside) {
                half linear01Depth = Linear01Depth(surfaceEyeDepth);
                c = TransparentFog(c, worldPos, i.grabUV.xyz/i.grabUV.w, linear01Depth);
            }
        #endif

//nl = dot(worldNormalFace, lightDir);
//c.rgb = nl;
   
    //  Smooth water edges
        c.rgb = lerp(OrigRefraction, c.rgb, edgeBlendFactor);  


        return half4(c.rgb, 1);
    }