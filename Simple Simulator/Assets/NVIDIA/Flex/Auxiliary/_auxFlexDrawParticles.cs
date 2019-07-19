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
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("")]
    public class _auxFlexDrawParticles : MonoBehaviour
    {
        #region Constants

        public static Color PARTICLE_COLOR = new Color(1.0f, 1.0f, 1.0f);
        public static Color CLOTH_COLOR = new Color(248, 137, 86) / 255.0f;
        public static Color NOZZLE_COLOR = new Color(152, 248, 248) / 255.0f;
        public static Color[] SHAPE_COLORS = new Color[] { new Color(255, 192, 203) / 255.0f,
                                                           new Color(194, 249, 220) / 255.0f,
                                                           new Color(191, 239, 251) / 255.0f,
                                                           new Color(252, 252, 202) / 255.0f,
                                                           new Color(214, 204, 255) / 255.0f,
                                                           new Color(225, 225, 225) / 255.0f };

        #endregion

        #region Methods

        public void UpdateMesh()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            if (m_actor && m_actor.container && m_particleMaterials != null)
            {
                if (m_actor is FlexSourceActor)
                {
                    int[] indices = m_actor.indices;
                    int indexCount = m_actor.indexCount;
                    if (m_indexBuffers[0] != null && indexCount != m_indexBuffers[0].count)
                    {
                        m_indexBuffers[0].Release();
                        m_indexBuffers[0] = null;
                    }
                    if (m_indexBuffers[0] == null && indexCount > 0)
                    {
                        m_indexBuffers[0] = new ComputeBuffer(indexCount, sizeof(int));
                        //m_mesh.subMeshCount = 0;
                        m_mesh.SetIndices(new int[indexCount], MeshTopology.Points, 0);
                    }
                    if (m_indexBuffers[0] != null)
                    {
                        m_indexBuffers[0].SetData(indices, 0, 0, indexCount);
                    }
                }

                m_mesh.bounds = m_actor.bounds;

                for (int i = 0; i < m_particleMaterials.Length; ++i)
                {
                    if (m_particleMaterials[i])
                    {
                        m_particleMaterials[i].SetBuffer("_Points", m_actor.container.particleBuffer);
                        m_particleMaterials[i].SetBuffer("_Indices", m_indexBuffers[i]);
                        m_particleMaterials[i].SetFloat("_Radius", ParticleSize());
                        m_particleMaterials[i].SetColor("_Color", SubmeshColor(i));
                    }
                }
            }
        }

        #endregion

        #region Messages

        void OnEnable()
        {
            Create();
        }

        void OnDisable()
        {
            Destroy();
        }

        #endregion

        #region Private

        const string PARTICLES_SHADER = "Flex/FlexDrawParticles";

        void Create()
        {
            m_actor = GetComponentInParent<FlexActor>();
            if (m_actor == null)
            {
                Debug.LogError("_auxFlexDrawPartcles should be parented to a FlexActor");
                Debug.Break();
            }

            if (m_actor && m_actor.handle)
            {
                FlexExt.Instance instance = m_actor.handle.instance;

                int[] indices = new int[instance.numParticles];
                if (instance.numParticles > 0) FlexUtils.FastCopy(instance.particleIndices, indices);

                m_mesh = new Mesh();
                m_mesh.name = "Flex Particles";
                m_mesh.vertices = new Vector3[1];

                if (m_actor is FlexSoftActor)
                {
                    int shapeCount = m_actor.asset.shapeOffsets.Length;
                    m_indexBuffers = new ComputeBuffer[shapeCount];
                    m_particleMaterials = new Material[shapeCount];
                    m_mesh.subMeshCount = shapeCount;
                    for (int i = 0; i < shapeCount; ++i)
                    {
                        int start = i == 0 ? 0 : m_actor.asset.shapeOffsets[i - 1];
                        int count = m_actor.asset.shapeOffsets[i] - start;
                        m_mesh.SetIndices(new int[count], MeshTopology.Points, i);
                        m_indexBuffers[i] = new ComputeBuffer(count, 4);
                        int[] shape = new int[count];
                        for (int j = 0; j < count; ++j) shape[j] = indices[m_actor.asset.shapeIndices[start + j]];
                        m_indexBuffers[i].SetData(shape);
                        m_particleMaterials[i] = new Material(Shader.Find(PARTICLES_SHADER));
                        m_particleMaterials[i].hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                else
                {
                    m_mesh.subMeshCount = 1;
                    m_mesh.SetIndices(new int[0], MeshTopology.Points, 0);
                    m_mesh.SetIndices(new int[instance.numParticles], MeshTopology.Points, 0);
                    m_indexBuffers = new ComputeBuffer[1];
                    if (instance.numParticles > 0)
                    {
                        m_indexBuffers[0] = new ComputeBuffer(instance.numParticles, sizeof(int));
                        m_indexBuffers[0].SetData(indices);
                    }
                    m_particleMaterials = new Material[1];
                    m_particleMaterials[0] = new Material(Shader.Find(PARTICLES_SHADER));
                    m_particleMaterials[0].hideFlags = HideFlags.HideAndDontSave;
                }

                m_mesh.bounds = m_actor.bounds;

                MeshFilter meshFilter = GetComponent<MeshFilter>();
                meshFilter.mesh = m_mesh;

                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.materials = m_particleMaterials;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
#if UNITY_EDITOR
                EditorUtility.SetSelectedRenderState(meshRenderer, EditorSelectedRenderState.Hidden);
#endif
                UpdateMesh();
            }
        }

        void Destroy()
        {
            if (m_mesh) DestroyImmediate(m_mesh);
            if (m_particleMaterials != null)
                foreach (var m in m_particleMaterials) if (m != null) DestroyImmediate(m);
            if (m_indexBuffers != null)
                foreach (var b in m_indexBuffers) if(b != null) b.Release();
        }

        float ParticleSize()
        {
            if (!m_actor || !m_actor.container) return 0;
            return (m_actor.fluid ? m_actor.container.fluidRest : m_actor.container.solidRest) * 0.5f;
        }

        Color SubmeshColor(int submesh = 0)
        {
            if (!m_actor) return PARTICLE_COLOR;
            if (m_actor is FlexSolidActor) return SHAPE_COLORS[0];
            else if (m_actor is FlexSoftActor) return SHAPE_COLORS[submesh % SHAPE_COLORS.Length];
            else if (m_actor is FlexClothActor) return CLOTH_COLOR;
            return PARTICLE_COLOR;
        }

        FlexActor m_actor;
        Mesh m_mesh;

        [NonSerialized]
        Material[] m_particleMaterials;
        [NonSerialized]
        ComputeBuffer[] m_indexBuffers;

        #endregion
    }
}
