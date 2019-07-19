using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LuxWater {

    [ExecuteInEditMode]
    public class LuxWater_PlanarReflection : MonoBehaviour {

        [Space(6)]
        [LuxWater_HelpBtn("h.5c3jy4qfh163")]
        public bool UpdateSceneView = true;

        [Space(5)]
        public bool isMaster = false;
        public Material[] WaterMaterials; 

        [Space(5)]
        public LayerMask reflectionMask = -1;
        public enum ReflectionResolution {
            Full = 1,
            Half = 2,
            Quarter = 4,
            Eighth = 8
        };
        public enum NumberOfShadowCascades {
            One = 1,
            Two = 2,
            Four = 4
        };

        public ReflectionResolution Resolution = ReflectionResolution.Half;

        public Color clearColor = Color.black;
        public bool reflectSkybox = true;

        [Space(5)]
        public bool disablePixelLights = false;

        [Space(5)]
        public bool renderShadows = true;
        public float shadowDistance = 0f;
        public NumberOfShadowCascades ShadowCascades = NumberOfShadowCascades.One;

        [Space(5)]
        public float WaterSurfaceOffset = 0.0f;
        public float clipPlaneOffset = 0.07F;

        private String reflectionSampler = "_LuxWater_ReflectionTex";
        private Vector3 m_Oldpos;
        private Camera m_ReflectionCamera;
        private Material m_SharedMaterial;
        private Dictionary<Camera, bool> m_HelperCameras;

        private RenderTexture m_reflectionMap;

        void OnEnable(){
        //  Make sure water is on layer "Water"
            gameObject.layer = LayerMask.NameToLayer ("Water");
            Renderer rend = GetComponent<Renderer>();
            if (rend != null) {
                m_SharedMaterial = GetComponent<Renderer>().sharedMaterial;
                //isTile = true;
            }
            else {
                //isTile = false;
            }
        }

        void OnDisable() {
            if(m_ReflectionCamera != null) {
                DestroyImmediate (m_ReflectionCamera.targetTexture);
                DestroyImmediate (m_ReflectionCamera);
            }
            if (null != m_HelperCameras) {
                m_HelperCameras.Clear();
            }
        }

        Camera CreateReflectionCameraFor(Camera cam) {
            String reflName = gameObject.name + "Reflection" + cam.name;
            GameObject go = GameObject.Find(reflName);
            if (!go) {
                go = new GameObject(reflName, typeof(Camera));
                go.hideFlags = HideFlags.HideAndDontSave;
            }
            if (!go.GetComponent(typeof(Camera))) {
                go.AddComponent(typeof(Camera));
            }
            Camera reflectCamera = go.GetComponent<Camera>();
            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            SetStandardCameraParameter(reflectCamera, reflectionMask);
            if (!reflectCamera.targetTexture) {
                reflectCamera.targetTexture = CreateTextureFor(cam);
            }
            return reflectCamera;
        }

        void SetStandardCameraParameter(Camera cam, LayerMask mask) {
            cam.cullingMask = mask & ~(1 << LayerMask.NameToLayer("Water"));
            cam.backgroundColor = Color.black;
            cam.enabled = false;
        }


        RenderTexture CreateTextureFor(Camera cam) {
            int rtWidth = Mathf.FloorToInt(cam.pixelWidth / (int)Resolution);
            int rtHeight = Mathf.FloorToInt(cam.pixelHeight / (int)Resolution);
            RenderTexture rt = new RenderTexture(rtWidth,rtHeight, 24);
            rt.hideFlags = HideFlags.DontSave;
            return rt;
        }


        public void RenderHelpCameras(Camera currentCam) {
            if (null == m_HelperCameras) {
                m_HelperCameras = new Dictionary<Camera, bool>();
            }
            if (!m_HelperCameras.ContainsKey(currentCam)) {
                m_HelperCameras.Add(currentCam, false);
            }
        
        //  "if (m_HelperCameras[currentCam])" prevents updates in scene view, so we wrap it.
            #if UNITY_EDITOR
                if( !(UnityEditor.SceneView.currentDrawingSceneView && UnityEditor.SceneView.currentDrawingSceneView.camera == Camera.current) ) {
                    if (m_HelperCameras[currentCam]) {
                        return;
                    }
                }
            #else
                if (m_HelperCameras[currentCam]) {
                   return;
                }
            #endif

        //  Omit real time reflection probes, crazy.
        //  Produces garbage... so we put it at the end to get at least less garbage.
            if (currentCam.name.Contains("Reflection Probes")) { 
                return;
            }

            if (!m_ReflectionCamera) {
                m_ReflectionCamera = CreateReflectionCameraFor(currentCam);
            }
            RenderReflectionFor(currentCam, m_ReflectionCamera);
            m_HelperCameras[currentCam] = true;
        }


        public void LateUpdate() {
            if (null != m_HelperCameras) {
                m_HelperCameras.Clear();
            }
        }


        public void WaterTileBeingRendered(Transform tr, Camera currentCam) {
            RenderHelpCameras(currentCam);
            if (m_ReflectionCamera && m_SharedMaterial) {
                m_SharedMaterial.SetTexture(reflectionSampler, m_ReflectionCamera.targetTexture);
            }
        }


        public void OnWillRenderObject() {
            WaterTileBeingRendered(transform, Camera.current);
        }


        void RenderReflectionFor(Camera cam, Camera reflectCamera) {
            if (!reflectCamera) {
                return;
            }
            if (m_SharedMaterial && !m_SharedMaterial.HasProperty(reflectionSampler)) {
                return;
            }

            #if UNITY_EDITOR
                int rtWidth = Mathf.FloorToInt(cam.pixelWidth / (int)Resolution);
                int rtHeight = Mathf.FloorToInt(cam.pixelHeight / (int)Resolution);
                if (reflectCamera.targetTexture.width != rtWidth || reflectCamera.targetTexture.height != rtHeight) {
                    DestroyImmediate (reflectCamera.targetTexture);
                    reflectCamera.targetTexture = CreateTextureFor (cam);
                }
            #endif

            reflectCamera.cullingMask = reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));

            SaneCameraSettings(reflectCamera);

            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            
            GL.invertCulling = true;

            Transform reflectiveSurface = transform;

            Vector3 eulerA = cam.transform.eulerAngles;

            reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
            reflectCamera.transform.position = cam.transform.position;

        reflectCamera.orthographic = cam.orthographic;
        reflectCamera.orthographicSize = cam.orthographicSize;   

            Vector3 pos = reflectiveSurface.transform.position;
        
        //  Allow custom offset
            pos.y = reflectiveSurface.position.y + WaterSurfaceOffset;
            
            Vector3 normal = reflectiveSurface.transform.up;
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
            m_Oldpos = cam.transform.position;
            Vector3 newpos = reflection.MultiplyPoint(m_Oldpos);

            reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            Vector4 clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);

            Matrix4x4 projection = cam.projectionMatrix;
            projection = CalculateObliqueMatrix(projection, clipPlane);
            reflectCamera.projectionMatrix = projection;

            reflectCamera.transform.position = newpos;
            Vector3 euler = cam.transform.eulerAngles;
            reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

        //  Set pixel light count
            int oldPixelLightCount = QualitySettings.pixelLightCount;
            if (disablePixelLights) {
                QualitySettings.pixelLightCount = 0;
            }

        //  Set shadows
            var oldShadowDist = QualitySettings.shadowDistance;
            var oldShadowCascades = QualitySettings.shadowCascades;
            if(!renderShadows) {
                QualitySettings.shadowDistance = 0f;
            }
            else if(shadowDistance > 0f) {
                QualitySettings.shadowDistance = shadowDistance;
            }
            QualitySettings.shadowCascades = (int)ShadowCascades;

            reflectCamera.Render();

            GL.invertCulling = false;

        //  Restore pixel light count
            if (disablePixelLights) {
                QualitySettings.pixelLightCount = oldPixelLightCount;
            }
        //  Restore shadows
            if(!renderShadows || shadowDistance > 0f) {
                QualitySettings.shadowDistance = oldShadowDist;
            }
            QualitySettings.shadowCascades = oldShadowCascades;

        //  
            if (isMaster) {
                for(int i = 0; i < WaterMaterials.Length; i++) {
                    WaterMaterials[i].SetTexture(reflectionSampler, reflectCamera.targetTexture); 
                }
            }
        }


        void SaneCameraSettings(Camera helperCam) {
            helperCam.depthTextureMode = DepthTextureMode.None;
            helperCam.backgroundColor = Color.black;
            helperCam.clearFlags = CameraClearFlags.SolidColor;
            helperCam.renderingPath = RenderingPath.Forward;
        }


        static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane) {
            Vector4 q = projection.inverse * new Vector4(
                Sgn(clipPlane.x),
                Sgn(clipPlane.y),
                1.0F,
                1.0F
            );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            // third row = clip plane - fourth row
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];

            return projection;
        }


        static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane) {
            reflectionMat.m00 = (1.0F - 2.0F * plane[0] * plane[0]);
            reflectionMat.m01 = (- 2.0F * plane[0] * plane[1]);
            reflectionMat.m02 = (- 2.0F * plane[0] * plane[2]);
            reflectionMat.m03 = (- 2.0F * plane[3] * plane[0]);

            reflectionMat.m10 = (- 2.0F * plane[1] * plane[0]);
            reflectionMat.m11 = (1.0F - 2.0F * plane[1] * plane[1]);
            reflectionMat.m12 = (- 2.0F * plane[1] * plane[2]);
            reflectionMat.m13 = (- 2.0F * plane[3] * plane[1]);

            reflectionMat.m20 = (- 2.0F * plane[2] * plane[0]);
            reflectionMat.m21 = (- 2.0F * plane[2] * plane[1]);
            reflectionMat.m22 = (1.0F - 2.0F * plane[2] * plane[2]);
            reflectionMat.m23 = (- 2.0F * plane[3] * plane[2]);

            reflectionMat.m30 = 0.0F;
            reflectionMat.m31 = 0.0F;
            reflectionMat.m32 = 0.0F;
            reflectionMat.m33 = 1.0F;

            return reflectionMat;
        }


        static float Sgn(float a) {
            if (a > 0.0F) {
                return 1.0F;
            }
            if (a < 0.0F) {
                return -1.0F;
            }
            return 0.0F;
        }


        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign) {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }
}