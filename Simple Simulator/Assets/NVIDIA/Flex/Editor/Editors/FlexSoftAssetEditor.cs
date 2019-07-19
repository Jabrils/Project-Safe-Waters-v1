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
    [CustomEditor(typeof(FlexSoftAsset))]
    public class FlexSoftAssetEditor : FlexAssetEditor
    {
        SerializedProperty m_boundaryMesh;
        SerializedProperty m_meshLocalScale;
        SerializedProperty m_particleSpacing;
        SerializedProperty m_volumeSampling;
        SerializedProperty m_surfaceSampling;
        SerializedProperty m_clusterSpacing;
        SerializedProperty m_clusterRadius;
        SerializedProperty m_clusterStiffness;
        SerializedProperty m_linkRadius;
        SerializedProperty m_linkStiffness;

        SerializedProperty m_particleCount;
        SerializedProperty m_clusterCount;
        SerializedProperty m_linkCount;
        SerializedProperty m_fixedCount;

        SerializedProperty m_rebuildAsset;

        ComputeBuffer m_particleBuffer = null;
        ComputeBuffer[] m_indexBuffers = null;
        Material[] m_previewMaterials = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_boundaryMesh = serializedObject.FindProperty("m_boundaryMesh");
            m_meshLocalScale = serializedObject.FindProperty("m_meshLocalScale");
            m_particleSpacing = serializedObject.FindProperty("m_particleSpacing");
            m_volumeSampling = serializedObject.FindProperty("m_volumeSampling");
            m_surfaceSampling = serializedObject.FindProperty("m_surfaceSampling");
            m_clusterSpacing = serializedObject.FindProperty("m_clusterSpacing");
            m_clusterRadius = serializedObject.FindProperty("m_clusterRadius");
            m_clusterStiffness = serializedObject.FindProperty("m_clusterStiffness");
            m_linkRadius = serializedObject.FindProperty("m_linkRadius");
            m_linkStiffness = serializedObject.FindProperty("m_linkStiffness");

            m_particleCount = serializedObject.FindProperty("m_particles.Array.size");
            m_clusterCount = serializedObject.FindProperty("m_shapeOffsets.Array.size");
            m_linkCount = serializedObject.FindProperty("m_springCoefficients.Array.size");
            m_fixedCount = serializedObject.FindProperty("m_fixedParticles.Array.size");

            m_rebuildAsset = serializedObject.FindProperty("m_rebuildAsset");
        }

        protected override void OnDisable()
        {
            if (m_particleBuffer != null)
            {
                m_particleBuffer.Release();
                m_particleBuffer = null;
            }
            if (m_indexBuffers != null)
            {
                foreach (var b in m_indexBuffers) b.Release();
                m_indexBuffers = null;
            }
            if (m_previewMaterials != null)
            {
                foreach (var m in m_previewMaterials) DestroyImmediate(m);
                m_previewMaterials = null;
            }

            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_boundaryMesh);
            EditorGUILayout.PropertyField(m_meshLocalScale);
            EditorGUILayout.PropertyField(m_particleSpacing);
            EditorGUILayout.PropertyField(m_volumeSampling);
            EditorGUILayout.PropertyField(m_surfaceSampling);
            EditorGUILayout.PropertyField(m_clusterSpacing);
            EditorGUILayout.PropertyField(m_clusterRadius);
            EditorGUILayout.PropertyField(m_clusterStiffness);
            EditorGUILayout.PropertyField(m_linkRadius);
            EditorGUILayout.PropertyField(m_linkStiffness);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_particleCount, new GUIContent("Particle Count"));
            EditorGUILayout.PropertyField(m_clusterCount, new GUIContent("Cluster Count"));
            EditorGUILayout.PropertyField(m_linkCount, new GUIContent("Link Count"));
            EditorGUILayout.PropertyField(m_fixedCount, new GUIContent("Fixed Particle Count"));

            if (GUI.changed)
            {
                m_rebuildAsset.boolValue = true;

                serializedObject.ApplyModifiedProperties();
                ReleaseBuffers();
            }
        }

        protected override void ReleaseBuffers()
        {
            if (m_particleBuffer != null)
            {
                m_particleBuffer.Release();
                m_particleBuffer = null;
            }
            if (m_indexBuffers != null)
            {
                foreach (var b in m_indexBuffers) b.Release();
                m_indexBuffers = null;
            }
            if (m_previewMaterials != null)
            {
                foreach (var m in m_previewMaterials) DestroyImmediate(m);
                m_previewMaterials = null;
            }
        }

        protected override CommandBuffer PreviewCommands()
        {
            CommandBuffer commandBuffer = new CommandBuffer();
            FlexSoftAsset asset = target as FlexSoftAsset;
            if (asset)
            {
                if (m_particleBuffer == null || m_indexBuffers == null)
                {
                    Vector4[] particles = asset.particles;
                    if (particles.Length > 0)
                    {
                        m_particleBuffer = new ComputeBuffer(particles.Length, 16);
                        m_particleBuffer.SetData(particles);
                        int shapeCount = asset.shapeOffsets.Length;
                        if (shapeCount > 0)
                        {
                            m_indexBuffers = new ComputeBuffer[shapeCount];
                            m_previewMaterials = new Material[shapeCount];
                            for (int i = 0; i < shapeCount; ++i)
                            {
                                int start = i == 0 ? 0 : asset.shapeOffsets[i - 1];
                                int length = asset.shapeOffsets[i] - start;
                                m_indexBuffers[i] = new ComputeBuffer(length, 4);
                                m_indexBuffers[i].SetData(asset.shapeIndices.Skip(start).Take(length).ToArray());
                                m_previewMaterials[i] = new Material(Shader.Find("Flex/DebugPoints"));
                                m_previewMaterials[i].SetPass(0);
                                m_previewMaterials[i].SetBuffer("_Points", m_particleBuffer);
                                m_previewMaterials[i].SetBuffer("_Indices", m_indexBuffers[i]);
                                m_previewMaterials[i].SetFloat("_Radius", asset.particleSpacing * 0.5f);
                                m_previewMaterials[i].SetColor("_Color", _auxFlexDrawParticles.SHAPE_COLORS[i % _auxFlexDrawParticles.SHAPE_COLORS.Length]);
                            }
                        }

                        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * float.MinValue);
                        foreach (var p in particles) bounds.Encapsulate(p);
                        bounds.Expand(asset.particleSpacing * 2.0f);
                        float radius = bounds.size.magnitude * 0.5f;
                        float angle = m_previewRender.camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
                        float distance = radius / Mathf.Sin(angle);
                        m_paintParticleRadius = radius * 0.03f;
                        m_cameraTarget = bounds.center;
                        Transform cameraTransform = m_previewRender.camera.transform;
                        cameraTransform.position = m_cameraTarget - cameraTransform.forward * distance;
                        m_previewRender.camera.nearClipPlane = distance - radius;
                        m_previewRender.camera.farClipPlane = distance + radius;
                    }
                }
                if (m_particleBuffer != null && m_indexBuffers != null)
                {
                    int shapeCount = asset.shapeOffsets.Length;
                    for (int i = 0; i < shapeCount; ++i)
                    {
                        commandBuffer.DrawProcedural(Matrix4x4.identity, m_previewMaterials[i], 0, MeshTopology.Points, m_indexBuffers[i].count);
                    }
                }
            }
            return commandBuffer;
        }
    }
}
