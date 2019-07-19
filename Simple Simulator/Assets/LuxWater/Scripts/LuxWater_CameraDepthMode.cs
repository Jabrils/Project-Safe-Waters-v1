using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LuxWater {

	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class LuxWater_CameraDepthMode : MonoBehaviour {

		public bool GrabDepthTexture = false;
		private Camera cam;

		private Material CopyDepthMat;
		private RenderTextureFormat format;

		private Dictionary<Camera, CommandBuffer> m_cmdBuffer = new Dictionary<Camera, CommandBuffer>();
		private bool CamCallBackAdded = false;

		[HideInInspector] public bool ShowShaderWarning = true;

		void OnEnable(){
		//	Get main Camera and make sure it renders into a depth texture
			cam = GetComponent<Camera>();
			cam.depthTextureMode |= DepthTextureMode.Depth;

			if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal) {
			//	Add CallBack to ALL Cameras including the Scene View
				Camera.onPreCull += OnPrecull;
				CamCallBackAdded = true;

			//	Set up the custom Depth Copy material
				CopyDepthMat = new Material(Shader.Find("Hidden/Lux Water/CopyDepth"));
				format = RenderTextureFormat.RFloat;
	            if (!SystemInfo.SupportsRenderTextureFormat(format))
	                format = RenderTextureFormat.RHalf;
	            if (!SystemInfo.SupportsRenderTextureFormat(format))
	                format = RenderTextureFormat.ARGBHalf;
	        }
		}

		
		void OnDisable() {
			if(CamCallBackAdded) {
				Camera.onPreCull -= OnPrecull;
				foreach (var cam in m_cmdBuffer) {
					if (cam.Key != null) {
						cam.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, cam.Value);
					}
				}
				m_cmdBuffer.Clear();
			}
			ShowShaderWarning = true;
		}


		void OnPrecull(Camera camera) {
		//	In case we use Metal and deferred shading reading and writing to the depth buffer makes water vanishing. So we do a copy of the depth buffer.
			if(GrabDepthTexture) {
				RenderingPath path = cam.actualRenderingPath;
			//	Only needed if we use metal and deferred
				if (path == RenderingPath.DeferredShading && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal) {

					CommandBuffer cBuffer;
					if (!m_cmdBuffer.TryGetValue(camera, out cBuffer)) {
						cBuffer = new CommandBuffer();
						cBuffer.name = "Lux Water Grab Depth";
					// 	This adds it to the lighting of the second camera?
					//	camera.AddCommandBuffer(CameraEvent.AfterLighting, cBuffer);
					//	This seems to work as the 2nd camera does not render a skybox
						camera.AddCommandBuffer(CameraEvent.BeforeSkybox, cBuffer);
						m_cmdBuffer[camera] = cBuffer;
					}

					cBuffer.Clear();
					var width = camera.pixelWidth;
					var height = camera.pixelHeight;
					int depthGrabID = Shader.PropertyToID("_Lux_GrabbedDepth");
					cBuffer.GetTemporaryRT(depthGrabID, width, height, 0, FilterMode.Point, format, RenderTextureReadWrite.Linear);
					cBuffer.Blit(BuiltinRenderTextureType.CurrentActive, depthGrabID, CopyDepthMat, 0);
					cBuffer.ReleaseTemporaryRT(depthGrabID);
					//Shader.EnableKeyword("LUXWATERMETALDEFERRED");
				}
				else {
					GrabDepthTexture = false;
					foreach (var cam in m_cmdBuffer) {
						if (cam.Key != null) {
							cam.Key.RemoveCommandBuffer(CameraEvent.AfterLighting, cam.Value);
						}
					}
					m_cmdBuffer.Clear();
					ShowShaderWarning = true;
					//Shader.DisableKeyword("LUXWATERMETALDEFERRED");
				}
			}
		}
	}
}