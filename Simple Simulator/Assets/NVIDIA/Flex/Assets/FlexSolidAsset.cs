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
    public class FlexSolidAsset : FlexAsset
    {
        #region Properties

        public Mesh boundaryMesh
        {
            get { return m_boundaryMesh; }
            set { m_boundaryMesh = value; }
        }

        public Vector3 meshLocalScale
        {
            get { return m_meshLocalScale; }
            set { m_meshLocalScale = value; }
        }

        public float meshExpansion
        {
            get { return m_meshExpansion; }
            set { m_meshExpansion = value; }
        }

        public float particleSpacing
        {
            get { return m_particleSpacing; }
            set { m_particleSpacing = Mathf.Max(value, 0.01f); }
        }

        #endregion

        #region Methods


        #endregion

        #region Messages


        #endregion

        #region Protected

        protected override void ValidateFields()
        {
            base.ValidateFields();
            m_particleSpacing = Mathf.Max(m_particleSpacing, 0.01f);
        }

        protected override void RebuildAsset()
        {
            BuildFromMesh();
            base.RebuildAsset();
        }

        #endregion

        #region Private

        void BuildFromMesh()
        {
            if (m_boundaryMesh)
            {
                Vector3[] vertices = m_boundaryMesh.vertices;
                if (vertices != null && vertices.Length > 0)
                {
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        Vector3 v = vertices[i];
                        vertices[i] = new Vector3(v.x * m_meshLocalScale.x, v.y * m_meshLocalScale.y, v.z * m_meshLocalScale.z);
                    }
                    int[] indices = m_boundaryMesh.triangles;
                    if (indices != null && indices.Length > 0)
                    {
                        FlexExt.Asset.Handle assetHandle = FlexExt.CreateRigidFromMesh(ref vertices[0], vertices.Length, ref indices[0], indices.Length, m_particleSpacing, m_meshExpansion);
                        if (assetHandle)
                        {
                            StoreAsset(assetHandle.asset);

                            FlexExt.DestroyAsset(assetHandle);
                        }
                    }
                }
            }
        }

        [SerializeField]
        Mesh m_boundaryMesh = null;
        [SerializeField]
        Vector3 m_meshLocalScale = Vector3.one;
        [SerializeField, Tooltip("Particles will be moved inwards (if negative) or outwards (if positive) from the surface of the mesh according to this factor")]
        float m_meshExpansion = 0.0f;
        [SerializeField, Tooltip("The spacing used for voxelization, note that the number of voxels grows proportional to the inverse cube of radius, currently this method limits construction to resolutions < 64^3")]
        float m_particleSpacing = 0.1f;

        #endregion
    }
}
