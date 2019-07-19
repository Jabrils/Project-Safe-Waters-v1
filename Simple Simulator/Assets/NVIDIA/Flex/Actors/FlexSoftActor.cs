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

using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("NVIDIA/Flex/Flex Soft Actor")]
    public class FlexSoftActor : FlexActor
    {
        #region Properties

        public new FlexSoftAsset asset
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
            if (m_asset && m_asset.boundaryMesh)
            {
                Gizmos.color = new Color(0, 0, 0, 0);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawMesh(m_asset.boundaryMesh, Vector3.zero, Quaternion.identity, m_asset.meshLocalScale);
            }
        }

        void OnDrawGizmosSelected()
        {
            if (m_asset && m_asset.boundaryMesh)
            {
                Gizmos.color = GIZMO_COLOR;
                Bounds bounds = m_asset.boundaryMesh.bounds;
                if (m_currentContainer)
                    bounds.Expand(Mathf.Max(m_currentContainer.solidRest - m_asset.particleSpacing, 0.0f));
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 scaledSize = new Vector3(bounds.size.x * m_asset.meshLocalScale.x, bounds.size.y * m_asset.meshLocalScale.y, bounds.size.z * m_asset.meshLocalScale.z);
                Gizmos.DrawWireCube(bounds.center, scaledSize);
            }
        }

        #endregion

        #region Protected

        protected override FlexAsset subclassAsset { get { return m_asset; } }

        protected override void CreateInstance()
        {
            base.CreateInstance();
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
            if (Application.isPlaying && handle && m_asset.referenceShape >= 0 && m_asset.referenceShape < m_asset.shapeCenters.Length)
            {
                FlexExt.Instance instance = handle.instance;
                Vector3 position = Vector3.zero;
                FlexUtils.FastCopy(instance.shapeTranslations, m_asset.referenceShape * 12, ref position, 0, 12);
                Quaternion rotation = Quaternion.identity;
                FlexUtils.FastCopy(instance.shapeRotations, m_asset.referenceShape * 16, ref rotation, 0, 16);
                transform.position = position - rotation * m_asset.shapeCenters[m_asset.referenceShape];
                transform.rotation = rotation;
                transform.hasChanged = false;
            }

            base.OnFlexUpdate(_particleData);
        }

        #endregion

        #region Private

        [SerializeField]
        FlexSoftAsset m_asset;

        #endregion
    }
}
