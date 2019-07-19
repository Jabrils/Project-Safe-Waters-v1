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

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NVIDIA.Flex
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FlexClothAsset))]
    public class FlexClothAssetEditor : FlexAssetEditor
    {
        SerializedProperty m_referenceMesh;
        SerializedProperty m_meshLocalScale;
        SerializedProperty m_meshTesselation;
        SerializedProperty m_weldingThreshold;
        SerializedProperty m_stretchStiffness;
        SerializedProperty m_bendStiffness;
        SerializedProperty m_tetherStiffness;
        SerializedProperty m_tetherGive;
        SerializedProperty m_pressure;
        SerializedProperty m_nodeCount;
        SerializedProperty m_fixedCount;
        SerializedProperty m_linkCount;

        SerializedProperty m_rebuildAsset;

        Material m_meshMaterial = null, m_nodeMaterial = null;
        ComputeBuffer m_particleBuffer = null, m_indexBuffer = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_referenceMesh = serializedObject.FindProperty("m_referenceMesh");
            m_meshLocalScale = serializedObject.FindProperty("m_meshLocalScale");
            m_meshTesselation = serializedObject.FindProperty("m_meshTesselation");
            m_weldingThreshold = serializedObject.FindProperty("m_weldingThreshold");
            m_stretchStiffness = serializedObject.FindProperty("m_stretchStiffness");
            m_bendStiffness = serializedObject.FindProperty("m_bendStiffness");
            m_tetherStiffness = serializedObject.FindProperty("m_tetherStiffness");
            m_tetherGive = serializedObject.FindProperty("m_tetherGive");
            m_pressure = serializedObject.FindProperty("m_pressure");
            m_nodeCount = serializedObject.FindProperty("m_particles.Array.size");
            m_fixedCount = serializedObject.FindProperty("m_fixedParticles.Array.size");
            m_linkCount = serializedObject.FindProperty("m_springCoefficients.Array.size");

            m_rebuildAsset = serializedObject.FindProperty("m_rebuildAsset");

            m_meshMaterial = new Material(Shader.Find("Flex/SourcePreview"));
            m_nodeMaterial = new Material(Shader.Find("Flex/DebugPoints"));
        }

        protected override void OnDisable()
        {
            ReleaseBuffers();
            if (m_meshMaterial)
            {
                DestroyImmediate(m_meshMaterial);
                m_meshMaterial = null;
            }
            if (m_nodeMaterial)
            {
                DestroyImmediate(m_nodeMaterial);
                m_nodeMaterial = null;
            }

            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_referenceMesh);
            EditorGUILayout.PropertyField(m_meshLocalScale);
            EditorGUILayout.PropertyField(m_meshTesselation);
            EditorGUILayout.PropertyField(m_weldingThreshold);
            EditorGUILayout.PropertyField(m_stretchStiffness);
            EditorGUILayout.PropertyField(m_bendStiffness);
            EditorGUILayout.PropertyField(m_tetherStiffness);
            EditorGUILayout.PropertyField(m_tetherGive);
            EditorGUILayout.PropertyField(m_pressure);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_nodeCount, new GUIContent("Node Count"));
            EditorGUILayout.PropertyField(m_fixedCount, new GUIContent("Fixed Node Count"));
            EditorGUILayout.PropertyField(m_linkCount, new GUIContent("Link Count"));

            if (GUI.changed)
            {
                m_rebuildAsset.boolValue = true;

                serializedObject.ApplyModifiedProperties();
                ReleaseBuffers();
            }
        }

        protected override CommandBuffer PreviewCommands()
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            FlexClothAsset asset = target as FlexClothAsset;
            if (asset && asset.referenceMesh && m_meshMaterial && m_nodeMaterial)
            {
                Bounds bounds = asset.referenceMesh.bounds;
                bounds.size = new Vector3(bounds.size.x * asset.meshLocalScale.x, bounds.size.y * asset.meshLocalScale.y, bounds.size.z * asset.meshLocalScale.z);
                float radius = bounds.size.magnitude * 0.5f;
                float angle = m_previewRender.camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
                float distance = radius / Mathf.Sin(angle);
                m_cameraTarget = bounds.center;
                Transform cameraTransform = m_previewRender.camera.transform;
                cameraTransform.position = m_cameraTarget - cameraTransform.forward * distance;
                m_previewRender.camera.nearClipPlane = distance - radius;
                m_previewRender.camera.farClipPlane = distance + radius;
                m_paintParticleRadius = radius * 0.03f;
                Matrix4x4 scale = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, asset.meshLocalScale);
                commandBuffer.DrawMesh(asset.referenceMesh, scale, m_meshMaterial);
                if (m_particleBuffer == null || m_indexBuffer == null)
                {
                    Vector4[] particles = asset.particles;
                    if (particles.Length > 0)
                    {
                        m_particleBuffer = new ComputeBuffer(particles.Length, 16);
                        m_particleBuffer.SetData(particles);
                        m_indexBuffer = new ComputeBuffer(particles.Length, 4);
                        m_indexBuffer.SetData(Enumerable.Range(0, particles.Length).ToArray());
                    }
                }
                if (m_particleBuffer != null && m_indexBuffer != null)
                {
                    m_nodeMaterial.SetPass(0);
                    m_nodeMaterial.SetBuffer("_Points", m_particleBuffer);
                    m_nodeMaterial.SetBuffer("_Indices", m_indexBuffer);
                    m_nodeMaterial.SetFloat("_Radius", radius * 0.03f);
                    m_nodeMaterial.SetColor("_Color", _auxFlexDrawParticles.CLOTH_COLOR);
                    commandBuffer.DrawProcedural(Matrix4x4.identity, m_nodeMaterial, 0, MeshTopology.Points, m_indexBuffer.count);
                }
            }
            return commandBuffer;
        }

        //bool m_skipMouseUp = false;

        //public override void OnPreviewGUI(Rect r, GUIStyle background)
        //{
        //    //if (Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition))
        //    //{
        //    //    if (m_skipMouseUp)
        //    //    {
        //    //        m_skipMouseUp = false;
        //    //    }
        //    //    else
        //    //    {
        //    //        FlexClothAsset asset = target as FlexClothAsset;
        //    //        if (asset)
        //    //        {
        //    //            Vector2 rectPoint = Event.current.mousePosition - r.min;
        //    //            Vector2 viewPoint = new Vector2(2.0f * rectPoint.x / r.width - 1.0f, 1.0f - 2.0f * rectPoint.y / r.height);
        //    //            Camera cam = m_previewRender.m_Camera;
        //    //            float tanFOV = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
        //    //            Vector3 camPoint = new Vector3(viewPoint.x * tanFOV * r.width / r.height, viewPoint.y * tanFOV, 1.0f) * cam.nearClipPlane;
        //    //            Ray mouseRay = new Ray(cam.transform.position, (cam.transform.TransformPoint(camPoint) - cam.transform.position).normalized);
        //    //            Bounds bounds = asset.referenceMesh.bounds;
        //    //            bounds.size = new Vector3(bounds.size.x * asset.meshLocalScale.x, bounds.size.y * asset.meshLocalScale.y, bounds.size.z * asset.meshLocalScale.z);
        //    //            float radius = bounds.size.magnitude * 0.5f * 0.03f;
        //    //            int node = FlexUtils.PickParticle(mouseRay, asset.particles, radius);
        //    //            if (node != -1)
        //    //            {
        //    //                asset.FixedParticle(node, true);
        //    //                ReleaseBuffers();
        //    //                Event.current.Use();
        //    //                GUI.changed = true;
        //    //                EditorUtility.SetDirty(target);
        //    //                HandleUtility.Repaint();
        //    //                return;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //if (Event.current.type == EventType.MouseDrag && r.Contains(Event.current.mousePosition))
        //    //{
        //    //    m_skipMouseUp = true;
        //    //}

        //    base.OnPreviewGUI(r, background);
        //}

        protected override void ReleaseBuffers()
        {
            if (m_particleBuffer != null)
            {
                m_particleBuffer.Release();
                m_particleBuffer = null;
            }
            if (m_indexBuffer != null)
            {
                m_indexBuffer.Release();
                m_indexBuffer = null;
            }
        }
    }
}
