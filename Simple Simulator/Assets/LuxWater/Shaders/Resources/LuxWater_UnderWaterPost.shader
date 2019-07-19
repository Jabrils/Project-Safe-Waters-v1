Shader "Hidden/Lux Water/UnderWaterPost" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		//_UnderWaterTex ("UnderWater", 2D) = "white" {}
		_Density ("Density", Range(0,10)) = .05
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off ZWrite Off ZTest Always 

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

		//	Caustics
			#pragma multi_compile __ GEOM_TYPE_FROND
		//	Caustic Normal Mode
			#pragma multi_compile __ GEOM_TYPE_LEAF
		//	Deep water lighting - not needed
			//#pragma multi_compile __ LUXWATER_DEEPWATERLIGHTING

			#pragma target 3.0
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
//				uint id : SV_VertexID;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 interpolatedRay : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _UnderWaterTex;
			sampler2D _UnderWaterMask;
			sampler2D _CameraDepthTexture;

			float4x4 _Lux_FrustumCornersWS;
			//float4x4 _Lux_FrustumCornersWS2ndEye;
			float4 _Lux_CameraWS;

			
			CBUFFER_START(LuxUnderwater)
			//	Lighting	
				fixed3 _Lux_UnderWaterSunColor;
				float3 _Lux_UnderWaterSunDir;
				fixed3 _Lux_UnderWaterAmbientSkyLight;

				float4 _Lux_UnderWaterSunDirViewSpace;
				
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

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// #if UNITY_UV_STARTS_AT_TOP
    			//	o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
				// #endif
				o.interpolatedRay = lerp( _Lux_FrustumCornersWS[0], _Lux_FrustumCornersWS[1], o.uv.x);
				o.interpolatedRay = lerp( o.interpolatedRay, lerp(_Lux_FrustumCornersWS[2], _Lux_FrustumCornersWS[3],o.uv.x), o.uv.y);
				return o;
			}

		//	Actually is up to 3 times faster than hash32 (https://www.shadertoy.com/view/MslGR8)
			float3 nrand3(float2 seed) {
				return frac(sin(dot(seed.xy, float2(12.9898, 78.233))) * float3(43758.5453, 28001.8384, 50849.4141));
			}



			half4 frag (v2f i) : SV_Target {

			//	Gets rendered twice, so uvs match the eyes automatically.
			/*
				float2 sampleUV = i.uv;
					#if defined(UNITY_SINGLE_PASS_STEREO)
						// right eye
						if (unity_StereoEyeIndex == 1) {
							sampleUV.x = (sampleUV.x * 0.5) + 0.5;
						}
						// left eye
						else {
							sampleUV.x = (sampleUV.x * 0.5);
						}
				#endif
			*/

			//	Add some noise to reduce color bending.
				float3 noise = 0;
				#if !defined(SHADER_API_D3D9)
					float2 seed = i.vertex.xy * _ScreenParams.zw;
					noise = nrand3(seed);
				#endif

				half4 col = tex2D(_MainTex, i.uv);
				half4 bground = col;

				half4 underwatermask = tex2D(_UnderWaterMask, i.uv);

				float sceneDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
				float origSceneDepth = sceneDepth;

			//	watersurfacefrombelowdepth comes in as Linear01Depth
				float watersurfacefrombelowdepth = DecodeFloatRG(underwatermask.ba); 

			//	Cancel caustics: when looking at the undersurface we have to check if the scene is "inside" the water volume.
			//	Keep an ey on the epsilon!
				float cCancel = (underwatermask.g < 1 &&  origSceneDepth   - _ProjectionParams.w * 4  /* - 0.01  */ > watersurfacefrombelowdepth) ? 0 : 1;
				cCancel *= saturate( underwatermask.g * 8); 

			//	Combine sceneDepth and depth of watersurface based on mask
				sceneDepth = (underwatermask.g < 1.0) ? min(watersurfacefrombelowdepth, sceneDepth) : sceneDepth;

			//	Dither sceneDepth - depends on far clipping plane, so should we take this into account?
			//	Dithering sceneDepth breaks caustics --> dither dist instead
			//	#if defined(UNITY_REVERSED_Z)
			//		sceneDepth += (noise.x - 0.5) * 2 * (0.01 * ( 1.0 - _ProjectionParams.w) ) * sceneDepth;
			//	#else
			//		sceneDepth -= (noise.x - 0.5) * 2 * (0.005 * ( 1.0 - _ProjectionParams.w) ); // * (1.0f - sceneDepth);
			//	#endif

			//	Distance to eye in units
				float dist = sceneDepth * _ProjectionParams.z;
				//dist -= _ProjectionParams.y;

			//	Reconstruct world position of refracted pixel
				float4 wDir = sceneDepth * i.interpolatedRay;
				float4 wPos = _Lux_CameraWS + wDir;

			//	Dither input
				dist += dist * (noise.x - 0.5) * 2 * .05;

			//	Underwater fog
			//	Along view
				float fogDensity = (1.0 - saturate( exp( -dist * _Lux_UnderWaterFogDensity ) ) );
			//	Depth along the y axis
				float depthAtten = ( saturate( (_Lux_UnderWaterWaterSurfacePos - wPos.y - _Lux_UnderWaterFogDepthAtten.x)  / (_Lux_UnderWaterFogDepthAtten.y) ) );
				depthAtten = saturate( 1.0 - exp( -depthAtten * 8.0)  ) * saturate(_Lux_UnderWaterFogDepthAtten.z); 
				fogDensity = max(fogDensity, depthAtten);

			//	Dither output - not so good
				//fogDensity += fogDensity * (noise.x - 0.5) * 2 * 0.005;

			//	Absorption	
				float3 ColorAbsortion = float3(0.45f, 0.029f, 0.018f);
			//	Calculate Depth Attenuation
				float depthBelowSurface = saturate( (_Lux_UnderWaterWaterSurfacePos - wPos.y) / _Lux_UnderWaterAbsorptionMaxHeight);
				depthBelowSurface = exp2(-depthBelowSurface * depthBelowSurface * _Lux_UnderWaterAbsorptionHeight);
			//	Calculate Attenuation along viewDirection
				float d = exp2( -dist * _Lux_UnderWaterAbsorptionDepth);
			//	Combine and apply strength
				d = lerp (1, saturate( d * depthBelowSurface), _Lux_UnderWaterAbsorptionStrength );
			//	Cancel absorption by fog
				d = saturate(d + fogDensity * _Lux_UnderWaterFogAbsorptionCancellation);	
			//	Add color absorption
				ColorAbsortion = lerp( d, -ColorAbsortion, _Lux_UnderWaterAbsorptionColorStrength * (1.0 - d));
				ColorAbsortion = saturate(ColorAbsortion);

			//	Set mask
				col.a = saturate(underwatermask.g * 8) * (1.0 - underwatermask.r) ;
				//col.a = cCancel;

			//  Light cancellation on caustics relative to the water surface
			    float depthBelowSurfaceC = saturate( ( _Lux_UnderWaterWaterSurfacePos - wPos.y ) / _Lux_UnderWaterDirLightingDepth);
			    float directionalLightCancellation = saturate( exp(-depthBelowSurfaceC * depthBelowSurfaceC * 8.0) );

			//	Add caustics
				#if defined(GEOM_TYPE_FROND)
					float2 cTexUV = wPos.xz * _Lux_UnderWaterCausticsTiling;
					half4 caustics = tex2D(_Lux_UnderWaterCaustics, cTexUV + _Time.x * 0.5 );
				//	animate			
					float CausticsTime = _Time.x * _Lux_UnderWaterCausticsSpeed;

					#if defined(GEOM_TYPE_LEAF)
						half3 gNormal = tex2D(_CameraGBufferTexture2, i.uv).rgb;
						gNormal = gNormal * 2 - 1;
					#else
						half3 gNormal = normalize(cross(ddx(wPos), ddy(wPos)));
						gNormal.y = -gNormal.y;
					#endif
				
					caustics =  tex2D(_Lux_UnderWaterCaustics, cTexUV + CausticsTime.xx * _Lux_UnderWaterFinalBumpSpeed01.xy);
					caustics += tex2D(_Lux_UnderWaterCaustics, cTexUV * 0.78 + float2(-CausticsTime, -CausticsTime * 0.87) * _Lux_UnderWaterFinalBumpSpeed01.xy + caustics.gb * _Lux_UnderWaterCausticsSelfDistortion );
					caustics += tex2D(_Lux_UnderWaterCaustics, cTexUV * 1.13 + float2(CausticsTime, 0.36) * _Lux_UnderWaterFinalBumpSpeed01.xy - caustics.gb * _Lux_UnderWaterCausticsSelfDistortion );
				
					//caustics.r *= saturate( (gNormal.y - 0.125) * 2);
					caustics.r *= saturate(gNormal.y - 0.15) * saturate( (gNormal.y - 0.15) * 2);

				//	This projects caustics also on the undersurface of the water – might be nice tho
					float causticsMask = cCancel; //(wPos.y > _Lux_UnderWaterWaterSurfacePos - 0.01) ? 0 : 1;

					
				//	Add caustics and cancel them by light and fog
					col.rgb += caustics.rrr * causticsMask * _Lux_UnderWaterCausticsScale * _Lux_UnderWaterSunColor.rgb 
							   * directionalLightCancellation;
							//	We lerp towards fog - no need to multiply here
							//  * (1 - fogDensity);
				#endif

			// 	_Lux_SunColor.rgb already contains ndotl!
				float3 fogLighting = _Lux_UnderWaterSunColor.rgb + _Lux_UnderWaterAmbientSkyLight.rgb;
			
			//	Scattering according to view and sun dir
				//float fCos = saturate ( dot(_Lux_UnderWaterSunDir, -normalize(i.interpolatedRay.xyz) ) );
				//float fCos = saturate ( dot(_Lux_UnderWaterSunDir, -normalize(i.interpolatedRay.xyz + noise * 16.0f ) ) ); // 200 = visible, 100 = also
				float fCos = saturate ( dot( normalize(_Lux_UnderWaterSunDir + (noise - 0.5) * 2 * 0.0075) , -normalize(i.interpolatedRay.xyz) ) );

				float viewScatter = exp2(fCos * _Lux_UnderWaterUnderwaterScatteringPower - _Lux_UnderWaterUnderwaterScatteringPower);
				viewScatter *= saturate(dist * _Lux_UnderwaterScatteringOcclusion);

				float3 fogPos = _Lux_CameraWS;
				float depthBelowSurface1 = saturate( ( _Lux_UnderWaterWaterSurfacePos - fogPos.y) / _Lux_UnderWaterFogLightingDepth);
				float depthBelowSurface2 = exp2(-depthBelowSurface1 * depthBelowSurface1 * 8.0);
				fogLighting *= saturate( depthBelowSurface2);

			//	Apply underwater fog
				col.rgb = lerp(col.rgb, _Lux_UnderWaterFogColor.rgb * fogLighting, fogDensity.xxx);
			//	Apply absorption
				col.rgb *= ColorAbsortion;
			//	Apply Scattering
				col.rgb += viewScatter * _Lux_UnderWaterUnderwaterScatteringColor * _Lux_UnderWaterUnderwaterScatteringIntensity * fogLighting;

			//	Add some more noise to reduce color bending.
				#if !defined(SHADER_API_D3D9)
					float3 offset = noise * 2 - 1;
					float offsetLum = dot(offset, float3(0.2126f, 0.7152f, 0.0722f));
					col.rgb = float3(col.rgb) + offsetLum / 255.0f * 0.25f;
				#endif

//col.rgb = fogLighting * _Lux_UnderWaterUnderwaterScatteringIntensity;
//col.rgb = viewScatter; //


				col.rgb = lerp(bground.rgb, col.rgb, col.a);



			//	Godrays
			//	_Lux_UnderWaterSunDir - here it must be in viewspace
				//half2 vec = _Lux_UnderWaterSunDirViewSpace.xy - i.uv;
				//half sdist = saturate(0.75 - length(vec.xy));
				//col.rgb = sdist;

				return col;
			}
			ENDCG
		}


	/// final composition underwater blur
	// No adjustments for single pass stereo needed
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _UnderWaterTex;
			float4 _UnderWaterTex_TexelSize;

			sampler2D _UnderWaterMask;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

		//	Random function
			float3 nrand3(float2 n) {
				return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * float3(43758.5453, 28001.8384, 50849.4141));
			}

			float Dither17(float2 n) {
				uint2 k0 = uint2(2, 7);
				float Ret = dot(n, k0 / 17.0f);
				return frac(Ret);
			}

			fixed4 frag (v2f i) : SV_Target {

				fixed4 col = tex2D(_MainTex, (i.uv) );

			//	4-tap bilinear upsampling - expensive and not worth it.
			/*	float4 d = _UnderWaterTex_TexelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0); 
				
				fixed4 underWater = tex2D(_UnderWaterTex, i.uv + d.xy);
				underWater += tex2D(_UnderWaterTex, i.uv + d.zy);
				underWater += tex2D(_UnderWaterTex, i.uv + d.xw);
				underWater += tex2D(_UnderWaterTex, i.uv + d.zw);
				underWater *= 0.25;
			*/

				fixed4 underWater = tex2D(_UnderWaterTex, (i.uv) );
				fixed4 underwatermask = tex2D(_UnderWaterMask, (i.uv) );

			//	Add some noise to reduce color bending.
				#if !defined(SHADER_API_D3D9)
					float2 seed = i.vertex.xy * _ScreenParams.zw;
					float3 offset = nrand3(seed) * 2 - 1;
					float offsetLum = dot(offset, float3(0.2126f, 0.7152f, 0.0722f));
					underWater.rgb = float3(underWater.rgb) + offsetLum / 255.0f * 0.5f;
				#endif
				

				col.rgb = lerp(col.rgb, underWater.rgb,
					saturate( (underwatermask.g * 8) * (1.0 - underwatermask.r) )
				);
				return col;
			}
			ENDCG
		}
	}
}
