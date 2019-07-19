// This code contains NVIDIA Confidential Information and is disclosed to you
// under a form of NVIDIA software license agreement provided separately to you.
//
// Notice
// NVIDIA Corporation and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from NVIDIA Corporation is strictly prohibited.
//
// ALL NVIDIA DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". NVIDIA MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, NVIDIA Corporation assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of NVIDIA Corporation. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// NVIDIA Corporation products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// NVIDIA Corporation.
//
// Copyright (c) 2018 NVIDIA Corporation. All rights reserved.

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NVIDIA.Flex
{
    [CustomEditor(typeof(FlexAsset))]
    public class FlexAssetEditor : Editor
    {
        protected PreviewRenderUtility m_previewRender = null;
        protected Vector3 m_cameraTarget = Vector3.zero;
        protected float m_paintParticleRadius = 0.0f;

        protected virtual void OnEnable()
        {
            m_previewRender = new PreviewRenderUtility();
            m_previewRender.cameraFieldOfView = 30.0f;
        }

        protected virtual void OnDisable()
        {
            m_previewRender.Cleanup();
            m_previewRender = null;
        }

        public override bool HasPreviewGUI()
        {
            return targets.Length == 1;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if ((Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) &&
                (Event.current.keyCode == KeyCode.LeftControl || Event.current.keyCode == KeyCode.RightControl ||
                 Event.current.keyCode == KeyCode.LeftAlt || Event.current.keyCode == KeyCode.RightAlt))
            {
                Event.current.Use();
                GUI.changed = true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                if (Event.current.control && Event.current.alt) EditorGUIUtility.AddCursorRect(r, MouseCursor.ArrowMinus);
                else if (Event.current.control) EditorGUIUtility.AddCursorRect(r, MouseCursor.ArrowPlus);
                else EditorGUIUtility.AddCursorRect(r, MouseCursor.Orbit);

                m_previewRender.BeginPreview(r, background);
                CommandBuffer commandBuffer = PreviewCommands();
                m_previewRender.camera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
                m_previewRender.camera.cullingMask = 0;
                m_previewRender.camera.Render();
                m_previewRender.camera.RemoveCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
                m_previewRender.EndAndDrawPreview(r);
            }
            else if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.control && Event.current.alt && r.Contains(Event.current.mousePosition))
            {
                PaintFixedParticles(r, false);
            }
            else if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.control && r.Contains(Event.current.mousePosition))
            {
                PaintFixedParticles(r, true);
            }
            else if (Event.current.type == EventType.MouseDrag && r.Contains(Event.current.mousePosition))
            {
                Vector2 mouseDelta = Event.current.delta;
                Transform cameraTransform = m_previewRender.camera.transform;
                cameraTransform.RotateAround(m_cameraTarget, cameraTransform.up, mouseDelta.x);
                cameraTransform.RotateAround(m_cameraTarget, cameraTransform.right, mouseDelta.y);
                Event.current.Use();
                GUI.changed = true;
            }
        }

        protected virtual CommandBuffer PreviewCommands()
        {
            return new CommandBuffer();
        }

        protected virtual void ReleaseBuffers()
        {
        }

        void PaintFixedParticles(Rect r, bool _fix)
        {
            FlexAsset asset = target as FlexAsset;
            if (asset)
            {
                Vector2 rectPoint = Event.current.mousePosition - r.min;
                Vector2 viewPoint = new Vector2(2.0f * rectPoint.x / r.width - 1.0f, 1.0f - 2.0f * rectPoint.y / r.height);
                Camera cam = m_previewRender.camera;
                float tanFOV = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
                Vector3 camPoint = new Vector3(viewPoint.x * tanFOV * r.width / r.height, viewPoint.y * tanFOV, 1.0f) * cam.nearClipPlane;
                Ray mouseRay = new Ray(cam.transform.position, (cam.transform.TransformPoint(camPoint) - cam.transform.position).normalized);
                //Bounds bounds = asset.referenceMesh.bounds;
                //bounds.size = new Vector3(bounds.size.x * asset.meshLocalScale.x, bounds.size.y * asset.meshLocalScale.y, bounds.size.z * asset.meshLocalScale.z);
                //float radius = 0.1f;// bounds.size.magnitude * 0.5f * 0.03f;
                int node = FlexUtils.PickParticle(mouseRay, asset.particles, m_paintParticleRadius);
                if (node != -1)
                {
                    if (asset.FixedParticle(node, _fix))
                    {
                        asset.Rebuild();
                        ReleaseBuffers();
                    }
                    Event.current.Use();
                    GUI.changed = true;
                    EditorUtility.SetDirty(target);
                    HandleUtility.Repaint();
                }
            }
        }
    }
}
