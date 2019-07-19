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
    public class FlexSourceAsset : FlexAsset
    {
        #region Properties

        public Mesh surfaceMesh
        {
            get { return m_surfaceMesh; }
            set { m_surfaceMesh = value; }
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

        public Vector3[] nozzlePositions
        {
            get { return m_nozzlePositions; }
        }

        public Vector3[] nozzleDirections
        {
            get { return m_nozzleDirections; }
        }

        public int nozzleCount
        {
            get { return m_nozzlePositions.Length; }
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

        static Vector3 Scale(Vector3 v, Vector3 s)
        {
            return new Vector3(v.x * s.x, v.y * s.y, v.z * s.z);
        }

        void BuildFromMesh()
        {
            FlexExt.Asset reserveSpace = new FlexExt.Asset();
            reserveSpace.maxParticles = maxParticles;
            StoreAsset(reserveSpace);

            if (m_surfaceMesh)
            {
                Vector3[] vertices = m_surfaceMesh.vertices;
                int[] triangles = m_surfaceMesh.triangles;
                if (vertices != null && vertices.Length > 0 && triangles != null && triangles.Length > 0)
                {
                    List<Vector3> surface = new List<Vector3>();
                    for (int i = 0; i < triangles.Length / 3; ++i)
                    {
                        surface.Add(Scale(vertices[triangles[i * 3 + 0]], m_meshLocalScale));
                        surface.Add(Scale(vertices[triangles[i * 3 + 1]], m_meshLocalScale));
                        surface.Add(Scale(vertices[triangles[i * 3 + 2]], m_meshLocalScale));
                    }
                    for (int i = 0; i < m_meshTesselation; ++i)
                    {
                        List<Vector3> newSurface = new List<Vector3>();
                        for (int j = 0; j < surface.Count / 3; ++j)
                        {
                            Vector3 v0 = surface[j * 3 + 0], v1 = surface[j * 3 + 1], v2 = surface[j * 3 + 2];
                            Vector3 v3 = (v0 + v1) * 0.5f, v4 = (v1 + v2) * 0.5f, v5 = (v2 + v0) * 0.5f;
                            newSurface.Add(v0); newSurface.Add(v3); newSurface.Add(v5);
                            newSurface.Add(v3); newSurface.Add(v1); newSurface.Add(v4);
                            newSurface.Add(v5); newSurface.Add(v4); newSurface.Add(v2);
                            newSurface.Add(v3); newSurface.Add(v4); newSurface.Add(v5);
                        }
                        surface = newSurface;
                    }
                    List<Vector3> pointList = new List<Vector3>();
                    List<Vector3> normalList = new List<Vector3>();
                    for (int i = 0; i < surface.Count / 3; ++i)
                    {
                        Vector3[] v = new Vector3[] { surface[i * 3 + 0], surface[i * 3 + 1], surface[i * 3 + 2] };
                        Vector3 n = Vector3.Cross(v[1] - v[0], v[2] - v[0]);
                        for (int j = 0; j < 3; ++j)
                        {
                            bool newPoint = true;
                            for (int k = 0; k < pointList.Count; ++k)
                            {
                                if ((v[j] - pointList[k]).sqrMagnitude <= float.Epsilon)
                                {
                                    normalList[k] += n;
                                    newPoint = false;
                                    break;
                                }
                            }
                            if (newPoint)
                            {
                                pointList.Add(v[j]);
                                normalList.Add(n);
                            }
                        }
                    }
                    for (int i = 0; i < normalList.Count; ++i)
                    {
                        normalList[i] = normalList[i].normalized;
                    }
                    m_nozzlePositions = pointList.ToArray();
                    m_nozzleDirections = normalList.ToArray();
                }
            }
        }

        [SerializeField]
        Mesh m_surfaceMesh = null;
        [SerializeField]
        Vector3 m_meshLocalScale = Vector3.one;
        [SerializeField]
        int m_meshTesselation = 0;
        [SerializeField]
        Vector3[] m_nozzlePositions = new Vector3[0];
        [SerializeField]
        Vector3[] m_nozzleDirections = new Vector3[0];

        #endregion
    }
}
