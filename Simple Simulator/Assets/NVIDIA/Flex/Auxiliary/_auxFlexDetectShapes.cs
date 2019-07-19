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
using System.Runtime.InteropServices;
using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("")]
    public class _auxFlexDetectShapes : MonoBehaviour
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

        void OnTriggerEnter(Collider collider)
        {
            if (collider.isTrigger) return;
            m_colliders.Add(collider, new ShapeData(collider));
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.isTrigger) return;
            if (m_colliders.ContainsKey(collider))
            {
                m_colliders[collider].Clear();
                m_colliders.Remove(collider);
            }
        }

        #endregion

        #region Methods

        public void UpdateShapes(FlexContainer.ParticleData _particleData)
        {
            int colliderCount = 0;

            foreach (var item in m_colliders)
            {
                Collider collider = item.Key;

                if (!(collider is SphereCollider) && !(collider is CapsuleCollider) && !(collider is BoxCollider) && !(collider is MeshCollider))
                    continue;

                ++colliderCount;
            }
            if (colliderCount > 0)
            {
                if (m_geometryBuffer == null || colliderCount > m_geometryBuffer.count)
                {
                    if (m_geometryBuffer != null)
                    {
                        m_geometryBuffer.Release();
                        m_shapePositionBuffer.Release();
                        m_shapeRotationBuffer.Release();
                        m_shapePrevPositionBuffer.Release();
                        m_shapePrevRotationBuffer.Release();
                        m_shapeFlagsBuffer.Release();
                    }
                    m_geometryBuffer = new FlexBuffer(FlexContainer.library, colliderCount, sizeof(float) * 4);
                    m_shapePositionBuffer = new FlexBuffer(FlexContainer.library, colliderCount, sizeof(float) * 4);
                    m_shapeRotationBuffer = new FlexBuffer(FlexContainer.library, colliderCount, sizeof(float) * 4);
                    m_shapePrevPositionBuffer = new FlexBuffer(FlexContainer.library, colliderCount, sizeof(float) * 4);
                    m_shapePrevRotationBuffer = new FlexBuffer(FlexContainer.library, colliderCount, sizeof(float) * 4);
                    m_shapeFlagsBuffer = new FlexBuffer(FlexContainer.library, colliderCount, sizeof(int));
                }

                int shapeIndex = 0;

                foreach (var item in m_colliders)
                {
                    Collider collider = item.Key;
                    ShapeData shapeData = item.Value;

                    if (!(collider is SphereCollider) && !(collider is CapsuleCollider) && !(collider is BoxCollider) && !(collider is MeshCollider))
                        continue;

                    m_geometryBuffer.Set(shapeIndex, shapeData.geometry);
                    m_shapeFlagsBuffer.Set(shapeIndex, shapeData.flags);
                    m_shapePositionBuffer.Set(shapeIndex, (Vector4)(collider.transform.position + collider.transform.rotation * shapeData.shapeCenter));
                    m_shapeRotationBuffer.Set(shapeIndex, collider.transform.rotation * shapeData.shapePreRotation);
                    m_shapePrevPositionBuffer.Set(shapeIndex, (Vector4)shapeData.shapePrevPosition);
                    m_shapePrevRotationBuffer.Set(shapeIndex, shapeData.shapePrevRotation);

                    shapeData.shapePrevPosition = collider.transform.position + collider.transform.rotation * shapeData.shapeCenter;
                    shapeData.shapePrevRotation = collider.transform.rotation * shapeData.shapePreRotation;

                    ++shapeIndex;
                }

                Flex.SetShapes(m_scene.container.solver, m_geometryBuffer.handle,
                               m_shapePositionBuffer.handle, m_shapeRotationBuffer.handle,
                               m_shapePrevPositionBuffer.handle, m_shapePrevRotationBuffer.handle,
                               m_shapeFlagsBuffer.handle, colliderCount);
            }
            else
            {
                Flex.SetShapes(m_scene.container.solver, default(Flex.Buffer),
                               default(Flex.Buffer), default(Flex.Buffer),
                               default(Flex.Buffer), default(Flex.Buffer),
                               default(Flex.Buffer), 0);
            }

            Vector3 lower = Vector3.zero, upper = Vector3.zero;
            FlexUtils.FastCopy(_particleData.particleData.lower, 0, ref lower, 0, sizeof(float) * 3);
            FlexUtils.FastCopy(_particleData.particleData.upper, 0, ref upper, 0, sizeof(float) * 3);

            if (lower.x < upper.x && lower.y < upper.y && lower.z < upper.z)
            {
                m_trigger.transform.position = (upper + lower) * 0.5f;
                m_trigger.size = (upper - lower) + Vector3.one * m_scene.container.radius;
            }
        }

        #endregion

        #region Private

        void Create()
        {
            m_scene = GetComponentInParent<FlexScene>();
            if (m_scene == null)
            {
                Debug.LogError("_auxFlexDetectShapes should be parented to a FlexScene");
                Debug.Break();
            }

            if (m_scene && m_scene.container && m_scene.container.handle)
            {
                Rigidbody rigidbody = GetComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                m_trigger = GetComponent<BoxCollider>();
                m_trigger.isTrigger = true;
            }
        }

        void Destroy()
        {
            if (m_geometryBuffer != null) { m_geometryBuffer.Release(); m_geometryBuffer = null; }
            if (m_shapePositionBuffer != null) { m_shapePositionBuffer.Release(); m_shapePositionBuffer = null; }
            if (m_shapeRotationBuffer != null) { m_shapeRotationBuffer.Release(); m_shapeRotationBuffer = null; }
            if (m_shapePrevPositionBuffer != null) { m_shapePrevPositionBuffer.Release(); m_shapePrevPositionBuffer = null; }
            if (m_shapePrevRotationBuffer != null) { m_shapePrevRotationBuffer.Release(); m_shapePrevRotationBuffer = null; }
            if (m_shapeFlagsBuffer != null) { m_shapeFlagsBuffer.Release(); m_shapeFlagsBuffer = null; }

            foreach (var item in m_colliders) item.Value.Clear();
            m_colliders.Clear();
        }

        FlexScene m_scene;
        BoxCollider m_trigger;

        Dictionary<Collider, ShapeData> m_colliders = new Dictionary<Collider, ShapeData>();

        FlexBuffer m_geometryBuffer;
        FlexBuffer m_shapePositionBuffer, m_shapeRotationBuffer;
        FlexBuffer m_shapePrevPositionBuffer, m_shapePrevRotationBuffer;
        FlexBuffer m_shapeFlagsBuffer;

        #endregion
    }

    class ShapeData
    {
        public int flags;
        public Flex.CollisionGeometry geometry;
        public Vector3 shapeMin, shapeMax, shapeCenter = Vector3.zero;
        public Quaternion shapePreRotation = Quaternion.identity;
        public Vector3 shapePrevPosition = Vector3.zero;
        public Quaternion shapePrevRotation = Quaternion.identity;
        public ShapeData(Collider collider)
        {
            bool dynamic = false;
            if (collider is SphereCollider)
            {
                SphereCollider sphereCollider = collider as SphereCollider;
                Vector3 scale = collider.transform.lossyScale;
                geometry.sphere.radius = sphereCollider.radius * Mathf.Max(scale.x, scale.y, scale.z);
                flags = Flex.MakeShapeFlags(Flex.CollisionShapeType.Sphere, dynamic);
                shapeMin = Vector3.one * -geometry.sphere.radius;
                shapeMax = Vector3.one * geometry.sphere.radius;
                shapeCenter = sphereCollider.center;
                shapeCenter.x *= scale.x; shapeCenter.y *= scale.y; shapeCenter.z *= scale.z;
            }
            else if (collider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                Vector3 scale = collider.transform.lossyScale;
                if (capsuleCollider.direction == 1) scale = new Vector3(scale.y, scale.z, scale.x);
                if (capsuleCollider.direction == 2) scale = new Vector3(scale.z, scale.x, scale.y);
                geometry.capsule.radius = capsuleCollider.radius * Mathf.Max(scale.y, scale.z);
                geometry.capsule.halfHeight = capsuleCollider.height * scale.x * 0.5f - geometry.capsule.radius;
                flags = Flex.MakeShapeFlags(Flex.CollisionShapeType.Capsule, dynamic);
                shapeMin = new Vector3(-geometry.capsule.halfHeight - geometry.capsule.radius, -geometry.capsule.radius, -geometry.capsule.radius);
                shapeMax = new Vector3(geometry.capsule.halfHeight + geometry.capsule.radius, geometry.capsule.radius, geometry.capsule.radius);
                shapeCenter = capsuleCollider.center;
                shapeCenter.x *= scale.x; shapeCenter.y *= scale.y; shapeCenter.z *= scale.z;
                if (capsuleCollider.direction == 1) shapePreRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                if (capsuleCollider.direction == 2) shapePreRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            }
            else if (collider is BoxCollider)
            {
                BoxCollider boxCollider = collider as BoxCollider;
                Vector3 scale = collider.transform.lossyScale;
                Vector3 halfSize = new Vector3(boxCollider.size.x * scale.x * 0.5f, boxCollider.size.y * scale.y * 0.5f, boxCollider.size.z * scale.z * 0.5f);
                geometry.box.halfExtents = halfSize;
                flags = Flex.MakeShapeFlags(Flex.CollisionShapeType.Box, dynamic);
                shapeMin = -halfSize;
                shapeMax = halfSize;
                shapeCenter = boxCollider.center;
                shapeCenter.x *= scale.x; shapeCenter.y *= scale.y; shapeCenter.z *= scale.z;
            }
            else if (collider is MeshCollider)
            {
                MeshCollider meshCollider = collider as MeshCollider;
                Mesh mesh = meshCollider.sharedMesh;
                if (mesh)
                {
                    Vector3 scale = collider.transform.lossyScale;
                    Vector3 boundsMin = new Vector3(mesh.bounds.min.x * scale.x, mesh.bounds.min.y * scale.y, mesh.bounds.min.z * scale.z);
                    Vector3 boundsMax = new Vector3(mesh.bounds.max.x * scale.x, mesh.bounds.max.y * scale.y, mesh.bounds.max.z * scale.z);
                    Vector3[] vertices = mesh.vertices;
                    int[] triangles = mesh.triangles;
                    if (vertices.Length > 0 && triangles.Length > 0)
                    {
                        bool useConvex = meshCollider.convex;
                        if (useConvex)
                        {
                            Vector4[] planes = new Vector4[triangles.Length / 3];
                            Vector3[] bounds = new Vector3[2];
                            int planeCount = FlexUtils.ConvexPlanes(ref vertices[0], ref scale, ref triangles[0], triangles.Length / 3, ref planes[0], ref bounds[0]);
                            // Convex meshes in Flex can't have more than 64 faces
                            if (planeCount <= 64)
                            {
                                m_flexConvex = Flex.CreateConvexMesh(FlexContainer.library);
                                Flex.Buffer planeBuffer = Flex.AllocBuffer(FlexContainer.library, planeCount, sizeof(float) * 4, Flex.BufferType.Host);
                                FlexUtils.FastCopy(planes, Flex.Map(planeBuffer)); Flex.Unmap(planeBuffer);
                                Flex.UpdateConvexMesh(FlexContainer.library, m_flexConvex, planeBuffer, planeCount, ref bounds[0], ref bounds[1]);
                                geometry.convexMesh.scale = Vector3.one;
                                geometry.convexMesh.mesh = m_flexConvex;
                                Flex.FreeBuffer(planeBuffer);
                                flags = Flex.MakeShapeFlags(Flex.CollisionShapeType.ConvexMesh, dynamic);
                                shapeMin = bounds[0];
                                shapeMax = bounds[1];
                            }
                            else
                                useConvex = false;
                        }
                        if (!useConvex)
                        {
                            int[] uniqueInds = new int[vertices.Length];
                            int[] originalToUniqueMap = new int[vertices.Length];
                            int uniqueVertCount = FlexExt.CreateWeldedMeshIndices(ref vertices[0], vertices.Length, ref uniqueInds[0], ref originalToUniqueMap[0], 0.0001f); // @@@
                            Vector4[] uniqueVerts = new Vector4[uniqueVertCount];
                            for (int i = 0; i < uniqueVertCount; ++i) uniqueVerts[i] = new Vector3(vertices[uniqueInds[i]].x * scale.x, vertices[uniqueInds[i]].y * scale.y, vertices[uniqueInds[i]].z * scale.z);
                            Flex.Buffer vertexBuffer = Flex.AllocBuffer(FlexContainer.library, uniqueVerts.Length, sizeof(float) * 4, Flex.BufferType.Host);
                            FlexUtils.FastCopy(uniqueVerts, Flex.Map(vertexBuffer)); Flex.Unmap(vertexBuffer);
                            int[] indices = new int[triangles.Length];
                            for (int i = 0; i < triangles.Length; ++i) indices[i] = originalToUniqueMap[triangles[i]];
                            Flex.Buffer indexBuffer = Flex.AllocBuffer(FlexContainer.library, indices.Length, sizeof(int), Flex.BufferType.Host);
                            FlexUtils.FastCopy(indices, Flex.Map(indexBuffer)); Flex.Unmap(indexBuffer);
                            m_flexMesh = Flex.CreateTriangleMesh(FlexContainer.library);
                            Flex.UpdateTriangleMesh(FlexContainer.library, m_flexMesh, vertexBuffer, indexBuffer, uniqueVertCount, indices.Length / 3, ref boundsMin, ref boundsMax);
                            geometry.triMesh.scale = Vector3.one;
                            geometry.triMesh.mesh = m_flexMesh;
                            Flex.FreeBuffer(vertexBuffer);
                            Flex.FreeBuffer(indexBuffer);
                            flags = Flex.MakeShapeFlags(Flex.CollisionShapeType.TriangleMesh, dynamic);
                            Flex.GetTriangleMeshBounds(FlexContainer.library, m_flexMesh, ref shapeMin, ref shapeMax);
                        }
                    }
                }
            }
            shapePrevPosition = collider.transform.position + collider.transform.rotation * shapeCenter;
            shapePrevRotation = collider.transform.rotation * shapePreRotation;
        }
        public void Clear()
        {
            if (FlexContainer.library && m_flexMesh)
                Flex.DestroyTriangleMesh(FlexContainer.library, m_flexMesh);
            m_flexMesh.Clear();
            if (FlexContainer.library && m_flexConvex)
                Flex.DestroyConvexMesh(FlexContainer.library, m_flexConvex);
            m_flexConvex.Clear();
        }
        Flex.TriangleMesh m_flexMesh;
        Flex.ConvexMesh m_flexConvex;
    }
}
