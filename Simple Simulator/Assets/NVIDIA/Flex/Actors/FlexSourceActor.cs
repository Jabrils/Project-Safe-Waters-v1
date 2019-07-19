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
using System.Linq;
using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("NVIDIA/Flex/Flex Source Actor")]
    public class FlexSourceActor : FlexActor
    {
        #region Properties

        public new FlexSourceAsset asset
        {
            get { return m_asset; }
            set { m_asset = value; }
        }

        public float lifeTime
        {
            get { return m_lifeTime; }
            set { m_lifeTime = value; }
        }

        public float[] ages
        {
            get { return m_ages; }
        }

        public float startSpeed
        {
            get { return m_startSpeed; }
            set { m_startSpeed = value; }
        }

        public bool isActive
        {
            get { return m_isActive; }
            set { m_isActive = value; }
        }

        #endregion

        #region Messages

        void OnDrawGizmos()
        {
            if (m_asset && m_asset.surfaceMesh)
            {
                Gizmos.color = new Color(0, 0, 0, 0);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawMesh(m_asset.surfaceMesh, Vector3.zero, Quaternion.identity, m_asset.meshLocalScale);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (m_asset && m_asset.surfaceMesh)
            {
                Gizmos.color = GIZMO_COLOR;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireMesh(m_asset.surfaceMesh, Vector3.zero, Quaternion.identity, m_asset.meshLocalScale);
            }
        }

        #endregion

        #region Protected

        protected override FlexAsset subclassAsset { get { return m_asset; } }

        protected override void CreateInstance()
        {
            base.CreateInstance();

            if (handle)
            {
                m_count = 0;
                m_indices = new int[m_asset.maxParticles];
                m_ages = new float[m_asset.maxParticles];
            }
        }

        protected override void DestroyInstance()
        {
            base.DestroyInstance();
        }

        protected override void ValidateFields()
        {
            base.ValidateFields();
        }

        protected override void OnFlexUpdate(FlexContainer.ParticleData _particleData)
        {
            base.OnFlexUpdate(_particleData);

            UpdateParticles(_particleData);
        }

        #endregion

        #region Private

        void UpdateParticles(FlexContainer.ParticleData _particleData)
        {
            if (handle && m_indices != null && m_indices.Length > 0)
            {
                float dT = Application.isPlaying ? Time.fixedDeltaTime : 0;

                int newCount = FlexUtils.UpdateSourceParticles(ref m_indices[0], ref m_ages[0], m_count, _particleData.particleData.particles, dT, massScale);

                if (newCount != m_count)
                {
                    m_currentContainer.FreeParticles(m_indices, newCount, m_count - newCount);
                    m_count = newCount;
                }

                if (m_isActive && m_startSpeed > 0)
                {
                    m_spawnTime += dT;
                    while (m_spawnTime > 0)
                    {
                        int spawnCount = Mathf.Min(m_asset.nozzleCount, m_asset.maxParticles - m_count);
                        if (spawnCount > 0)
                        {
                            newCount += m_currentContainer.AllocParticles(m_indices, m_count, spawnCount);
                            for (int i = m_count; i < newCount; ++i)
                            {
                                int index = m_indices[i];
                                int nozzle = i - m_count;
                                _particleData.SetPhase(index, m_phase);
                                Vector3 direction = transform.TransformDirection(m_asset.nozzleDirections[nozzle]);
                                Vector3 velocity = direction * m_startSpeed;
                                _particleData.SetVelocity(index, velocity);
                                Vector3 position = transform.TransformPoint(m_asset.nozzlePositions[nozzle]);
                                position -= velocity * (2.0f * dT - m_spawnTime);
                                Vector4 particle = position; particle.w = 0.000001f;//1.0f / massScale;//
                                _particleData.SetParticle(index, particle);
                                _particleData.SetRestParticle(index, Vector3.zero);
                                m_ages[i] = m_lifeTime;
                            }
                            m_count = newCount;
                        }
                        float dist = /*fluid ? m_currentContainer.fluidRest :*/ m_currentContainer.solidRest;
                        m_spawnTime -= dist / m_startSpeed;
                    }
                }

                SetIndices(m_indices, m_count);
            }
        }

        [NonSerialized]
        float m_spawnTime = 0.0f;

        [NonSerialized]
        int m_count;
        [NonSerialized]
        int[] m_indices;
        [NonSerialized]
        float[] m_ages;

        [SerializeField]
        FlexSourceAsset m_asset;
        //[SerializeField]
        //float m_spawnRate = 100.0f;//
        [SerializeField]
        float m_lifeTime = 5.0f;
        [SerializeField]
        float m_startSpeed = 1.0f;
        [SerializeField]
        bool m_isActive = true;

        #endregion
    }
}
