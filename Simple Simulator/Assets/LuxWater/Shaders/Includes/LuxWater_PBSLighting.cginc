

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

// Surface shader output structure to be used with physically
// based shading model.

//-------------------------------------------------------------------------------------
// Specular workflow

struct SurfaceOutputStandardSpecularLuxWater
{
	fixed3 Albedo;      // diffuse color
	fixed3 Specular;    // specular color
	fixed3 Normal;      // tangent space normal, if written
	half3 Emission;
	half Smoothness;    // 0=rough, 1=smooth
	half Occlusion;     // occlusion (default 1)

						//  Lux Water specials  

	fixed2 Alpha;       // alpha for transparencies

	fixed FacingSign;
	float ClipSpaceDepth;
	fixed3 worldNormalFace;

	fixed4 Refraction;

	fixed3 Reflections;
	fixed3 ReflectionNormal;
	half ReflectionSmoothness;

	float3 Absorption;
	half4 Foam;
	half Caustics;

	half Translucency;
};


half _FresnelPower;

fixed4 _Lux_UnderWaterAmbientSkyLight;
fixed _ReflectionStrength;

half _WaterIOR;
fixed4 _UnderWaterReflCol;

half _ScatteringPower;
half3 _TranslucencyColor;



float3 nrand3(float2 n) {
	return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * float3(43758.5453, 28001.8384, 50849.4141));
}


// GPU Pro 2, Page 317: https://books.google.de/books?id=zfPRBQAAQBAJ&pg=PA318&lpg=PA318&dq=water+fresnel+term&source=bl&ots=WHdSW0OQvx&sig=l6jFIIrZ7GuQ7ysb5jOS8ZiJHtU&hl=de&sa=X&ved=0ahUKEwjrloKAkO7aAhVMC8AKHf7BBfk4ChDoAQgwMAI#v=onepage&q=water%20fresnel%20term&f=false
// https://www.scratchapixel.com/lessons/3d-basic-rendering/introduction-to-shading/reflection-refraction-fresnel

inline half3 LuxFresnelLerpUnderwater(half3 F90, half cosA) {
	half3 F0 = 0.02037;
	half n1 = 1.000293;   // Air
	half n2 = _WaterIOR;        //1.333333; // Water at rooom temperatur...
	half eta = (n2 / n1);
	half k = eta * eta * (1.0 - cosA * cosA);
	//  As we do not have any real internal reflections
	//    k = saturate(k);
	if (k > 1.0) {     // Total internal Reflection
		return 1.0;
	}
	cosA = sqrt(1.0 - k);
	return lerp(F0, F90, Pow5(1.0 - cosA));
}

inline half3 LuxFresnelLerp(half3 F0, half3 F90, half cosA)
{
	//half t = Pow5 (1 - cosA);   // ala Schlick interpoliation
	half t = pow(1 - cosA, _FresnelPower);
	return lerp(F0, F90, t);
}


inline half4 LightingStandardSpecularLuxWater(SurfaceOutputStandardSpecularLuxWater s, half3 viewDir, UnityGI gi) {

	s.Normal = normalize(s.Normal);

	//  Custom fog
	//  I have no idea why – but Absorption gets not fogged... so we calculate the fogFactor upfront and fade out Absorption.
	/*    #if defined (UNITY_PASS_FORWARDBASE)
	float unityFogFactor = 1;
	#if defined(FOG_LINEAR)
	unityFogFactor = (s.ClipSpaceDepth) * unity_FogParams.z + unity_FogParams.w;
	#elif defined(FOG_EXP)
	unityFogFactor = unity_FogParams.y * (s.ClipSpaceDepth);
	unityFogFactor = exp2(-unityFogFactor);
	#elif defined(FOG_EXP2)
	unityFogFactor = unity_FogParams.x * (s.ClipSpaceDepth);
	unityFogFactor = exp2(-unityFogFactor * unityFogFactor);
	#endif
	#endif
	*/


	half oneMinusReflectivity = 1 - SpecularStrength(s.Specular);
	//  No energy conservation – as s.Albedo contains fog color
	//s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

	half4 c;

	half perceptualRoughness = SmoothnessToPerceptualRoughness(s.Smoothness);
	half3 halfDir = Unity_SafeNormalize(gi.light.dir + viewDir);
	half nv = abs(dot(s.Normal, viewDir));
	half nl = saturate(dot(s.Normal, gi.light.dir));
	half nh = saturate(dot(s.Normal, halfDir));
	half lv = saturate(dot(gi.light.dir, viewDir));
	half lh = saturate(dot(gi.light.dir, halfDir));

	half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
	half V = SmithJointGGXVisibilityTerm(nl, nv, roughness);
	half D = GGXTerm(nh, roughness);
	half specularTerm = V*D * UNITY_PI;
	specularTerm = max(0, specularTerm * nl);

	half3 specularLighting = specularTerm * gi.light.color * FresnelTerm(s.Specular, lh);

	//  Calculate ambient fresnel
	half surfaceReduction;
#ifdef UNITY_COLORSPACE_GAMMA
	surfaceReduction = 1.0 - 0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
#else
	surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
#endif
	half grazingTerm = saturate(s.Smoothness + (1 - oneMinusReflectivity));

	half3 aFresnel;

	//	Check for backside rendering
	bool backside = (s.FacingSign < 0) ? true : false;

	if (backside) {
		aFresnel = LuxFresnelLerpUnderwater(grazingTerm, nv);
	}
	else {
		aFresnel = LuxFresnelLerp(s.Specular, grazingTerm, nv);
	}

	//  ///////////////////////////////////////
	//  Diffuse lighting at the surface = foam
	half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
	half3 diffuseLighting = s.Foam.rgb * s.Foam.a * (gi.indirect.diffuse + gi.light.color * diffuseTerm);

	//  ///////////////////////////////////////
	//  Diffuse lighting for underwater fog
	half diffuse_nl = saturate(dot(half3(0, 1, 0), gi.light.dir));
#if defined (UNITY_PASS_FORWARDBASE)
	//gi.indirect.diffuse is not equal _Lux_AmbientSkyLight   
	half3 diffuseUnderwaterLighting = gi.light.color * diffuse_nl;
	float viewScatter = 1.0 - saturate(dot(gi.light.dir, viewDir) + 1.75);
	diffuseUnderwaterLighting *= 1 + viewScatter * 2;
#if defined(USINGWATERVOLUME)
	diffuseUnderwaterLighting = s.Albedo * (diffuseUnderwaterLighting + _Lux_UnderWaterAmbientSkyLight);
#else
	diffuseUnderwaterLighting = s.Albedo * (diffuseUnderwaterLighting + gi.indirect.diffuse);
#endif
#else
	half3 diffuseUnderwaterLighting = s.Albedo * (gi.light.color * diffuse_nl);
#endif

	//  ///////////////////////////////////////
	//  Combine and apply diffuse lighting

	if (backside) {
		c.rgb = diffuseLighting * (1.0 - s.Refraction.a);
		float fogLightingAdjust = 1.0 - (saturate(0.0 - normalize(viewDir.y)));
		//fogLightingAdjust = lerp(0.25, 20, fogLightingAdjust );
	}
	else {
		c.rgb = diffuseLighting;
	}

	fixed3 origRefraction = s.Refraction.rgb;

	//  Add caustics
	s.Refraction.rgb += s.Caustics * gi.light.color * diffuse_nl * saturate(s.Absorption); // Why did i choose "* diffuse_nl" here?
	if (backside) {
		s.Refraction.rgb *= 1 - aFresnel;
s.Refraction.rgb *= 1 - s.Foam.a;
	}

	//  Add underwater fog
	s.Refraction.rgb = lerp(s.Refraction.rgb, diffuseUnderwaterLighting, s.Refraction.a);

	//  Apply absorption – important: absorption might be neagtive!
	s.Refraction.rgb *= saturate(s.Absorption);
	specularLighting += surfaceReduction * gi.indirect.specular * aFresnel * _ReflectionStrength * ((backside) ? (_UnderWaterReflCol.rgb) : 1.0);

	if (backside) {
		c.rgb += s.Refraction.rgb;
		specularLighting *= (1 - s.Refraction.a);
	}
	else {
		c.rgb += saturate(1.0 - s.Foam.a) * s.Refraction.rgb * (1.0 - aFresnel);
	}
	//  ///////////////////////////////////////
	//  Combine and apply specular lighting
	c.rgb += specularLighting;

	//  ///////////////////////////////////////
	//  Add translucent lighting
	half3 lightScattering = 0;
	//  https://colinbarrebrisebois.com/2012/04/09/approximating-translucency-revisited-with-simplified-spherical-gaussian/
	half3 transLightDir = gi.light.dir - s.Normal * 0.1;
	half transDot = dot(-transLightDir, viewDir);
	transDot = exp2(saturate(transDot) * _ScatteringPower - _ScatteringPower);
// TODO: Check this.
	//  Using abs to get some translucency when rendering underwater
	transDot *= saturate((1.0 - abs(s.Normal.y)) * 2);
	lightScattering = transDot * gi.light.color * s.Translucency;
// TODO: Check factor 10.
	c.rgb += saturate(lightScattering * _TranslucencyColor * 10.0f );

// Translucent fog
	//c.rgb += transDot * gi.light.color * s.Albedo * s.Foam.a * 1.0f * backside;

	c.rgb = lerp(origRefraction, c.rgb, s.Alpha.y);
	c.a = s.Alpha.x;

	//	Custom fog
#if !defined (UNITY_PASS_FORWARDADD)
	if (!backside) {
		float unityFogFactor = 1;
#if defined(FOG_LINEAR)
		unityFogFactor = (s.ClipSpaceDepth) * unity_FogParams.z + unity_FogParams.w;
#elif defined(FOG_EXP)
		unityFogFactor = unity_FogParams.y * (s.ClipSpaceDepth);
		unityFogFactor = exp2(-unityFogFactor);
#elif defined(FOG_EXP2)
		unityFogFactor = unity_FogParams.x * (s.ClipSpaceDepth);
		unityFogFactor = exp2(-unityFogFactor * unityFogFactor);
#endif
		c.rgb = lerp(unity_FogColor.rgb, c.rgb, saturate(unityFogFactor));
	}
#endif

	return c;
}

inline void LightingStandardSpecularLuxWater_GI(
	SurfaceOutputStandardSpecularLuxWater s,
	UnityGIInput data,
	inout UnityGI gi)
{

	// Planar reflections
#if defined (GEOM_TYPE_MESH)
	// We do not need to look up the cube map here.
	// Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.ReflectionNormal, s.Specular);
	gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
	gi.indirect.specular = s.Reflections;
#else
	Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.ReflectionSmoothness, data.worldViewDir, s.ReflectionNormal, s.Specular);
	gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
#endif
}

#endif // UNITY_PBS_LIGHTING_INCLUDED
