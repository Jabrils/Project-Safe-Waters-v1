using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater {
    public class LuxWater_SetMeshBounds : MonoBehaviour {

        [Space(6)]
        [LuxWater_HelpBtn("h.s0d0kaaphhix")]
    	public float Expand_XZ = 0.0f;
        public float Expand_Y = 0.0f;
        private Renderer rend;

        void OnEnable() {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds;
            bounds.Expand( new Vector3(Expand_XZ, Expand_Y, Expand_XZ) );
            mesh.bounds = bounds;
        }
        
        void OnDisable() {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateBounds();
        }

        void OnValidate() {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds;
            bounds.Expand( new Vector3(Expand_XZ, Expand_Y, Expand_XZ) );
            mesh.bounds = bounds;
        }

        void OnDrawGizmosSelected() {
        	rend = GetComponent<Renderer>();
            Vector3 center = rend.bounds.center;
            Vector3 Extents = rend.bounds.extents;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, rend.bounds.size);
        }
    }
}