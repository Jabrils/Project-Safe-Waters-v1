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
using System.Collections.Generic;
using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(FlexClothActor))]
    [AddComponentMenu("NVIDIA/Flex/Flex Cloth Deformation")]
    public class FlexClothDeformation : MonoBehaviour
    {
        #region Messages

        void OnEnable()
        {
            Create();
        }

        void OnDisable()
        {
            Destroy();
        }

        void Reset()
        {
            m_deformationTarget = gameObject;
        }

        #endregion

        #region Private

        void Create()
        {
            m_actor = GetComponent<FlexClothActor>();
            m_actor.onFlexUpdate += OnFlexUpdate;
            m_actor.onBeforeRecreate += OnBeforeRecreate;
            m_actor.onAfterRecreate += OnAfterRecreate;

            if (m_deformationTarget)
            {
                MeshFilter meshFilter = m_deformationTarget.GetComponent<MeshFilter>();
                m_mesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;

                m_originalMeshVertices = m_mesh.vertices;

                if (!Application.isPlaying || m_particles.Length == 0)
                    FindMatchingPairs();

                m_meshVertices = m_mesh.vertices;
                m_meshNormals = m_mesh.normals;
            }
        }

        void Destroy()
        {
            m_actor.onFlexUpdate -= OnFlexUpdate;
            m_actor.onBeforeRecreate -= OnBeforeRecreate;
            m_actor.onAfterRecreate -= OnAfterRecreate;
        }

        void FindMatchingPairs()
        {
            // @@@ Optimize!!!
            List<int> particles = new List<int>();
            List<int> vertices = new List<int>();

            if (m_actor.asset && m_mesh)
            {
                Vector4[] assetParticles = m_actor.asset.particles;
                Vector3[] meshVertices = m_originalMeshVertices;// m_mesh.vertices;
                for (int i = 0; i < assetParticles.Length; ++i)
                {
                    Vector3 p = m_deformationTarget.transform.InverseTransformPoint(m_actor.transform.TransformPoint(assetParticles[i]));
                    for (int j = 0; j < meshVertices.Length; ++j)
                    {
                        Vector3 v = meshVertices[j];
                        if (Vector3.Distance(p, v) < m_proximityTreshold)
                        {
                            particles.Add(i); vertices.Add(j);
                        }
                    }
                }
            }

            m_particles = particles.ToArray();
            m_vertices = vertices.ToArray();
        }

        void UpdateClothDeformation(FlexContainer.ParticleData _particleData)
        {
            if (m_deformationTarget && m_actor.handle)
            {
                Matrix4x4 xform = transform.worldToLocalMatrix;
                FlexUtils.UpdateClothVertices(ref m_particles[0], ref m_vertices[0], m_particles.Length,
                                              ref m_actor.indices[0], _particleData.particleData.particles, _particleData.particleData.normals, ref xform,
                                              ref m_meshVertices[0], ref m_meshNormals[0]);

                m_mesh.vertices = m_meshVertices;
                m_mesh.normals = m_meshNormals;
                m_mesh.RecalculateBounds();
            }
        }

        void OnFlexUpdate(FlexContainer.ParticleData _particleData)
        {
            if (Application.isPlaying) UpdateClothDeformation(_particleData);
        }

        void OnBeforeRecreate()
        {
            m_particles = new int[0];
        }

        void OnAfterRecreate()
        {
            if (!Application.isPlaying || m_particles.Length == 0)
                FindMatchingPairs();
        }

        FlexClothActor m_actor;
        Mesh m_mesh;

        [NonSerialized]
        Vector3[] m_originalMeshVertices;
        [NonSerialized]
        Vector3[] m_meshVertices;
        [NonSerialized]
        Vector3[] m_meshNormals;

        [SerializeField]
        GameObject m_deformationTarget;
        [SerializeField]
        int[] m_particles = new int[0];
        [SerializeField]
        int[] m_vertices = new int[0];
        [SerializeField]
        float m_proximityTreshold = 1e-5f;

        #endregion
    }
}
