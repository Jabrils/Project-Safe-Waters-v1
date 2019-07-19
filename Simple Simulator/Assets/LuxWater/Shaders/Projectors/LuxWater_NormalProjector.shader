Shader "Lux Water/Projectors/Normal Projector"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Culling ("Culling", Float) = 0

		[Space(5)]
		_MainTex ("Normal (RG) Mask (B) Height (A)", 2D) = "bump" {}
		_Strength ("Normal Strength", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
		LOD 100
	//	Alpha blending: Needs special textures.
		// Blend SrcAlpha OneMinusSrcAlpha
	//	Additive blending: Needs rt half texture.
		Blend SrcAlpha One

		ZTest [_ZTest]
		ZWrite Off
		Cull [_Culling]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma exclude_renderers d3d9 d3d11_9x
			
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				half3 tangentToWorld[3] : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Strength;

			half3x3 CreateTangentToWorldPerVertex(half3 normal, half3 tangent, half tangentSign)
			{
			    // For odd-negative scale transforms we need to flip the sign
			    half sign = tangentSign * unity_WorldTransformParams.w;
			    half3 binormal = cross(normal, tangent) * sign;
			    return half3x3(tangent, binormal, normal);
			}

			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float3 normalWorld = float3( 0, 1, 0); // UnityObjectToWorldNormal(v.normal);
				float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
				o.tangentToWorld[0].xyz = tangentToWorld[0];
        		o.tangentToWorld[1].xyz = tangentToWorld[1];
        		o.tangentToWorld[2].xyz = tangentToWorld[2];

				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				//col.rgb = UnpackNormal(col.aggg);

				//half worldN = PerPixelWorldNormal()
				//worldN.x = dot(i.tangentToWorld[0].xyz, col.rgb );
  				//worldN.y = dot(i.tangentToWorld[1].xyz, col.rgb );
  				//worldN.z = dot(i.tangentToWorld[2].xyz, col.rgb );

  				half3 tangent = i.tangentToWorld[0].xyz;
			    half3 binormal = i.tangentToWorld[1].xyz;
			    half3 normal = i.tangentToWorld[2].xyz;

			    normal = normalize (normal);
				// ortho-normalize Tangent
			    tangent = normalize (tangent - normal * dot(tangent, normal));
		        // recalculate Binormal
		        half3 newB = cross(normal, tangent);
		        binormal = newB * sign (dot (newB, binormal));

		        //half3 normalTangent = col.rgb * 2 - 1; //UnpackNormal(col.aggg);
		        //half3 normalWorld = normalize (tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z);

			//	Tangentspace rotataion
				float3x3 rotation = float3x3( tangent, binormal, normal );
				rotation = float3x3( tangent, binormal, normal );
			//	Unpack and rotate normal
				// Alpha blended custom normal
				// col.rgb = mul( col.rgb * 2 - 1, rotation);

				// Regular normal and ARGBHalf
//				col.rgb = mul(UnpackNormal(col), rotation);


			// As we can't sample a smooth normal (rgb does not know a value of 0.5)
				const half correction = 0.25/255;
				col.r -= correction;
				col.g += correction;

				half mask = col.b;

			//	Unpack normal
				col.rgb = mul( (col.rgb * 2 - 1), rotation);
				
			//	Swizzle components
				col.rgb = col.rbg;
			//	Make normal better fit the given tangent space
				col.rg *= -1;

			//	Copy height into blue channel
				col.b = col.a; // in case we want to go up and down: col.b = (col.a - 0.5) * 2;

			//	Pack normals - not needed if we use ARGBHalf
				// col.rgb = col.rgb * 0.5 + 0.5;

			//	Set blending
				col.a = saturate(i.color.a * _Strength             * mask);

			//	Tangent space normals
				//col.rgb = lerp( fixed3(0.5, 0.5, 1), col.rgb, _Strength);
				return col;
				//return half4( (col.rgb * 2 - 1) * col.a, col.a);
			}
			ENDCG
		}
	}
}
