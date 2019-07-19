// expects first normal to be unpacked already
half3 UnpackAndBlendNormals(fixed3 n1, fixed4 n2, fixed4 n3) {
    half3 normal;
    #if defined(UNITY_NO_DXT5nm)
        normal = normalize( n1 + (n2.xyz * 2 - 1) + (n3.xyz * 2 - 1) );
    #else
        normal.xy =  n1.xy;
        normal.xy += (n2.ag * 2 - 1) * _BumpScale.y;
        normal.xy += (n3.ag * 2 - 1) * _BumpScale.z;
        normal.z = sqrt(1.0 - saturate( normal.x * normal.x + normal.y * normal.y));
        normal = normalize(normal);
    #endif
    return normal;
}

half3 WorldNormal(half3 t0, half3 t1, half3 t2, half3 normal) {
//	normalize is done once in the fragment shader
    return /*normalize*/( half3( dot(t0, normal), dot(t1, normal), dot(t2, normal) ) );
}

float2 rotate2D(float2 v, float a) {
    float s = sin(a);
    float c = cos(a);
    float2x2 m = float2x2(c, -s, s, c);
    return mul(m, v);
}

//	Shadow Helpers -------------------------------------------------

	UNITY_DECLARE_SHADOWMAP(_LuxParticles_CascadedShadowMap);

	float computeShadowFadeDistance(float3 wpos, float z) {
    	float sphereDist = distance(wpos, unity_ShadowFadeCenterAndType.xyz);
    	return lerp(z, sphereDist, unity_ShadowFadeCenterAndType.w);
	}
	half computeShadowFade(float fadeDist) {
	    return saturate(fadeDist * _LightShadowData.z + _LightShadowData.w);
	}

	inline fixed4 getCascadeWeights_splitSpheres(float3 wpos) {
		float3 fromCenter0 = wpos.xyz - unity_ShadowSplitSpheres[0].xyz;
		float3 fromCenter1 = wpos.xyz - unity_ShadowSplitSpheres[1].xyz;
		float3 fromCenter2 = wpos.xyz - unity_ShadowSplitSpheres[2].xyz;
		float3 fromCenter3 = wpos.xyz - unity_ShadowSplitSpheres[3].xyz;
		float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1), dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));
		fixed4 weights = float4(distances2 < unity_ShadowSplitSqRadii);
		weights.yzw = saturate(weights.yzw - weights.xyz);
		return weights;
	}

	inline float4 getShadowCoord(float4 wpos, fixed4 cascadeWeights) {
		float3 sc0 = mul(unity_WorldToShadow[0], wpos).xyz;
		float3 sc1 = mul(unity_WorldToShadow[1], wpos).xyz;
		float3 sc2 = mul(unity_WorldToShadow[2], wpos).xyz;
		float3 sc3 = mul(unity_WorldToShadow[3], wpos).xyz;
		float4 shadowMapCoordinate = float4(sc0 * cascadeWeights[0] + sc1 * cascadeWeights[1] + sc2 * cascadeWeights[2] + sc3 * cascadeWeights[3], 1);
		#if defined(UNITY_REVERSED_Z)
			float  noCascadeWeights = 1 - dot(cascadeWeights, float4(1, 1, 1, 1));
			shadowMapCoordinate.z += noCascadeWeights;
		#endif
		return shadowMapCoordinate;
	}

	float GetLightAttenuation(float3 wpos) {
	//	Directional Lights only
		float atten = 1;
		fixed4 cascadeWeights = getCascadeWeights_splitSpheres(wpos);
		float4 coord = getShadowCoord(float4(wpos, 1), cascadeWeights);
		atten = UNITY_SAMPLE_SHADOW(_LuxParticles_CascadedShadowMap, coord.xyz);
	//	Fade out shadows
	    float zDist = dot(_WorldSpaceCameraPos - wpos, UNITY_MATRIX_V[2].xyz);
	    float fadeDist = computeShadowFadeDistance(wpos, zDist);
	    half  realtimeShadowFade = computeShadowFade(fadeDist);
		atten = lerp (_LightShadowData.r, 1.0f, saturate(atten + realtimeShadowFade));
		return atten;
	}

// https://forum.unity.com/threads/fixing-screen-space-directional-shadows-and-anti-aliasing.379902/
// Thanks bgolus once more!
float MSSADepth(float fragDepth, float sceneDepth, float depth_raw, float2 screenUV) {

	// this is good far distances but bad at close distances
 	// screenUV.xy = (screenUV.xy * (_CameraDepthTexture_TexelSize.zw - 1.0) + 0.5f) * _CameraDepthTexture_TexelSize.xy;
	// this is bad too
	// screenUV = (floor(screenUV * _CameraDepthTexture_TexelSize.zw) + 0.5) / _CameraDepthTexture_TexelSize.zw;
	// Testing, testing, testing - and finding a good compromise...
	// screenUV.x -= _CameraDepthTexture_TexelSize.x;

	float depthDiff = abs(fragDepth - sceneDepth); //LinearEyeDepth(depth_raw));

//	The if does not safe much but stabilizes results (jittering)
	if (depthDiff < 20.0) { 
	//	Would help the 0,1 offsets
		//screenUV.x -= 0.975 * _CameraDepthTexture_TexelSize.x;

		float2 texelSize = _CameraDepthTexture_TexelSize.xy;
		float4 offsetDepths = 0;

		//	https://github.com/TheRealMJP/MSAAFilter/blob/master/MSAAFilter/Resolve.hlsl
		//	These offsets are not very stable
		#define FACTOR 1.0f
		float2 uvOffsetsX[5] = {
			float2(-0.125f, -0.375f) * FACTOR * texelSize,
			float2(0.375f, -0.125f) * FACTOR * texelSize,
			float2(-0.375f,  0.125f) * FACTOR * texelSize,
			float2(0.125f,  0.375f) * FACTOR * texelSize,
			float2(0.0, 0.0)
		};

		// These offsets create strange jagged artifacts
		float2 uvOffsets[5] = {
			float2(1.0, 0.0) * texelSize,
			float2(-1.0, 0.0) * texelSize,
			float2(0.0, 1.0) * texelSize,
			float2(0.0, -1.0) * texelSize,
			float2(0.0, 0.0)
		};

		offsetDepths.x = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(screenUV + uvOffsets[0], 0, 0));
		offsetDepths.y = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(screenUV + uvOffsets[1], 0, 0));
		offsetDepths.z = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(screenUV + uvOffsets[2], 0, 0));
		offsetDepths.w = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(screenUV + uvOffsets[3], 0, 0));

		float4 offsetDiffs = abs(fragDepth - offsetDepths);
		float diffs[4] = { offsetDiffs.x, offsetDiffs.y, offsetDiffs.z, offsetDiffs.w };

		int lowest = 0;
		float tempDiff = depthDiff;
		for (int i = 0; i < 4; i++) {

// TODO: why is it flipped - at least when using perspective?
		#if defined(UNITY_REVERSED_Z)
			if (diffs[i] > tempDiff) {        // DX11
		#else
			if (diffs[i] < tempDiff) {        // OpenGL
		#endif
				tempDiff = diffs[i];
				lowest = i;
			}
		}
		return SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, float4(screenUV + uvOffsets[lowest], 0, 0));
	}
	else {
		return depth_raw;
	}
}