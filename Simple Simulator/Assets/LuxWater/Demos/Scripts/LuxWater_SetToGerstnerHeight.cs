using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuxWater_SetToGerstnerHeight : MonoBehaviour {

	public Material WaterMaterial;
	public Vector3 Damping = new Vector3(0.3f, 1.0f, 0.3f);
	public float TimeOffset = 0.0f;
	public bool UpdateWaterMaterialPerFrame = false;

	[Space(8)]
	public bool AddCircleAnim = false;
	public float Radius = 6.0f;
	public float Speed = 1.0f;

	[Space(8)]
	public Transform[] ManagedWaterProjectors;

	[Header("Debug")]
	public float MaxDisp;
	
	private Transform trans;

	private LuxWaterUtils.GersterWavesDescription Description;
	private bool ObjectIsVisible = false;
	private Vector3 Offset = Vector3.zero;

	void Start () {
		trans = this.transform;
	//	Get the Gestner Wave settings from the material and store them into or Description struct
		LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
	}

	void OnBecameVisible () {
		ObjectIsVisible = true;
	}

	void OnBecameInvisible () {
		ObjectIsVisible = false;
	}
	
	void LateUpdate () {
		
	//	In case the object is rendered by any camera we have to update its position.
		if (ObjectIsVisible || AddCircleAnim) {

		//	Check for material – you could add a check here if Gerstner Waves are enabled
			if (WaterMaterial == null) {
				return;
			}

			if (UpdateWaterMaterialPerFrame) {
			//	Update the Gestner Wave settings from the material if needed
				LuxWaterUtils.GetGersterWavesDescription(ref Description, WaterMaterial);
			}

			var pos = trans.position;
		//	Reset pos by subtracting the last Offset
			pos -= Offset;

		//	Animate the position
			if(AddCircleAnim) {
				pos.x += Mathf.Sin(Time.time * Speed) * Time.deltaTime * Radius;
				pos.z += Mathf.Cos(Time.time * Speed) * Time.deltaTime * Radius;
			}
	
		//	Sync assigned Managed Water Projectors (transform)
			var count = ManagedWaterProjectors.Length;
			if(count > 0) {
				for(int i = 0; i != count; i++) {
					var cpos = ManagedWaterProjectors[i].position;
					cpos.x = pos.x;
					cpos.z = pos.z;
					ManagedWaterProjectors[i].position = cpos;
				}
			}

		//	Get the offset of the Gerstner displacement. We have to pass:
		//	- a sample location in world space,
		//	- the Gestner Wave settings from the material sttored in our Description struct,
		//	- a time offset (in seconds) which lets us create an effect of the inertia of masses.
			Offset = LuxWaterUtils.GetGestnerDisplacement(pos, Description, TimeOffset);

		#if UNITY_EDITOR
			var maxd = 	Offset.magnitude;
			if(maxd > MaxDisp)	{
				MaxDisp = maxd;
			}
		#endif	
			
		//	Calculate the new Offset
			Offset.x += Offset.x * Damping.x;
			Offset.y += Offset.y * Damping.y;
			Offset.z += Offset.z * Damping.z;

			trans.position = pos + Offset;
		}
	}
}
