// Regular "Particles/Alpha Blended" - without regular but underwater fog

Shader "Lux Water/Particles/UnderwaterParticles Alpha Blended" {
Properties {
    _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Particle Texture", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;

            //  LuxWater: Additional inputs for the fragment shader
                float4 projPos : TEXCOORD2;
                float4 fogLighting : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                fixed3 scatter : TEXCOORD5;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;


        //  LuxWater: Include the UnderwaterFog file - this must be done after all structs are defined
            #include "../Includes/LuxWater_UnderwaterParticleFog.cginc"

            v2f vert (appdata_t v) {
                
            //  LuxWater: Here we have to setup v2f using: (v2f)0
                v2f o = (v2f)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
            
            //  LuxWater: We always need this
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);

                o.color = v.color * _TintColor;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

            //  LuxWater: Call SetupVertexFoglighting which sets up all missing members of v2f
                SetupVertexFoglighting (v, o);

                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef SOFTPARTICLES_ON
	                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
	                float partZ = i.projPos.z;
	                float fade = saturate (_InvFade * (sceneZ-partZ));
	                i.color.a *= fade;
                #endif

                fixed4 col = 2.0f * i.color * tex2D(_MainTex, i.texcoord);


            //  LuxWater: Add underwater fog and absorption
                col.rgb = LuxWater_ParticleFog( col.rgb, i.fogLighting, i.worldPos, i.projPos, i.scatter);
                return col;
            }
            ENDCG
        }
    }
}
}
