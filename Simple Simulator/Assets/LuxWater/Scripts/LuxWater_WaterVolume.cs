using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater {

	public class LuxWater_WaterVolume : MonoBehaviour {

	//	Events
		public delegate void TriggerEnter();
		public static event TriggerEnter OnEnterWaterVolume;
		public delegate void TriggerExit();
		public static event TriggerExit OnExitWaterVolume;

		[Space(6)]
		[LuxWater_HelpBtn("h.86taxuhovssb")]
		public Mesh WaterVolumeMesh;
		
		[Space(8)]
		public bool SlidingVolume = false;
		public float GridSize = 10.0f;

		
		private LuxWater_UnderWaterRendering waterrendermanager;
		private bool readyToGo = false;

		private int ID;

		void OnEnable () {
			if (WaterVolumeMesh == null) {
				Debug.Log("No WaterVolumeMesh assigned.");
				return;
			}

			ID = this.GetInstanceID();

		//	Register with LuxWater_UnderWaterRendering singleton – using invoke just in order to get around script execution order problems
			Invoke("Register", 0.0f);

		//	Make sure wtaer does not cast shadows as otherwise OnBecameVisible is triggered way too early.
			var rend = GetComponent<Renderer>();
			rend.shadowCastingMode = ShadowCastingMode.Off;
		//	Config water material so it uses fixed watersurface position and _Lux_Time	
			var waterMat = rend.sharedMaterial;
			waterMat.EnableKeyword("USINGWATERVOLUME");
			waterMat.SetFloat("_WaterSurfaceYPos", this.transform.position.y);
		}

		void OnDisable () {
			if (waterrendermanager) {
				waterrendermanager.DeRegisterWaterVolume(this, ID);
			}
			readyToGo = false;
			GetComponent<Renderer>().sharedMaterial.DisableKeyword("USINGWATERVOLUME");
		}

		void Register() {
		//	Try to get waterrendermanager
			var t_waterrendermanager = LuxWater_UnderWaterRendering.instance;
			if (t_waterrendermanager != null) {
				waterrendermanager = LuxWater_UnderWaterRendering.instance;
			// 	Check if the renderer is visible. Needed in case the script is enabled when the renderer already is on screen as OnBecameVisible will fail...
				var visible = GetComponent<Renderer>().isVisible;
				waterrendermanager.RegisterWaterVolume(this, ID, visible, SlidingVolume);
				readyToGo = true;
			}
		//	Try to get waterrendermanager in the next frame
			else {
				Invoke("Register", 0.0f);
			}
		}

		void OnBecameVisible() {
			if(readyToGo) {
				waterrendermanager.SetWaterVisible(ID);
			}
		}
		void OnBecameInvisible() {
			if(readyToGo) {
				waterrendermanager.SetWaterInvisible(ID);
			}
		}


	// 	Handle collision between water volume and camera
		private void OnTriggerEnter(Collider other) {
			var trigger = other.GetComponent<LuxWater_WaterVolumeTrigger>();
			if (trigger != null && waterrendermanager != null && readyToGo) {
				if (trigger.active == true) {
					//waterrendermanager.EnteredWaterVolume(this, ID, other.GetComponent<Camera>() );
					waterrendermanager.EnteredWaterVolume(this, ID, trigger.cam, GridSize );
					
					if(OnEnterWaterVolume != null) {
						OnEnterWaterVolume();
					}
				}
			}
		}

		private void OnTriggerStay(Collider other) {
			var trigger = other.GetComponent<LuxWater_WaterVolumeTrigger>();
			if (trigger != null && waterrendermanager != null && readyToGo) {
				if (trigger.active == true) {
					//waterrendermanager.EnteredWaterVolume(this, ID, other.GetComponent<Camera>() );
					waterrendermanager.EnteredWaterVolume(this, ID, trigger.cam, GridSize );
				}
			}
		}

		private void OnTriggerExit(Collider other) {
			var trigger = other.GetComponent<LuxWater_WaterVolumeTrigger>();
			if (trigger != null && waterrendermanager != null && readyToGo) {
				if (trigger.active == true) {
					//waterrendermanager.LeftWaterVolume(this, ID, other.GetComponent<Camera>() );
					waterrendermanager.LeftWaterVolume(this, ID, trigger.cam );

					if(OnExitWaterVolume != null) {
						OnExitWaterVolume();
					}
				}
			}
		}
	}
}
