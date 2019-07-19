using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater {

	public class LuxWater_InfiniteOcean : MonoBehaviour {

		[Space(6)]
        [LuxWater_HelpBtn("h.c1utuz9up55r")]
		public Camera MainCam;
		public float GridSize = 10.0f;

		private Transform trans;
		private Transform camTrans;

		void OnEnable () {
			trans = this.GetComponent<Transform>();
		}
		
		void LateUpdate () {
			if (MainCam == null) {
				var mCam = Camera.main;
				if (mCam == null) {
					return;
				}
				else {
					MainCam = mCam;
				}
			}
			if (camTrans == null) {
				camTrans = MainCam.transform;
			}

			var CamPos = camTrans.position;
			var oceanPos = trans.position;

			var scale = trans.lossyScale;
			var gridSteps = new Vector2(GridSize * scale.x, GridSize * scale.z);

			var factor = (float)Math.Round(CamPos.x / gridSteps.x);
			var stepLeft = gridSteps.x * factor;
			factor = (float)Math.Round(CamPos.z / gridSteps.y );
			var stepUp = gridSteps.y * factor;

			oceanPos.x = stepLeft + (oceanPos.x % gridSteps.x);
			oceanPos.z = stepUp + (oceanPos.z % gridSteps.y); 

			trans.position = oceanPos;
			
		}
	}

}