// https://api.unrealengine.com/udk/Three/VolumetricLightbeamTutorial.html
// https://www.gamedev.net/forums/topic/692224-udk-volumetric-light-beam/

Shader "Lux Water/Underwater/Light Beam" {
Properties {
    [HDR] _Color ("Color", Color) = (1,1,1,1)
    [NoScaleOffset] _MainTex ("Fall Off (G)", 2D) = "white" {}
    [NoScaleOffset] _SpotTex ("Spot Mask (G)", 2D) = "white" {}

    [Header(Detail Noise)]
    [Toggle(_SPECGLOSSMAP)]
    _SpecGlossEnabled       ("Enable detail noise", Float) = 0
    _DetailTex              ("    Detail Noise (G)", 2D) = "white" {}
    _DetailStrength         ("    Strength", Range(0.0, 1.0)) = 1.0
    _DetailScrollSpeed      ("    Scroll Speed 1:(XY) 2:(ZW)", Vector) = (0,0,0,0)

    _ConeWidth              ("Cone Width", Range(1.0, 20.0)) = 10.0
    _SpotFade               ("Spot Mask Intensity", Range(0.0, 1.0)) = 0.75

    [Space(5)]
    _FogDensity             ("Fog Density", Range(0.1,1.0)) = 1.0

    [Header(Fade)]
    _WatersurfFadeDistance  ("    Watersurf Distance Fade", float) = 8.0
    _CamFadeDistance        ("    Camera Distance Fade", float) = 0.5
    _InvFade                ("    Soft Edge Factor", Range(0.01,3.0)) = 1.0
}

SubShader {
    Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent"}
    LOD 200
    ZWrite Off

    Pass {
        Name "FORWARD"
        Tags { "LightMode" = "ForwardBase" }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        
    //  CHECK: We do not do ZTest here as water will write into depth. So beams would be occluded when viewed from above.
    //  Instead we rely on the "Soft Edge Factor" which tests against the opaque ZBuffer.
        //ZTest Off

        CGPROGRAM
        #pragma vertex vert_surf
        #pragma fragment frag_surf
        #pragma target 3.0
        
        //#pragma multi_compile_fog

    //  Detail noise
        #pragma shader_feature _SPECGLOSSMAP
        
        #include "HLSLSupport.cginc"
        #include "UnityShaderVariables.cginc"
        #define UNITY_PASS_FORWARDBASE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _SpotTex;
        half _SpotFade;
        float _CamFadeDistance;
        fixed4 _Color;
        float _ConeWidth;
        float _FogDensity;

        #if defined(_SPECGLOSSMAP)
            sampler2D _DetailTex;
            float4 _DetailTex_ST;
            fixed _DetailStrength;
            float4 _DetailScrollSpeed;
        #endif

        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
        float _WatersurfFadeDistance;
        float _InvFade;

        struct v2f_surf {
            float4 pos : SV_POSITION;
            float4 mask_uvs : TEXCOORD1;
            #if defined(_SPECGLOSSMAP)
                float4 detail_texcoord : TEXCOORD2;
            #endif
            float4 projPos : TEXCOORD3;
            float alpha : TEXCOORD4;
            float3 worldPos : TEXCOORD5;
            //UNITY_FOG_COORDS(6)
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f_surf vert_surf (appdata_tan v) {
            v2f_surf o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
            o.pos = UnityObjectToClipPos (v.vertex);

            o.projPos = ComputeScreenPos (o.pos);
            COMPUTE_EYEDEPTH(o.projPos.z);

            o.mask_uvs.xy = v.texcoord.xy;
        
        //  Calculate Tangent Space viewDir
            TANGENT_SPACE_ROTATION; 
            float3 rVec = mul(rotation, normalize(ObjSpaceViewDir(v.vertex)));
            rVec.z = sqrt( (rVec.z + _SpotFade) * _ConeWidth);
            rVec.x = rVec.x / rVec.z + 0.5;
            rVec.y = rVec.y / rVec.z + 0.5;
            o.mask_uvs.x = rVec.x;
            o.mask_uvs.zw = rVec.xy;

        //  Clip towards camera
            float3 viewPos = UnityObjectToViewPos(v.vertex.xyz);
            float alpha = (-viewPos.z - _ProjectionParams.y) / _CamFadeDistance;
            o.alpha = min(alpha, 1);

            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

            #if defined(_SPECGLOSSMAP)
                o.detail_texcoord.xy = TRANSFORM_TEX(v.texcoord, _DetailTex);
                o.detail_texcoord.zw = o.detail_texcoord.xy * 2;
                _DetailScrollSpeed *= _Time.x;
                o.detail_texcoord.xy += _DetailScrollSpeed.xy;
                o.detail_texcoord.zw += _DetailScrollSpeed.zw;
            #endif

            //UNITY_TRANSFER_FOG(o,o.pos);
            return o;
        }

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

        fixed4 frag_surf (v2f_surf i) : SV_Target {

            float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
            float partZ = i.projPos.z;
            float fade = saturate (_InvFade * (sceneZ-partZ));

            fixed4 col = _Color;
            fixed mask01 = tex2D(_MainTex, i.mask_uvs.xy).g;
            fixed mask02 = saturate(tex2D(_SpotTex, i.mask_uvs.zw).g);
            col.a *= mask01 * mask02;

            col.a *= fade * i.alpha;

            #if defined(_SPECGLOSSMAP)
                fixed detailTex = tex2D(_DetailTex, i.detail_texcoord.xy).g;
                detailTex *= tex2D(_DetailTex, i.detail_texcoord.zw).g;
                col *= lerp(1, detailTex, _DetailStrength);
            #endif

            // UNITY_APPLY_FOG(i.fogCoord, c);

        //  Fade in beams if the camera is below the water surface
            float depthFade = saturate( (_Lux_UnderWaterWaterSurfacePos - _WorldSpaceCameraPos.y) / _WatersurfFadeDistance );
            col.a *= depthFade;

        //  Underwater fog
            float3 fogLighting = _Lux_UnderWaterSunColor.rgb;
            float3 worldViewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
            float fCos = saturate( dot( _WorldSpaceLightPos0.xyz, -worldViewDir ) );
            // Old: float viewScatter = fCos * fCos * _Lux_UnderWaterUnderwaterScatteringPower;
            // Old: fogLighting *= viewScatter * _Lux_UnderWaterUnderwaterScatteringIntensity + 1.0f;
            float viewScatter = exp2(saturate(fCos) * _Lux_UnderWaterUnderwaterScatteringPower - _Lux_UnderWaterUnderwaterScatteringPower);
    
        //  Add ambient lighting
            fogLighting += _Lux_UnderWaterAmbientSkyLight.rgb; // gi.indirect.diffuse; //

            float3 fogPos = _WorldSpaceCameraPos;
            float depthBelowSurface1 = saturate( ( _Lux_UnderWaterWaterSurfacePos - fogPos.y) / _Lux_UnderWaterFogLightingDepth);
            float depthBelowSurface2 = exp2(-depthBelowSurface1 * depthBelowSurface1 * 8.0);
            fogLighting *= saturate( depthBelowSurface2);

            float fogDensity = (1.0 - saturate( exp( -i.projPos.z * _Lux_UnderWaterFogDensity   * _FogDensity   ) ) );
        //  Depth along the y axis
            float depthAtten = saturate( (_Lux_UnderWaterWaterSurfacePos - i.worldPos.y - _Lux_UnderWaterFogDepthAtten.x) / (_Lux_UnderWaterFogDepthAtten.y) );
            depthAtten = saturate( 1.0 - exp( -depthAtten * 8.0)  ) * saturate(_Lux_UnderWaterFogDepthAtten.z); 
            fogDensity = max(fogDensity, depthAtten);

            col.rgb = lerp(col.rgb, _Lux_UnderWaterFogColor * fogLighting, fogDensity);

        //  Absorption
            float3 ColorAbsortion = float3(0.45f, 0.029f, 0.018f);
        //  Calculate Depth Attenuation
        //  Selfilluminated - so we do not want this here
            //float depthBelowSurface = saturate( (_Lux_UnderWaterWaterSurfacePos - i.worldPos.y) / _Lux_UnderWaterAbsorptionMaxHeight);
            //depthBelowSurface = exp2(-depthBelowSurface * depthBelowSurface * _Lux_UnderWaterAbsorptionHeight);
        //  Calculate Attenuation along viewDirection
            float d = exp2( -i.projPos.z * _Lux_UnderWaterAbsorptionDepth);
        //  Combine and apply strength
            //d = lerp (1, saturate( d * depthBelowSurface), _Lux_UnderWaterAbsorptionStrength );
            d = lerp (1, d, _Lux_UnderWaterAbsorptionStrength );
        //  Cancel absorption by fog 
            d = saturate(d + fogDensity * _Lux_UnderWaterFogAbsorptionCancellation);

            ColorAbsortion = lerp( d, -ColorAbsortion, _Lux_UnderWaterAbsorptionColorStrength * (1.0 - d));
            ColorAbsortion = saturate(ColorAbsortion);  
        //  Apply absorption
            col.rgb *= ColorAbsortion;

        //  Add Scattering
            col.rgb += viewScatter * _Lux_UnderWaterUnderwaterScatteringColor  * _Lux_UnderWaterUnderwaterScatteringIntensity * fogLighting;

            return col;
          }
          ENDCG
        }
    } 
}