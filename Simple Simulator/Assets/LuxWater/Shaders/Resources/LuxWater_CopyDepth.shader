Shader "Hidden/Lux Water/CopyDepth"
{
	Properties { }
	SubShader
	{

		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target {			
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				#if defined(UNITY_REVERSED_Z)
				//		depth = 1.0f - depth;
				#endif
				return float4(depth.xxx, 1);
			}
			ENDCG
		}
	}
}
