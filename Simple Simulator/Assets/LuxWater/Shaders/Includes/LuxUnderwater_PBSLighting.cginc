

#ifndef UNITY_PBS_LIGHTING_INCLUDED
#define UNITY_PBS_LIGHTING_INCLUDED

#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "UnityGBuffer.cginc"
#include "UnityGlobalIllumination.cginc"

//-------------------------------------------------------------------------------------
// Default BRDF to use:
#if !defined (UNITY_BRDF_PBS) // allow to explicitly override BRDF in custom shader
// still add safe net for low shader models, otherwise we might end up with shaders failing to compile
#if SHADER_TARGET < 30
#define UNITY_BRDF_PBS BRDF3_Unity_PBS
#elif defined(UNITY_PBS_USE_BRDF3)
#define UNITY_BRDF_PBS BRDF3_Unity_PBS
#elif defined(UNITY_PBS_USE_BRDF2)
#define UNITY_BRDF_PBS BRDF2_Unity_PBS
#elif defined(UNITY_PBS_USE_BRDF1)
#define UNITY_BRDF_PBS BRDF1_Unity_PBS
#elif defined(SHADER_TARGET_SURFACE_ANALYSIS)
// we do preprocess pass during shader analysis and we dont actually care about brdf as we need only inputs/outputs
#define UNITY_BRDF_PBS BRDF1_Unity_PBS
#else
#error something broke in auto-choosing BRDF
#endif
#endif

//-------------------------------------------------------------------------------------
// little helpers for GI calculation
// CAUTION: This is deprecated and not use in Untiy shader code, but some asset store plugin still use it, so let here for compatibility

#if !defined (UNITY_BRDF_GI)
#define UNITY_BRDF_GI BRDF_Unity_Indirect
#endif

inline half3 BRDF_Unity_Indirect(half3 baseColor, half3 specColor, half oneMinusReflectivity, half smoothness, half3 normal, half3 viewDir, half occlusion, UnityGI gi)
{
	return half3(0, 0, 0);
}

#define UNITY_GLOSSY_ENV_FROM_SURFACE(x, s, data)               \
    Unity_GlossyEnvironmentData g;                              \
    g.roughness /* perceptualRoughness */   = SmoothnessToPerceptualRoughness(s.Smoothness); \
    g.reflUVW = reflect(-data.worldViewDir, s.Normal);  \


#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
#define UNITY_GI(x, s, data) x = UnityGlobalIllumination (data, s.Occlusion, s.Normal);
#else
#define UNITY_GI(x, s, data)                                \
        UNITY_GLOSSY_ENV_FROM_SURFACE(g, s, data);              \
        x = UnityGlobalIllumination (data, s.Occlusion, s.Normal, g);
#endif

//-------------------------------------------------------------------------------------
// BRDF

// https://seblagarde.wordpress.com/2013/04/29/memo-on-fresnel-equations/
inline half3 LuxFresnelLerpUnderwater( half F0, half CosX) {
	half nt = (1 + sqrt(F0)) / (1 - sqrt(F0) );
	half ni = 1.333; // Water IOR at room temperature
	half R0 = (nt - ni) / (nt + ni);
	R0 *= R0;
	if (ni > nt) {
		half inv_eta = ni / nt;
		half SinT2 = inv_eta * inv_eta * (1.0h - CosX * CosX);
	//	>= 1.0h to fix NaNs
		if (SinT2 >= 1.0h) {
			return half3(1.0h, 1.0h, 1.0h); // TIR: Total internal Reflection
		}
		CosX = sqrt(1.0h - SinT2);
	}
	return R0 + (1.0h - R0) * Pow5(1.0h - CosX);
}

half4 LuxUnderwater_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir,
    UnityLight light, UnityIndirect gi)
{
    float perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
    float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);
    half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact

    float nl = saturate(dot(normal, light.dir));
    float nh = saturate(dot(normal, halfDir));

    half lv = saturate(dot(light.dir, viewDir));
    half lh = saturate(dot(light.dir, halfDir));

    // Diffuse term
    half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;

    // Specular term
    // HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
    // BUT 1) that will make shader look significantly darker than Legacy ones
    // and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
    float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

    // GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
    roughness = max(roughness, 0.002);
    float V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
    float D = GGXTerm (nh, roughness);

    float specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later

#   ifdef UNITY_COLORSPACE_GAMMA
        specularTerm = sqrt(max(1e-4h, specularTerm));
#   endif

    // specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
    specularTerm = max(0, specularTerm * nl);
#if defined(_SPECULARHIGHLIGHTS_OFF)
    specularTerm = 0.0;
#endif

    // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
    half surfaceReduction;
#   ifdef UNITY_COLORSPACE_GAMMA
        surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
#   else
        surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
#   endif

    // To provide true Lambert lighting, we need to be able to kill specular completely.
    specularTerm *= any(specColor) ? 1.0 : 0.0;

    //half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
    
    half3 color =   diffColor * (gi.diffuse + light.color * diffuseTerm)
                    + specularTerm * light.color * FresnelTerm (specColor, lh)
                #if defined(GEOM_TYPE_LEAF)
                    + surfaceReduction * gi.specular * LuxFresnelLerpUnderwater ( (1.0 - oneMinusReflectivity), nv);
                #else
                	+ surfaceReduction * gi.specular * FresnelLerp (specColor, saturate(smoothness + (1-oneMinusReflectivity)) , nv);
                #endif
    return half4(color, 1);
}

//-------------------------------------------------------------------------------------
// Specular workflow

// Surface shader output structure to be used with physically
// based shading model.

struct SurfaceOutputStandardSpecularLuxUnderwater
{
	fixed3 Albedo;      // diffuse color
	fixed3 Specular;    // specular color
	fixed3 Normal;      // tangent space normal, if written
	half3 Emission;
	half Smoothness;    // 0=rough, 1=smooth
	half Occlusion;     // occlusion (default 1)
						// Lux Water specials
	fixed3 Alpha;       // alpha for transparencies x: regular alpha, y: final alpha mask (particles), z: depth fade
	float Depth;
	float3 WorldPos;
};


CBUFFER_START(LuxUnderwater)
//	Lighting	
	fixed3 _Lux_UnderWaterSunColor;
	float3 _Lux_UnderWaterSunDir;
	fixed3 _Lux_UnderWaterAmbientSkyLight;
	
//	Depth Attenuation and Fog
	float  _Lux_UnderWaterWaterSurfacePos;
	float  _Lux_UnderWaterDirLightingDepth;
	float  _Lux_UnderWaterFogLightingDepth;

//	Fog
	fixed3 _Lux_UnderWaterFogColor;
	float3 _Lux_UnderWaterFogDepthAtten;
	float  _Lux_UnderWaterFogDensity;
	float  _Lux_UnderWaterFinalFogDensity;
	float  _Lux_UnderWaterFogAbsorptionCancellation;

//	Absorption
	float _Lux_UnderWaterAbsorptionHeight;
	float _Lux_UnderWaterAbsorptionMaxHeight;
	float _Lux_UnderWaterAbsorptionDepth;
	float _Lux_UnderWaterAbsorptionColorStrength;
	float _Lux_UnderWaterAbsorptionStrength;

//	Scattering
	float _Lux_UnderWaterUnderwaterScatteringPower;
	float _Lux_UnderWaterUnderwaterScatteringIntensity;
	fixed3 _Lux_UnderWaterUnderwaterScatteringColor;
	float _Lux_UnderwaterScatteringOcclusion;

//	Caustics
	float _Lux_UnderWaterCausticsScale;
	float _Lux_UnderWaterCausticsSpeed;
	float _Lux_UnderWaterCausticsTiling;
	float _Lux_UnderWaterCausticsSelfDistortion;
	float2 _Lux_UnderWaterFinalBumpSpeed01;
CBUFFER_END
//	Caustics
	sampler2D _Lux_UnderWaterCaustics;
	sampler2D _CameraGBufferTexture2;


// NaN checker - taken from post processing stack v2
// /Gic isn't enabled on fxc so we can't rely on isnan() anymore
bool IsNan(float x)
{
    // For some reason the following tests outputs "internal compiler error" randomly on desktop
    // so we'll use a safer but slightly slower version instead :/
    //return (x <= 0.0 || 0.0 <= x) ? false : true;
    return (x < 0.0 || x > 0.0 || x == 0.0) ? false : true;
}

bool AnyIsNan(float3 x)
{
    return IsNan(x.x) || IsNan(x.y) || IsNan(x.z);
}


// NOTE: Here we use inout - as we have to tweak s.Emission
inline half4 LightingStandardSpecularLuxUnderwater(inout SurfaceOutputStandardSpecularLuxUnderwater s, half3 viewDir, UnityGI gi) {

//	Particles may produce a currupted normal resulting in NaNs
	#if defined(GEOM_TYPE_BRANCH)
	//	We only have to check for one component! If NaN simply skip the fragment.
		if (IsNan(s.Normal.x))
			return 0;
	#endif
	s.Normal = normalize(s.Normal);
	
//	energy conservation
    half oneMinusReflectivity;
    s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

    // shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    // this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
    half outputAlpha;
    // s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha.x, oneMinusReflectivity, /*out*/ outputAlpha);
    // ^ Macro does not work?!... so we do it manually:
    #if defined(USEALPHAPREMULTIPLY)
		s.Albedo *= s.Alpha.xxx;
		outputAlpha = 1 - oneMinusReflectivity + s.Alpha.x * oneMinusReflectivity;
	#else
		outputAlpha = s.Alpha.x;
	#endif

	#if !defined(UNITY_PASS_FORWARDADD)
		float causticsDepthBelowSurface = saturate( ( _Lux_UnderWaterWaterSurfacePos - s.WorldPos.y) / _Lux_UnderWaterDirLightingDepth);
		float causticsDepthBelowSurface1 = exp2(-causticsDepthBelowSurface * causticsDepthBelowSurface * 8.0);
		float dirLightCancellation = saturate( causticsDepthBelowSurface1 - causticsDepthBelowSurface * 0.01);
		gi.light.color *= dirLightCancellation;
	#endif


    half4 c = /*UNITY_BRDF_PBS*/ LuxUnderwater_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
    c.a = outputAlpha;

//	Underwater fog - double checked: it is fine and matches post.
    float fogDensity = (1.0 - saturate( exp( -s.Depth * _Lux_UnderWaterFogDensity ) ) );
//	Depth along the y axis
	float depthAtten = saturate( (_Lux_UnderWaterWaterSurfacePos - s.WorldPos.y - _Lux_UnderWaterFogDepthAtten.x)  / (_Lux_UnderWaterFogDepthAtten.y) );
	depthAtten = saturate( 1.0 - exp( -depthAtten * 8.0)  ) * saturate(_Lux_UnderWaterFogDepthAtten.z); 
	fogDensity = max(fogDensity, depthAtten);

//	Absorption
	float3 ColorAbsortion = float3(0.45f, 0.029f, 0.018f);
//	Calculate Depth Attenuation
	float depthBelowSurface = saturate( (_Lux_UnderWaterWaterSurfacePos - s.WorldPos.y) / _Lux_UnderWaterAbsorptionMaxHeight);
	depthBelowSurface = exp2( -depthBelowSurface * depthBelowSurface * _Lux_UnderWaterAbsorptionHeight);
//	Calculate Attenuation along viewDirection
	float d = exp2( -s.Depth * _Lux_UnderWaterAbsorptionDepth);
//	Combine and apply strength
	d = lerp (1, saturate( d * depthBelowSurface), _Lux_UnderWaterAbsorptionStrength );
//	Cancel absorption by fog 
	d = saturate(d + fogDensity * _Lux_UnderWaterFogAbsorptionCancellation);
	ColorAbsortion = lerp( d, -ColorAbsortion, _Lux_UnderWaterAbsorptionColorStrength * (1.0 - d)  );
	ColorAbsortion = saturate(ColorAbsortion);	

//	Add caustics
	#if defined(GEOM_TYPE_FROND)
		#if !defined(UNITY_PASS_FORWARDADD)
			float2 cTexUV = s.WorldPos.xz * _Lux_UnderWaterCausticsTiling;
			half4 caustics = tex2D(_Lux_UnderWaterCaustics, cTexUV + _Time.x * 0.5 );
		//	animate			
			float CausticsTime = _Time.x * _Lux_UnderWaterCausticsSpeed;

			#define gNormal s.Normal
			
			caustics =  tex2D(_Lux_UnderWaterCaustics, cTexUV + CausticsTime.xx * _Lux_UnderWaterFinalBumpSpeed01.xy);
			caustics += tex2D(_Lux_UnderWaterCaustics, cTexUV * 0.78 + float2(-CausticsTime, -CausticsTime * 0.87) + caustics.gb * 0.1 );
			caustics += tex2D(_Lux_UnderWaterCaustics, cTexUV * 1.13 + float2(CausticsTime, 0.36) - caustics.gb * _Lux_UnderWaterCausticsSelfDistortion );
		
			caustics.r *= saturate(gNormal.y - 0.15) * saturate( (gNormal.y - 0.15) * 2);
			float causticsMask = dirLightCancellation * s.Alpha.x;

			c.rgb += caustics.rrr * causticsMask * _Lux_UnderWaterCausticsScale * _Lux_UnderWaterSunColor.rgb;
		#endif
	#endif

			
	#if !defined(UNITY_PASS_FORWARDADD)  

	//	Fog lighting
		float3 fogLighting = _Lux_UnderWaterSunColor.rgb;
	
	//	Add ambient lighting
		fogLighting += _Lux_UnderWaterAmbientSkyLight.rgb; // gi.indirect.diffuse; //
	//  Light cancellation on fog realtive to camera below water surface
		float3 fogPos = _WorldSpaceCameraPos;
		float depthBelowSurface1 = saturate( ( _Lux_UnderWaterWaterSurfacePos - fogPos.y) / _Lux_UnderWaterFogLightingDepth);
		float depthBelowSurface2 = exp2(-depthBelowSurface1 * depthBelowSurface1 * 8.0);
		fogLighting *= saturate( depthBelowSurface2);

	//	Apply fog	
		c.rgb = lerp(c.rgb, _Lux_UnderWaterFogColor * fogLighting
			#if defined(USEALPHAPREMULTIPLY)
			* c.a
			#endif
			, fogDensity);
	#else
		c.rgb *= (1.0 - fogDensity) ;
	#endif

//	Apply absorption
	c.rgb *= ColorAbsortion;

	#if !defined(UNITY_PASS_FORWARDADD) 
	//	Scattering
		float3 worldViewDir = viewDir;
		float fCos = saturate( dot( _WorldSpaceLightPos0.xyz, -worldViewDir ) );
		// Old: float viewScatter = fCos * fCos * _Lux_UnderWaterUnderwaterScatteringPower;
		// Old: fogLighting *= viewScatter * _Lux_UnderWaterUnderwaterScatteringIntensity + 1.0f;
		float viewScatter = exp2(fCos * _Lux_UnderWaterUnderwaterScatteringPower - _Lux_UnderWaterUnderwaterScatteringPower);
	//	Scatter occlusion
	//	In case particle lighting is enabled we skip occlusion
		#if !defined(GEOM_TYPE_BRANCH)
			viewScatter *= saturate(s.Depth * _Lux_UnderwaterScatteringOcclusion);
		#endif
	#endif

//	Fog lies infront of the object - so it does not depend on transparency	
	#if defined(USEALPHAPREMULTIPLY)
	//	c.a = lerp(c.a, 1, fogDensity);
	//	In case particle lighting is enabled apply final alpha
		#if defined(GEOM_TYPE_BRANCH)
			c *= s.Alpha.yyyy;
		#endif
	#endif

	#if !defined(UNITY_PASS_FORWARDADD)

//	Apply Scattering
		c.rgb +=
			#if defined(USEALPHAPREMULTIPLY)
				/* alpha was missing */ c.a * 
			#endif
			viewScatter * _Lux_UnderWaterUnderwaterScatteringColor * _Lux_UnderWaterUnderwaterScatteringIntensity * fogLighting;

		#if defined(_EMISSION)
			s.Emission = lerp(s.Emission, half3(0,0,0), fogDensity);
			s.Emission *= ColorAbsortion;
		#endif
	#endif


	#if defined(USEALPHAPREMULTIPLY)
		c *= s.Alpha.z;
	#else
		c.a *= s.Alpha.z;
	#endif
	
    return c;
}

inline void LightingStandardSpecularLuxUnderwater_GI(
	SurfaceOutputStandardSpecularLuxUnderwater s,
	UnityGIInput data,
	inout UnityGI gi)
{
	Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, s.Specular);
	gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
}


// --------------------------




#endif // UNITY_PBS_LIGHTING_INCLUDED