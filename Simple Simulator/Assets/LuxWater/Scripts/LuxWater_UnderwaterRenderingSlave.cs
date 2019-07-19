using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater {

	[RequireComponent(typeof(Camera))]
	public class LuxWater_UnderwaterRenderingSlave : MonoBehaviour {


		private LuxWater_UnderWaterRendering waterrendermanager;
		private bool readyToGo = false;

		public Camera cam;

		void OnEnable () {
			cam = GetComponent<Camera>();
		//	Get with LuxWater_UnderWaterRendering singleton – using invoke just in order to get around script execution order problems
			Invoke("GetWaterrendermanager", 0.0f);
		}

		void GetWaterrendermanager() {
			var manager = LuxWater_UnderWaterRendering.instance;
			if (manager != null) {
				waterrendermanager = manager;
				readyToGo = true;
			}
		}

		void OnPreCull () {
			if (readyToGo) {
				waterrendermanager.RenderWaterMask( cam, true );
			}
		}

		[ImageEffectOpaque]
		void OnRenderImage(RenderTexture src, RenderTexture dest) {
			if (readyToGo) {
				waterrendermanager.RenderUnderWater(src, dest, cam, true);
			}
		//	We have to blit in any case - otherwise the screen will be black.
			else {
				Graphics.Blit(src, dest);
			}
		}

	}

}
