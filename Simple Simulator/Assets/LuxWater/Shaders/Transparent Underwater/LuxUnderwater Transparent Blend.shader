// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Lux Water/Underwater/Transparent Blend" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB) Alpha (A)", 2D) = "white" {}

		[Header(Particle Options)]
		[Toggle(GEOM_TYPE_BRANCH)]
		_VertexColorsEnabled 			("    Enable Particle Shading", Float) = 0
		_InvFade 						("    Soft Particles Factor", Range(0.01,3.0)) = 1.0
		
		[Header(Specular Gloss)]
		[Toggle(_SPECGLOSSMAP)]
		_SpecGlossEnabled 				("    Enable Spec Gloss Map", Float) = 0
		[NoScaleOffset] _SpecGlossMap 	("    Specular (RGB) Smoothness (A)", 2D) = "gray" {}
		_Glossiness 					("Smoothness (Multiplier)", Range(0.0, 1.0)) = 0.5
		_SpecColor 						("Specular", Color) = (0.2,0.2,0.2)
		[Toggle(GEOM_TYPE_LEAF)]
		_DielectricFresenEnabled 		("Use proper dielectric Fresnel", Float) = 0

		[Header(Normal)]
		[Toggle(_NORMALMAP)]
		_NormalEnabled 					("    Enable Normal Map", Float) = 0
		[NoScaleOffset] _BumpMap 		("    Normal Map", 2D) = "bump" {}
		_BumpScale 						("    Scale", Float) = 1.0

		[Header(Emission)]
		[Toggle(_EMISSION)]
		_EmissionEnabled 				("    Enable Emission Map", Float) = 0
		[NoScaleOffset] _EmissionMap 	("    Emission", 2D) = "white" {}
		[HDR] _EmissionColor 			("    Color", Color) = (1,1,1)

		[Header(Caustics)]
		[Toggle(GEOM_TYPE_FROND)]
		_CausticsEnabled 				("    Enable Caustics", Float) = 1

		[Header(Fade)]
    	_WatersurfFadeDistance  		("    Watersurf Distance Fade ", float) = 8.0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "ForceNoShadowCasting" = "True"}
		LOD 200

		CGPROGRAM
		#pragma surface surf StandardSpecularLuxUnderwater fullforwardshadows 	nofog   vertex:vert   alpha:blend   nometa
		#pragma target 3.0

		#pragma shader_feature _NORMALMAP
		#pragma shader_feature _SPECGLOSSMAP
		#pragma shader_feature _EMISSION
	//	Caustics
		#pragma shader_feature GEOM_TYPE_FROND
	//	Enable particle features such as Vertex Colors
		#pragma shader_feature GEOM_TYPE_BRANCH

		// #define USEALPHAPREMULTIPLY // do not define this here as we want to use alpha blending
		#include "../Includes/LuxUnderwater_PBSLighting.cginc"

		sampler2D _MainTex;

		#if defined(_NORMALMAP)
			sampler2D _BumpMap;
			float _BumpScale;
		#endif

		#if defined(_SPECGLOSSMAP)
			sampler2D _SpecGlossMap;
		#endif
		
	//	Particle Lighting
		#if defined(GEOM_TYPE_BRANCH)
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;
		#endif

		#if defined(_EMISSION)
			sampler2D _EmissionMap;
			half3 _EmissionColor;
		#endif

		struct Input {
			float2 uv_MainTex;

		//	In case we enable particle options we need vertex colors and the projPos to sample the depth texture
			#if defined(GEOM_TYPE_BRANCH)
				fixed4 color : COLOR;
				float4 projPos;
			#endif

		//	Needed by LuxUnderwater Lighting
			float3 worldPos;
			float depth;
		};

		half _Glossiness;
		fixed4 _Color;
		float _WatersurfFadeDistance;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

		//	Needed by LuxUnderwater Lighting
		 	COMPUTE_EYEDEPTH(o.depth);

		//	Particle lighting
		 	#if defined(GEOM_TYPE_BRANCH)
				o.projPos = ComputeScreenPos ( UnityObjectToClipPos(v.vertex) );
			#endif
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecularLuxUnderwater o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			#if defined(GEOM_TYPE_BRANCH)
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
	            float partZ = IN.projPos.z;
	            float fade = saturate (_InvFade * (sceneZ-partZ));
				c *= IN.color;
				c.a *= fade;
			#endif

			o.Albedo = c.rgb;
			o.Alpha = c.a;

			#if defined(_SPECGLOSSMAP)
				fixed4 specGloss = tex2D(_SpecGlossMap, IN.uv_MainTex);
				o.Smoothness = specGloss.a * _Glossiness;
				o.Specular = specGloss.rgb;
			#else
				o.Smoothness = _Glossiness;
				o.Specular = _SpecColor;
			#endif

			#if defined(_NORMALMAP)
				o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
			#endif

			#if defined(_EMISSION)
				o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _EmissionColor.rgb;
			#endif

		//  Fade in if the camera is below the water surface
            float depthFade = saturate( (_Lux_UnderWaterWaterSurfacePos - _WorldSpaceCameraPos.y) / _WatersurfFadeDistance );
            o.Alpha.z = depthFade;
            o.Smoothness *= depthFade;
            o.Specular *= depthFade;
            #if defined(_EMISSION)
            	o.Emission *= depthFade;
            #endif

		//	Needed by LuxUnderwater Lighting
			o.Depth = IN.depth;
			o.WorldPos = IN.worldPos;
		}

		ENDCG
	}
}
