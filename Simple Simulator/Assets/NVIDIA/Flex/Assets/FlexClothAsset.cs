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
    public class FlexClothAsset : FlexAsset
    {
        #region Properties

        public Mesh referenceMesh
        {
            get { return m_referenceMesh; }
            set { m_referenceMesh = value; }
        }

        public Vector3 meshLocalScale
        {
            get { return m_meshLocalScale; }
            set { m_meshLocalScale = value; }
        }

        public int meshTessellation
        {
            get { return m_meshTesselation; }
            set { m_meshTesselation = value; }
        }

        public float weldingThreshold
        {
            get { return m_weldingThreshold; }
            set { m_weldingThreshold = value; }
        }

        public float stretchStiffness
        {
            get { return m_stretchStiffness; }
            set { m_stretchStiffness = value; }
        }

        public float bendStiffness
        {
            get { return m_bendStiffness; }
            set { m_bendStiffness = value; }
        }

        public float tetherStiffness
        {
            get { return m_tetherStiffness; }
            set { m_tetherStiffness = value; }
        }

        public float tetherGive
        {
            get { return m_tetherGive; }
            set { m_tetherGive = value; }
        }

        public float pressure
        {
            get { return m_pressure; }
            set { m_pressure = value; }
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
            if (m_referenceMesh)
            {
                Vector3[] vertices = m_referenceMesh.vertices;
                int[] triangles = m_referenceMesh.triangles;
                if (vertices != null && vertices.Length > 0 && triangles != null && triangles.Length > 0)
                {
                    for (int i = 0; i < m_meshTesselation; ++i)
                    {
                        Vector3[] newVertices = new Vector3[triangles.Length * 4];
                        int[] newTriangles = new int[triangles.Length * 4];
                        for (int j = 0; j < triangles.Length / 3; ++j)
                        {
                            Vector3 v0 = vertices[triangles[j * 3 + 0]], v1 = vertices[triangles[j * 3 + 1]], v2 = vertices[triangles[j * 3 + 2]];
                            Vector3 v3 = (v0 + v1) * 0.5f, v4 = (v1 + v2) * 0.5f, v5 = (v2 + v0) * 0.5f;
                            int i0 = j * 12 + 0, i1 = j * 12 +  1, i2 = j * 12 +  2;
                            int i3 = j * 12 + 3, i4 = j * 12 +  4, i5 = j * 12 +  5;
                            int i6 = j * 12 + 6, i7 = j * 12 +  7, i8 = j * 12 +  8;
                            int i9 = j * 12 + 9, iA = j * 12 + 10, iB = j * 12 + 11;
                            newTriangles[i0] = i0; newTriangles[i1] = i1; newTriangles[i2] = i2;
                            newTriangles[i3] = i3; newTriangles[i4] = i4; newTriangles[i5] = i5;
                            newTriangles[i6] = i6; newTriangles[i7] = i7; newTriangles[i8] = i8;
                            newTriangles[i9] = i9; newTriangles[iA] = iA; newTriangles[iB] = iB;
                            newVertices[i0] = v0; newVertices[i1] = v3; newVertices[i2] = v5;
                            newVertices[i3] = v3; newVertices[i4] = v1; newVertices[i5] = v4;
                            newVertices[i6] = v5; newVertices[i7] = v4; newVertices[i8] = v2;
                            newVertices[i9] = v3; newVertices[iA] = v4; newVertices[iB] = v5;
                        }
                        vertices = newVertices;
                        triangles = newTriangles;
                    }
                    int[] uniqueVerts = new int[vertices.Length];
                    int[] originalToUniqueMap = new int[vertices.Length];
                    int particleCount = FlexExt.CreateWeldedMeshIndices(ref vertices[0], vertices.Length, ref uniqueVerts[0], ref originalToUniqueMap[0], m_weldingThreshold);
                    Vector4[] particles = new Vector4[particleCount];
                    for (int i = 0; i < particleCount; ++i)
                    {
                        Vector3 v = vertices[uniqueVerts[i]];
                        particles[i] = new Vector4(v.x * m_meshLocalScale.x, v.y * m_meshLocalScale.y, v.z * m_meshLocalScale.z, 1.0f);
                    }
                    foreach (var i in fixedParticles)
                    {
                        if (i < particleCount) particles[i].w = 0.0f;
                    }
                    int indexCount = triangles.Length;
                    int[] indices = new int[indexCount];
                    for (int i = 0; i < indexCount; ++i)
                    {
                        indices[i] = originalToUniqueMap[triangles[i]];
                    }
                    FlexExt.Asset.Handle assetHandle = FlexExt.CreateClothFromMesh(ref particles[0], particles.Length, ref indices[0], indices.Length / 3, m_stretchStiffness, m_bendStiffness, m_tetherStiffness, m_tetherGive, m_pressure);
                    if (assetHandle)
                    {
                        StoreAsset(assetHandle.asset);
                        FlexExt.DestroyAsset(assetHandle);
                    }
                }
            }
        }

        [SerializeField]
        Mesh m_referenceMesh = null;
        [SerializeField]
        Vector3 m_meshLocalScale = Vector3.one;
        [SerializeField]
        int m_meshTesselation = 0;
        [SerializeField]
        float m_weldingThreshold = 0.001f;
        [SerializeField, Tooltip("The stiffness coefficient for stretch constraints")]
        float m_stretchStiffness = 1.0f;
        [SerializeField, Tooltip("The stiffness coefficient used for bending constraints")]
        float m_bendStiffness = 1.0f;
        [SerializeField, Tooltip("If > 0.0f then the function will create tethers attached to particles with zero inverse mass. These are unilateral, long-range attachments, which can greatly reduce stretching even at low iteration counts")]
        float m_tetherStiffness = 0.0f;
        [SerializeField, Tooltip("Because tether constraints are so effective at reducing stiffness, it can be useful to allow a small amount of extension before the constraint activates")]
        float m_tetherGive = 0.0f;
        [SerializeField, Tooltip("If > 0.0f then a volume (pressure) constraint will also be added to the asset, the rest volume and stiffness will be automatically computed by this function")]
        float m_pressure = 0.0f;

        #endregion
    }
}
