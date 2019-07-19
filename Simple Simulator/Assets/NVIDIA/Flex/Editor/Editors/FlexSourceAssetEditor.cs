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

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NVIDIA.Flex
{
    [CustomEditor(typeof(FlexSourceAsset))]
    public class FlexSourceAssetEditor : FlexAssetEditor
    {
        SerializedProperty m_surfaceMesh;
        SerializedProperty m_meshLocalScale;
        SerializedProperty m_maxParticles;
        SerializedProperty m_meshTesselation;
        SerializedProperty m_nozzleCount;

        SerializedProperty m_rebuildAsset;

        Material m_meshMaterial = null, m_nozzleMaterial = null;
        ComputeBuffer m_particleBuffer = null, m_indexBuffer = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_surfaceMesh = serializedObject.FindProperty("m_surfaceMesh");
            m_meshLocalScale = serializedObject.FindProperty("m_meshLocalScale");
            m_maxParticles = serializedObject.FindProperty("m_maxParticles");
            m_meshTesselation = serializedObject.FindProperty("m_meshTesselation");
            m_nozzleCount = serializedObject.FindProperty("m_nozzlePositions.Array.size");

            m_rebuildAsset = serializedObject.FindProperty("m_rebuildAsset");

            m_meshMaterial = new Material(Shader.Find("Flex/SourcePreview"));
            m_nozzleMaterial = new Material(Shader.Find("Flex/DebugPoints"));
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
            if (m_meshMaterial)
            {
                DestroyImmediate(m_meshMaterial);
                m_meshMaterial = null;
            }
            if (m_nozzleMaterial)
            {
                DestroyImmediate(m_nozzleMaterial);
                m_nozzleMaterial = null;
            }

            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_surfaceMesh);
            EditorGUILayout.PropertyField(m_meshLocalScale);
            EditorGUILayout.PropertyField(m_meshTesselation);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_nozzleCount, new GUIContent("Nozzle Count"));
            GUI.enabled = true;
            EditorGUILayout.PropertyField(m_maxParticles);


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
            FlexSourceAsset asset = target as FlexSourceAsset;
            if (asset && asset.surfaceMesh && m_meshMaterial && m_nozzleMaterial)
            {
                Bounds bounds = asset.surfaceMesh.bounds;
                bounds.size = new Vector3(bounds.size.x * asset.meshLocalScale.x, bounds.size.y * asset.meshLocalScale.y, bounds.size.z * asset.meshLocalScale.z);
                float radius = bounds.size.magnitude * 0.5f;
                float angle = m_previewRender.camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
                float distance = radius / Mathf.Sin(angle);
                m_cameraTarget = bounds.center;
                Transform cameraTransform = m_previewRender.camera.transform;
                cameraTransform.position = m_cameraTarget - cameraTransform.forward * distance;
                m_previewRender.camera.nearClipPlane = distance - radius;
                m_previewRender.camera.farClipPlane = distance + radius;
                Matrix4x4 scale = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, asset.meshLocalScale);
                commandBuffer.DrawMesh(asset.surfaceMesh, scale, m_meshMaterial);
                if (m_particleBuffer == null || m_indexBuffer == null)
                {
                    Vector3[] nozzles = asset.nozzlePositions;
                    if (nozzles.Length > 0)
                    {
                        m_particleBuffer = new ComputeBuffer(nozzles.Length, 16);
                        m_particleBuffer.SetData(Array.ConvertAll<Vector3, Vector4>(nozzles, v => v));
                        m_indexBuffer = new ComputeBuffer(nozzles.Length, 4);
                        m_indexBuffer.SetData(Enumerable.Range(0, nozzles.Length).ToArray());
                    }
                }
                if (m_particleBuffer != null && m_indexBuffer != null)
                {
                    m_nozzleMaterial.SetPass(0);
                    m_nozzleMaterial.SetBuffer("_Points", m_particleBuffer);
                    m_nozzleMaterial.SetBuffer("_Indices", m_indexBuffer);
                    m_nozzleMaterial.SetFloat("_Radius", radius * 0.03f);
                    m_nozzleMaterial.SetColor("_Color", _auxFlexDrawParticles.NOZZLE_COLOR);
                    commandBuffer.DrawProcedural(Matrix4x4.identity, m_nozzleMaterial, 0, MeshTopology.Points, m_indexBuffer.count);
                }
            }
            return commandBuffer;
        }
    }
}
