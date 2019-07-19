using System;
using UnityEngine;

namespace LuxWater
{
    [ExecuteInEditMode]
    public class LuxWater_PlanarWaterTile : MonoBehaviour {
        
        [Space(6)]
        [LuxWater_HelpBtn("h.nu6w5ylbucb7")]
        public LuxWater_PlanarReflection reflection;

        public void OnEnable() {
            AcquireComponents();
        }

        void AcquireComponents() {
            if (!reflection)
            {
                if (transform.parent)
                {
                    reflection = transform.parent.GetComponent<LuxWater_PlanarReflection >();
                }
                else
                {
                    reflection = transform.GetComponent<LuxWater_PlanarReflection >();
                }
            }
        }

        #if UNITY_EDITOR
            public void Update() {
                AcquireComponents();
            }
        #endif

        public void OnWillRenderObject() {
            if (reflection) {
                reflection.WaterTileBeingRendered(transform, Camera.current);
            }
        }
    }
}