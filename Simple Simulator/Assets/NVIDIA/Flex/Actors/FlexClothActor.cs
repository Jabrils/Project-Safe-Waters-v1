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

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("NVIDIA/Flex/Flex Cloth Actor")]
    public class FlexClothActor : FlexActor
    {
        #region Properties

        public new FlexClothAsset asset
        {
            get { return m_asset; }
            set { m_asset = value; }
        }

        #endregion

        #region Messages

        void Reset()
        {
            selfCollide = false;
        }

        void OnDrawGizmos()
        {
        }

        void OnDrawGizmosSelected()
        {
        }

        #endregion

        #region Protected

        protected override FlexAsset subclassAsset { get { return m_asset; } }

        protected override void CreateInstance()
        {
            base.CreateInstance();

            if (handle)
            {
                FlexExt.Instance instance = handle.instance;
                if (instance.numParticles > 0)
                {
                    if (m_asset.fixedParticles.Length == 0)
                    {
                        int[] refPoints = new int[3] { 0, 1, 2 }; // @@@
                        Vector4[] particles = m_asset.particles;
                        //FlexUtils.ClothRefPoints(ref particles[0], particles.Length, ref refPoints[0]); // @@@ !!! Too slow
                        m_referencePoints = new int[] { indices[refPoints[0]], indices[refPoints[1]], indices[refPoints[2]] };
                        Vector3 p0 = particles[refPoints[0]], p1 = particles[refPoints[1]], p2 = particles[refPoints[2]];
                        m_localPosition = (p0 + p1 + p2) / 3;
                        Vector3 clothZ = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                        Vector3 clothY = Vector3.Cross(clothZ, p1 - p0).normalized;
                        m_localRotation = Quaternion.LookRotation(clothZ, clothY);
                    }
                }
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
            if (Application.isPlaying && m_referencePoints != null && m_referencePoints.Length >= 3)
            {
                Vector3 p0 = _particleData.GetParticle(m_referencePoints[0]);
                Vector3 p1 = _particleData.GetParticle(m_referencePoints[1]);
                Vector3 p2 = _particleData.GetParticle(m_referencePoints[2]);
                Vector3 clothZ = Vector3.Cross(p1 - p0, p2 - p0).normalized, clothY = Vector3.Cross(clothZ, p1 - p0).normalized;
                Quaternion globalRotation = Quaternion.LookRotation(clothZ, clothY) * Quaternion.Inverse(m_localRotation);
                Vector3 globalPosition = (p0 + p1 + p2) / 3 - globalRotation * m_localPosition;
                transform.position = globalPosition;
                transform.rotation = globalRotation;
                transform.hasChanged = false;
            }

            base.OnFlexUpdate(_particleData);
        }

        #endregion

        #region Private

        [NonSerialized]
        int[] m_referencePoints = null;
        [NonSerialized]
        Vector3 m_localPosition = Vector3.zero;
        [NonSerialized]
        Quaternion m_localRotation = Quaternion.identity;

        [SerializeField]
        FlexClothAsset m_asset;

        #endregion
    }
}
