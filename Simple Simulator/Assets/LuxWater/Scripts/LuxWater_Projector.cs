using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater {

	public class LuxWater_Projector : MonoBehaviour {

		public enum ProjectorType {
            FoamProjector,
            NormalProjector
        };

        [Space(8)]
        public ProjectorType Type = ProjectorType.FoamProjector;

		[System.NonSerialized] public static List<LuxWater_Projector > FoamProjectors = new List<LuxWater_Projector>();
		[System.NonSerialized] public static List<LuxWater_Projector > NormalProjectors = new List<LuxWater_Projector>();

	//	These varaiables must be public as they are accesses by the LuxWater_WaterProjectors - but we do not have to see them nor do we have to serialize them.	
		[System.NonSerialized] public Renderer m_Rend;
		[System.NonSerialized] public Material m_Mat;

		private bool added = false;

		private Vector3 origPos;

		void Update () {
			var pos = this.transform.position;
			pos.y = origPos.y;
//			this.transform.position = pos;
		}

		// Use this for initialization
		void OnEnable () {
origPos = this.transform.position;
			var rend = this.GetComponent<Renderer>();
			if (rend != null) {
				m_Rend = this.GetComponent<Renderer>();
				m_Mat = m_Rend.sharedMaterials[0];
				m_Rend.enabled = false;
				
				if (Type == ProjectorType.FoamProjector)
					FoamProjectors.Add(this);
				else
					NormalProjectors.Add(this);
				added = true;
			}
		}
		
		// Update is called once per frame
		void OnDisable () {
			if (added) {
				if (Type == ProjectorType.FoamProjector)
					FoamProjectors.Remove(this);
				else
					NormalProjectors.Remove(this);
				m_Rend.enabled = true;
			}
		}
	}
}