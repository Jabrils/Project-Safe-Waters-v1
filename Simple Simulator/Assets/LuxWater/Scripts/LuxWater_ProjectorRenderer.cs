using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater {

	[RequireComponent(typeof(Camera))]
	[ExecuteInEditMode]
	public class LuxWater_ProjectorRenderer : MonoBehaviour {


		public enum BufferResolution {
            Full = 1,
            Half = 2,
            Quarter = 4,
            Eighth = 8
        };

        [Space(8)]
        public BufferResolution FoamBufferResolution = BufferResolution.Full;
        public BufferResolution NormalBufferResolution = BufferResolution.Full;

        [Space(2)]
		[Header("Debug")]
		[Space(4)]
        public bool DebugFoamBuffer = false;
        public bool DebugNormalBuffer = false;
        public bool DebugStats = false;

        private int drawnFoamProjectors = 0;
        private int drawnNormalProjectors = 0;

		private static CommandBuffer cb_Foam;
		private static CommandBuffer cb_Normals;

		private Camera cam;
		private Transform camTransform;

		private RenderTexture ProjectedFoam;
		private RenderTexture ProjectedNormals;

		private Texture2D defaultBump;

		private Bounds tempBounds;

		private int _LuxWater_FoamOverlayPID;
		private int _LuxWater_NormalOverlayPID;
		//private int _CameraDepthTexturePID;

		private Plane[] frustumPlanes = new Plane[6];

		private Material DebugMat;
        private Material DebugNormalMat;

        // Use this for initialization
        void OnEnable () {

			_LuxWater_FoamOverlayPID = Shader.PropertyToID("_LuxWater_FoamOverlay");
			_LuxWater_NormalOverlayPID = Shader.PropertyToID("_LuxWater_NormalOverlay");
			//_CameraDepthTexturePID = Shader.PropertyToID("_CameraDepthTexture");

			cb_Foam = new CommandBuffer();
			cb_Foam.name = "Lux Water: Foam Overlay Buffer";

			cb_Normals = new CommandBuffer();
			cb_Normals.name = "Lux Water: Normal Overlay Buffer";

		}

		void OnDisable() {
			if(ProjectedFoam != null) {
				DestroyImmediate(ProjectedFoam);
			}
			if(ProjectedNormals != null) {
				DestroyImmediate(ProjectedNormals);
			}
			if(defaultBump != null) {
				DestroyImmediate(defaultBump);
			}
			if(DebugMat != null) {
				DestroyImmediate(DebugMat);
			}
			if(cb_Foam != null) {
			//	Checking for != null is not enough, so we check for its size as well
				if(cb_Foam.sizeInBytes > 0) {
					cb_Foam.Clear();
            		cb_Foam.Dispose();
            	}
            }
			if(cb_Normals != null) {
			//	Checking for != null is not enough, so we check for its size as well
				if(cb_Normals.sizeInBytes > 0) {
					cb_Normals.Clear();
					cb_Normals.Dispose();
				}
			}
            Shader.DisableKeyword("USINGWATERPROJECTORS");
        }
	
	//	We have to use OnPreCull or OnPreRender as otherwise particlesystems will make unity crash
	//	void OnPreRender() {
	//	This is what is active in 1.08
		void OnPreCull () { 

			#if UNITY_EDITOR
				if(!Application.isPlaying) {
					if(defaultBump == null) {
						defaultBump = new Texture2D(1, 1, TextureFormat.RGBA32, false);
						defaultBump.SetPixel(0, 0, new Color(0.5f, 0.5f, 1.0f, 0.5f).gamma);
						defaultBump.Apply(false);
					}
					Shader.SetGlobalTexture(_LuxWater_NormalOverlayPID, defaultBump);
					Shader.SetGlobalTexture(_LuxWater_FoamOverlayPID, Texture2D.blackTexture);
					return;
				}
			#endif
				
			cam = GetComponent<Camera>();

			#if UNITY_EDITOR
            	if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera == cam) {
            //		Shader.DisableKeyword("USINGWATERPROJECTORS");
            		return;
            	}
			#endif

		//	Check if we have to do anything
			var numFoamProjectors = LuxWater_Projector.FoamProjectors.Count;
			var numNormalProjectors = LuxWater_Projector.NormalProjectors.Count;

		//	No registered projectors
			if (numFoamProjectors + numNormalProjectors == 0) {
				if(cb_Foam != null) {
					cb_Foam.Clear();
                }
				if(cb_Normals != null) {
					cb_Normals.Clear();
				}
				Shader.DisableKeyword("USINGWATERPROJECTORS");
				return;
			}
			else {
				Shader.EnableKeyword("USINGWATERPROJECTORS");
			}

			var projectionMatrix = cam.projectionMatrix;
			var worldToCameraMatrix = cam.worldToCameraMatrix;
			var worldToProjectionMatrix = projectionMatrix * worldToCameraMatrix;

			var camPixelWidth = cam.pixelWidth;
			var camPixelHeight = cam.pixelHeight;

			//UnityEngine.Profiling.Profiler.BeginSample("Get Planes");
			GeomUtil.CalculateFrustumPlanes(frustumPlanes, worldToProjectionMatrix);
			//UnityEngine.Profiling.Profiler.EndSample();

		//	Foam Buffer
			var rtWidth = Mathf.FloorToInt(camPixelWidth / (int)FoamBufferResolution);
			var rtHeight = Mathf.FloorToInt(camPixelHeight / (int)FoamBufferResolution);
			
		//	Check if buffer's rt has to be created/updated
			if(!ProjectedFoam) {
				ProjectedFoam = new RenderTexture(rtWidth, rtHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
				//Shader.SetGlobalTexture(_LuxWater_FoamOverlayPID, ProjectedFoam);
			}
			else if (ProjectedFoam.width != rtWidth) {
				DestroyImmediate(ProjectedFoam);
				ProjectedFoam = new RenderTexture(rtWidth, rtHeight, 0, RenderTextureFormat.ARGB32,  RenderTextureReadWrite.Linear);
			//	We have to reassign the texture (prevented projectors from being updated after pause)
				//Shader.SetGlobalTexture(_LuxWater_FoamOverlayPID, ProjectedFoam);
			}

			GL.PushMatrix();
			GL.modelview = worldToCameraMatrix;
			GL.LoadProjectionMatrix(projectionMatrix);
			
			cb_Foam.Clear();
			cb_Foam.SetRenderTarget(ProjectedFoam);
			cb_Foam.ClearRenderTarget(true, true, new Color(0,0,0,0), 1.0f);
			//Shader.SetGlobalTexture(_CameraDepthTexturePID, Texture2D.whiteTexture);

			drawnFoamProjectors = 0;
			
			for(int i = 0; i < numFoamProjectors; i++) {
			//	Check renderer's bounds against frustum before calling DrawRenderer
				var currentProjector = LuxWater_Projector.FoamProjectors[i];
				tempBounds = currentProjector.m_Rend.bounds;
				if (GeometryUtility.TestPlanesAABB(frustumPlanes, tempBounds)) {
					cb_Foam.DrawRenderer(currentProjector.m_Rend, currentProjector.m_Mat);

					drawnFoamProjectors ++;
				}
			}
			Graphics.ExecuteCommandBuffer(cb_Foam);
		//	We might have multiple Cameras (split screen) - so we have to assign the Rendertexture each time.
			Shader.SetGlobalTexture(_LuxWater_FoamOverlayPID, ProjectedFoam);

		//	Normal Buffer
			rtWidth = Mathf.FloorToInt(camPixelWidth / (int)NormalBufferResolution);
			rtHeight = Mathf.FloorToInt(camPixelHeight / (int)NormalBufferResolution);

		//	Check if buffer's rt has to be created/updated
			if(!ProjectedNormals) {
                ProjectedNormals = new RenderTexture(rtWidth, rtHeight, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                //Shader.SetGlobalTexture(_LuxWater_NormalOverlayPID, ProjectedNormals);
			}
			else if (ProjectedNormals.width != rtWidth) {
				DestroyImmediate(ProjectedNormals);
                ProjectedNormals = new RenderTexture(rtWidth, rtHeight, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            //	We have to reassign the texture (prevented projectors from being updated after pause)
                //Shader.SetGlobalTexture(_LuxWater_NormalOverlayPID, ProjectedNormals);
            }

			cb_Normals.Clear();
			cb_Normals.SetRenderTarget(ProjectedNormals);
            // Regular ARGB buffer
            // cb_Normals.ClearRenderTarget(true, true, new Color(0.5f,0.5f,1.0f,0.5f), 1.0f);
            // ARGBHalf buffer
            // cb_Normals.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 1.0f, 0.0f), 1.0f); // blue was 1.0 corrupting height!
            cb_Normals.ClearRenderTarget(true, true, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1.0f);

            drawnNormalProjectors = 0;

			for(int i = 0; i < numNormalProjectors; i++) {
			//	Check renderer's bounds against frustum before calling DrawRenderer
				var currentProjector = LuxWater_Projector.NormalProjectors[i];
				tempBounds = currentProjector.m_Rend.bounds;
				if (GeometryUtility.TestPlanesAABB(frustumPlanes, tempBounds)) {
					cb_Normals.DrawRenderer(currentProjector.m_Rend, currentProjector.m_Mat);

					drawnNormalProjectors ++;
				}
			}
			Graphics.ExecuteCommandBuffer(cb_Normals);
		//	We might have multiple Cameras (split screen) - so we have to assign the Rendertexture each time.
			Shader.SetGlobalTexture(_LuxWater_NormalOverlayPID, ProjectedNormals);
			GL.PopMatrix();
		}
	

	//	Debug functions
		void OnDrawGizmos() {
			var tcam = GetComponent<Camera>();
			var offset = 0;
			var textureDrawWidth = (int)(tcam.aspect * 128.0f);

		//	Due to the alpha channels of the buffers we have to use a custom material/shader here
			if (DebugMat == null) {
				DebugMat = new Material(Shader.Find("Hidden/LuxWater_Debug"));
			}

            if (DebugNormalMat == null) {
                DebugNormalMat = new Material(Shader.Find("Hidden/LuxWater_DebugNormals"));
            }

			if (DebugFoamBuffer) {
				if(ProjectedFoam == null)
					return;
				GL.PushMatrix();
		        GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
		        Graphics.DrawTexture(new Rect(offset, 0, textureDrawWidth, 128), ProjectedFoam, DebugMat);
		        GL.PopMatrix();
		        offset = textureDrawWidth;
		    }

			if (DebugNormalBuffer) {
				if(ProjectedNormals == null)
					return;
		        GL.PushMatrix();
		        GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
		        Graphics.DrawTexture(new Rect(offset, 0, textureDrawWidth, 128), ProjectedNormals, DebugNormalMat);
		        GL.PopMatrix();
		    }
		}

		void OnGUI() {

		    if(DebugStats) {

		    	var NumberOfFoamProjectors = LuxWater_Projector.FoamProjectors.Count;
				var NumberOfNormalProjectors = LuxWater_Projector.NormalProjectors.Count;		

                var Alignement = GUI.skin.label.alignment;
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                
                GUI.Label(new Rect(10, 0, 300, 40),  "Foam Projectors   [Registered] " + NumberOfFoamProjectors + "  [Drawn] " + drawnFoamProjectors);
                GUI.Label(new Rect(10, 18, 300, 40), "Normal Projectors [Registered] " + NumberOfNormalProjectors + "  [Drawn] " + drawnNormalProjectors );

                GUI.skin.label.alignment = Alignement;
            }
		}
	}

//	Alloc free version to get the frustum planes
	public static class GeomUtil {
	    private static System.Action<Plane[], Matrix4x4> _calculateFrustumPlanes_Imp;
	    public static void CalculateFrustumPlanes(Plane[] planes, Matrix4x4 worldToProjectMatrix)
	    {
	        //if (planes == null) throw new System.ArgumentNullException("planes");
	        //if (planes.Length < 6) throw new System.ArgumentException("Output array must be at least 6 in length.", "planes");
	 
	        if (_calculateFrustumPlanes_Imp == null)
	        {
	            var meth = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic, null, new System.Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);
	            if (meth == null) throw new System.Exception("Failed to reflect internal method. Your Unity version may not contain the presumed named method in GeometryUtility.");
	 
	            _calculateFrustumPlanes_Imp = System.Delegate.CreateDelegate(typeof(System.Action<Plane[], Matrix4x4>), meth) as System.Action<Plane[], Matrix4x4>;
	            if(_calculateFrustumPlanes_Imp == null) throw new System.Exception("Failed to reflect internal method. Your Unity version may not contain the presumed named method in GeometryUtility.");
	        }
	        _calculateFrustumPlanes_Imp(planes, worldToProjectMatrix);
	    }
	}
}
