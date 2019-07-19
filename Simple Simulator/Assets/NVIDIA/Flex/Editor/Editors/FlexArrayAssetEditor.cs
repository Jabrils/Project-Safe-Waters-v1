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
    [CustomEditor(typeof(FlexArrayAsset))]
    public class FlexArrayAssetEditor : FlexAssetEditor
    {
        SerializedProperty m_boundaryMesh;
        SerializedProperty m_meshLocalScale;
        SerializedProperty m_meshExpansion;
        SerializedProperty m_particleSpacing;
        SerializedProperty m_particleCount;

        SerializedProperty m_rebuildAsset;

        ComputeBuffer m_particleBuffer = null, m_indexBuffer = null;
        Material m_previewMaterial = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_boundaryMesh = serializedObject.FindProperty("m_boundaryMesh");
            m_meshLocalScale = serializedObject.FindProperty("m_meshLocalScale");
            m_meshExpansion = serializedObject.FindProperty("m_meshExpansion");
            m_particleSpacing = serializedObject.FindProperty("m_particleSpacing");
            m_particleCount = serializedObject.FindProperty("m_particles.Array.size");

            m_rebuildAsset = serializedObject.FindProperty("m_rebuildAsset");

            m_previewMaterial = Resources.Load<Material>("Materials/DebugParticles");
        }

        protected override void OnDisable()
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
            if (m_previewMaterial)
            {
                m_previewMaterial = null;
            }

            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_boundaryMesh);
            EditorGUILayout.PropertyField(m_meshLocalScale);
            EditorGUILayout.PropertyField(m_meshExpansion);
            EditorGUILayout.PropertyField(m_particleSpacing);

            //EditorGUILayout.Separator();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_particleCount, new GUIContent("Particle Count"));

            if (GUI.changed)
            {
                m_rebuildAsset.boolValue = true;

                serializedObject.ApplyModifiedProperties();

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

        protected override CommandBuffer PreviewCommands()
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            FlexArrayAsset asset = target as FlexArrayAsset;
            if (asset)
            {
                if (m_particleBuffer == null || m_indexBuffer == null)
                {
                    Vector4[] particles = asset.particles;
                    if (particles.Length > 0)
                    {
                        m_particleBuffer = new ComputeBuffer(particles.Length, 16);
                        m_particleBuffer.SetData(particles);
                        m_indexBuffer = new ComputeBuffer(particles.Length, 4);
                        m_indexBuffer.SetData(Enumerable.Range(0, particles.Length).ToArray());

                        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * float.MinValue);
                        foreach (var p in particles) bounds.Encapsulate(p);
                        bounds.Expand(asset.particleSpacing * 2.0f);
                        float radius = bounds.size.magnitude * 0.5f;
                        float angle = m_previewRender.camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
                        float distance = radius / Mathf.Sin(angle);
                        m_cameraTarget = bounds.center;
                        Transform cameraTransform = m_previewRender.camera.transform;
                        cameraTransform.position = m_cameraTarget - cameraTransform.forward * distance;
                        m_previewRender.camera.nearClipPlane = distance - radius;
                        m_previewRender.camera.farClipPlane = distance + radius;
                    }
                }
                if (m_particleBuffer != null && m_indexBuffer != null)
                {
                    m_previewMaterial.SetPass(0);
                    m_previewMaterial.SetBuffer("_Points", m_particleBuffer);
                    m_previewMaterial.SetBuffer("_Indices", m_indexBuffer);
                    m_previewMaterial.SetFloat("_Radius", asset.particleSpacing * 0.5f);
                    m_previewMaterial.SetColor("_Color", _auxFlexDrawParticles.PARTICLE_COLOR);
                    commandBuffer.DrawProcedural(Matrix4x4.identity, m_previewMaterial, 0, MeshTopology.Points, m_indexBuffer.count);
                }
            }
            return commandBuffer;
        }
    }
}
