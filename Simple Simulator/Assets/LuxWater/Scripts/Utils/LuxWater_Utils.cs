using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//using System.Runtime.CompilerServices;
//#pragma warning disable 0660, 0661


static public class LuxWaterUtils {


	public struct GersterWavesDescription {
		public Vector3 intensity;
		public Vector4 steepness;
		public Vector4 amp;
		public Vector4 freq;
		public Vector4 speed;
		public Vector4 dirAB;
		public Vector4 dirCD;
		public Vector4 secondaryWaveParams;
	}


	static public void GetGersterWavesDescription (ref GersterWavesDescription Description, Material WaterMaterial ) {
		Description.intensity = WaterMaterial.GetVector("_GerstnerVertexIntensity");
		Description.steepness = WaterMaterial.GetVector("_GSteepness");
		Description.amp = WaterMaterial.GetVector("_GAmplitude");
		Description.freq = WaterMaterial.GetVector("_GFinalFrequency");
		Description.speed = WaterMaterial.GetVector("_GFinalSpeed");
		Description.dirAB = WaterMaterial.GetVector("_GDirectionAB");
		Description.dirCD = WaterMaterial.GetVector("_GDirectionCD");
		Description.secondaryWaveParams = WaterMaterial.GetVector("_GerstnerSecondaryWaves");
	}

	static public Vector3 InternalGetGestnerDisplacement (
		Vector2 xzVtx, Vector4 intensity, Vector4 steepness, Vector4 amp, Vector4 freq, Vector4 speed, Vector4 dirAB, Vector4 dirCD, float TimeOffset
	) {

		Vector4 AB;
		Vector4 CD;
		
	//	half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
		AB.x = steepness.x * amp.x * dirAB.x;
		AB.y = steepness.x * amp.x * dirAB.y;
		AB.z = steepness.y * amp.y * dirAB.z;
		AB.w = steepness.y * amp.y * dirAB.w;

	//	half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
		CD.x = steepness.z * amp.z * dirCD.x;
		CD.y = steepness.z * amp.z * dirCD.y;
		CD.z = steepness.w * amp.w * dirCD.z;
		CD.w = steepness.w * amp.w * dirCD.w;


	//	half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
		Vector4 dotABCD;
		dotABCD.x = freq.x * (dirAB.x * xzVtx.x + dirAB.y * xzVtx.y);
		dotABCD.y = freq.y * (dirAB.z * xzVtx.x + dirAB.w * xzVtx.y);
		dotABCD.z = freq.z * (dirCD.x * xzVtx.x + dirCD.y * xzVtx.y);
		dotABCD.w = freq.w * (dirCD.z * xzVtx.x + dirCD.w * xzVtx.y);
		
		Vector4 TIME;
	//	In case we do not use underwater rendering
		float time = Time.timeSinceLevelLoad + TimeOffset; //  Shader.GetGlobalVector("_Time").y + TimeOffset ; //Time.time; //timeSinceLevelLoad; //.time
	//	half4 TIME = _Time.yyyy * speed;
		TIME.x = time * speed.x;
		TIME.y = time * speed.y;
		TIME.z = time * speed.z;
		TIME.w = time * speed.w;

		Vector4 COS;
	//	half4 COS = cos (dotABCD + TIME);
		dotABCD.x += TIME.x;
		dotABCD.y += TIME.y;
		dotABCD.z += TIME.z;
		dotABCD.w += TIME.w;

		COS.x = (float)Math.Cos(dotABCD.x);
		COS.y = (float)Math.Cos(dotABCD.y);
		COS.z = (float)Math.Cos(dotABCD.z);
		COS.w = (float)Math.Cos(dotABCD.w);

		Vector4 SIN;
	//	half4 SIN = sin (dotABCD + TIME);
		SIN.x = (float)Math.Sin(dotABCD.x);
		SIN.y = (float)Math.Sin(dotABCD.y);
		SIN.z = (float)Math.Sin(dotABCD.z);
		SIN.w = (float)Math.Sin(dotABCD.w);

		Vector3 offsets;

	//	offsets.x = dot(COS, half4(AB.xz, CD.xz));
		offsets.x = (COS.x * AB.x + COS.y * AB.z + COS.z * CD.x + COS.w * CD.z) * intensity.x;
	//	offsets.z = dot(COS, half4(AB.yw, CD.yw));
    	offsets.z = (COS.x * AB.y + COS.y * AB.w + COS.z * CD.y + COS.w * CD.w) * intensity.z;
    //	offsets.y = dot(SIN, amp);
    	offsets.y = (SIN.x * amp.x + SIN.y * amp.y + SIN.z * amp.z + SIN.w * amp.w) * intensity.y;

    	return (offsets);
	}

	static public Vector3 GetGestnerDisplacement (
		Vector3 WorldPosition,
		GersterWavesDescription Description,
		float TimeOffset
	) {

		Vector3 offsets;

		Vector2 xzVtx;
		xzVtx.x = WorldPosition.x;
		xzVtx.y = WorldPosition.z;

		offsets = InternalGetGestnerDisplacement (
			xzVtx, Description.intensity, Description.steepness, Description.amp, Description.freq, Description.speed, Description.dirAB, Description.dirCD, TimeOffset
		);

		if (Description.secondaryWaveParams.x > 0) {
			xzVtx.x += offsets.x;
			xzVtx.y += offsets.z;
			offsets += InternalGetGestnerDisplacement (
				xzVtx, Description.intensity, Description.steepness * Description.secondaryWaveParams.z, Description.amp * Description.secondaryWaveParams.x, Description.freq * Description.secondaryWaveParams.y, Description.speed * Description.secondaryWaveParams.w,
			//	Directions are swizzled
				new Vector4 (Description.dirAB.z, Description.dirAB.w, Description.dirAB.x, Description.dirAB.y),
				new Vector4 (Description.dirCD.z, Description.dirCD.w, Description.dirCD.x, Description.dirCD.y),
				TimeOffset
			);
		}

		return (offsets);

	}


	//	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public Vector3 GetGestnerDisplacementSingle (
		Vector3 WorldPosition,
		GersterWavesDescription Description,
		float TimeOffset
	) {

		Vector2 xzVtx;
		xzVtx.x = WorldPosition.x;
		xzVtx.y = WorldPosition.z;

		Vector4 AB;
		Vector4 CD;
		
	//	half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
		AB.x = Description.steepness.x * Description.amp.x * Description.dirAB.x;
		AB.y = Description.steepness.x * Description.amp.x * Description.dirAB.y;
		AB.z = Description.steepness.y * Description.amp.y * Description.dirAB.z;
		AB.w = Description.steepness.y * Description.amp.y * Description.dirAB.w;

	//	half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
		CD.x = Description.steepness.z * Description.amp.z * Description.dirCD.x;
		CD.y = Description.steepness.z * Description.amp.z * Description.dirCD.y;
		CD.z = Description.steepness.w * Description.amp.w * Description.dirCD.z;
		CD.w = Description.steepness.w * Description.amp.w * Description.dirCD.w;


	//	half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
		Vector4 dotABCD;
		dotABCD.x = Description.freq.x * (Description.dirAB.x * xzVtx.x + Description.dirAB.y * xzVtx.y);
		dotABCD.y = Description.freq.y * (Description.dirAB.z * xzVtx.x + Description.dirAB.w * xzVtx.y);
		dotABCD.z = Description.freq.z * (Description.dirCD.x * xzVtx.x + Description.dirCD.y * xzVtx.y);
		dotABCD.w = Description.freq.w * (Description.dirCD.z * xzVtx.x + Description.dirCD.w * xzVtx.y);
		
		Vector4 TIME;
	//	In case we do not use underwater rendering
		float time = Time.timeSinceLevelLoad + TimeOffset; //  Shader.GetGlobalVector("_Time").y + TimeOffset ; //Time.time; //timeSinceLevelLoad; //.time
	//	half4 TIME = _Time.yyyy * speed;
		TIME.x = time * Description.speed.x;
		TIME.y = time * Description.speed.y;
		TIME.z = time * Description.speed.z;
		TIME.w = time * Description.speed.w;

		Vector4 COS;
	//	half4 COS = cos (dotABCD + TIME);
		dotABCD.x += TIME.x;
		dotABCD.y += TIME.y;
		dotABCD.z += TIME.z;
		dotABCD.w += TIME.w;

		COS.x = (float)Math.Cos(dotABCD.x);
		COS.y = (float)Math.Cos(dotABCD.y);
		COS.z = (float)Math.Cos(dotABCD.z);
		COS.w = (float)Math.Cos(dotABCD.w);

		Vector4 SIN;
	//	half4 SIN = sin (dotABCD + TIME);
		SIN.x = (float)Math.Sin(dotABCD.x);
		SIN.y = (float)Math.Sin(dotABCD.y);
		SIN.z = (float)Math.Sin(dotABCD.z);
		SIN.w = (float)Math.Sin(dotABCD.w);

		Vector3 offsets;

	//	offsets.x = dot(COS, half4(AB.xz, CD.xz));
		offsets.x = (COS.x * AB.x + COS.y * AB.z + COS.z * CD.x + COS.w * CD.z) * Description.intensity.x;
	//	offsets.z = dot(COS, half4(AB.yw, CD.yw));
    	offsets.z = (COS.x * AB.y + COS.y * AB.w + COS.z * CD.y + COS.w * CD.w) * Description.intensity.z;
    //	offsets.y = dot(SIN, amp);
    	offsets.y = (SIN.x * Description.amp.x + SIN.y * Description.amp.y + SIN.z * Description.amp.z + SIN.w * Description.amp.w) * Description.intensity.y;

    	return (offsets);
	}
}



