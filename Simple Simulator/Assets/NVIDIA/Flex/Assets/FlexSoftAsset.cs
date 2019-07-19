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
    public class FlexSoftAsset : FlexAsset
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

        public float particleSpacing
        {
            get { return m_particleSpacing; }
            set { m_particleSpacing = Mathf.Max(value, 0.01f); }
        }

        public float volumeSampling
        {
            get { return m_volumeSampling; }
            set { m_volumeSampling = value; }
        }

        public float surfaceSampling
        {
            get { return m_surfaceSampling; }
            set { m_surfaceSampling = value; }
        }

        public float clusterSpacing
        {
            get { return m_clusterSpacing; }
            set { m_clusterSpacing = value; }
        }

        public float clusterRadius
        {
            get { return m_clusterRadius; }
            set { m_clusterRadius = value; }
        }

        public float clusterStiffness
        {
            get { return m_clusterStiffness; }
            set { m_clusterStiffness = value; }
        }

        public float linkRadius
        {
            get { return m_linkRadius; }
            set { m_linkRadius = value; }
        }

        public float linkStiffness
        {
            get { return m_linkStiffness; }
            set { m_linkStiffness = value; }
        }

        public int referenceShape
        {
            get { return m_referenceShape; }
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
            m_particleSpacing = Mathf.Max(m_particleSpacing, 0.05f);
            m_volumeSampling = Mathf.Max(m_volumeSampling, 0.0f);
            m_surfaceSampling = Mathf.Max(m_surfaceSampling, 0.0f);
            m_clusterSpacing = Mathf.Max(m_clusterSpacing, m_particleSpacing);
            m_clusterRadius = Mathf.Max(m_clusterRadius, 0.0f);
            m_clusterStiffness = Mathf.Clamp(m_clusterStiffness, 0.0f, 0.9f);
            m_linkRadius = Mathf.Max(m_linkRadius, 0.0f);
            m_linkStiffness = Mathf.Clamp(m_linkStiffness, 0.0f, 1.0f);
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
                        FlexExt.Asset.Handle assetHandle = FlexExt.CreateSoftFromMesh(ref vertices[0], vertices.Length, ref indices[0], indices.Length, m_particleSpacing, m_volumeSampling, m_surfaceSampling, m_clusterSpacing, m_clusterRadius, m_clusterStiffness, m_linkRadius, m_linkStiffness);
                        if (assetHandle)
                        {
                            StoreAsset(assetHandle.asset);

                            FlexExt.DestroyAsset(assetHandle);

                            foreach (var i in fixedParticles)
                            {
                                if (i < particles.Length) particles[i].w = 0.0f;
                            }

                            m_referenceShape = -1;
                            if (fixedParticles.Length == 0)
                            {
                                float minDist2 = float.MaxValue;
                                for (int i = 0; i < shapeCenters.Length; ++i)
                                {
                                    float dist2 = shapeCenters[i].sqrMagnitude;
                                    if (dist2 < minDist2)
                                    {
                                        dist2 = minDist2;
                                        m_referenceShape = i;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [SerializeField]
        Mesh m_boundaryMesh = null;
        [SerializeField]
        Vector3 m_meshLocalScale = Vector3.one;
        [SerializeField, Tooltip("The spacing to use when creating particles")]
        float m_particleSpacing = 0.05f;
        [SerializeField, Tooltip("Control the resolution the mesh is voxelized at in order to generate interior sampling, if the mesh is not closed then this should be set to zero and surface sampling should be used instead")]
        float m_volumeSampling = 1.0f;
        [SerializeField, Tooltip("Controls how many samples are taken of the mesh surface, this is useful to ensure fine features of the mesh are represented by particles, or if the mesh is not closed")]
        float m_surfaceSampling = 0.0f;
        [SerializeField, Tooltip("The spacing for shape-matching clusters, should be at least the particle spacing")]
        float m_clusterSpacing = 0.2f;
        [SerializeField, Tooltip("Controls the overall size of the clusters, this controls how much overlap  the clusters have which affects how smooth the final deformation is, if parts of the body are detaching then it means the clusters are not overlapping sufficiently to form a fully connected set of clusters")]
        float m_clusterRadius = 0.2f;
        [SerializeField, Tooltip("Controls the stiffness of the resulting clusters")]
        float m_clusterStiffness = 0.2f;
        [SerializeField, Tooltip("Any particles below this distance will have additional distance constraints created between them")]
        float m_linkRadius = 0.0f;
        [SerializeField, Tooltip("The stiffness of distance links")]
        float m_linkStiffness = 1.0f;
        [SerializeField]
        int m_referenceShape = -1;

        #endregion
    }
}
