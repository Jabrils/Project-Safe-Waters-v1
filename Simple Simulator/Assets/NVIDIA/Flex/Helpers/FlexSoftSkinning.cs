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

using System.Collections.Generic;
using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(FlexSoftActor))]
    [AddComponentMenu("NVIDIA/Flex/Flex Soft Skinning")]
    public class FlexSoftSkinning : MonoBehaviour
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
            m_skinningTarget = gameObject;
        }

        #endregion

        #region Private

        void Create()
        {
            m_actor = GetComponent<FlexSoftActor>();
            m_renderer = m_skinningTarget ? m_skinningTarget.GetComponent<SkinnedMeshRenderer>() : null;

            m_actor.onFlexUpdate += OnFlexUpdate;

            BuildSkinningInfo();
            CreateSkinningBones();
        }

        void Destroy()
        {
            DestroySkinningBones();

            m_actor.onFlexUpdate -= OnFlexUpdate;

            m_actor = null;
            m_renderer = null;
        }

        void BuildSkinningInfo()
        {
            if (m_actor && m_actor.asset && m_renderer && m_renderer.sharedMesh)
            {
                Vector3[] vertices = m_renderer.sharedMesh.vertices;

                Transform actorTransform = m_actor.transform;
                Transform rendererTransform = m_renderer.transform;

                Vector3[] shapeCenters = (Vector3[])m_actor.asset.shapeCenters.Clone();
                for (int i = 0; i < shapeCenters.Length; ++i)
                {
                    shapeCenters[i] = rendererTransform.InverseTransformPoint(actorTransform.TransformPoint(shapeCenters[i]));
                }

                Quaternion shapeRotation = Quaternion.Inverse(rendererTransform.rotation) * actorTransform.rotation;
                m_bindposes = new Matrix4x4[shapeCenters.Length];
                for (int i = 0; i < shapeCenters.Length; ++i)
                    m_bindposes[i] = Matrix4x4.TRS(shapeCenters[i], shapeRotation, Vector3.one).inverse;

                float[] skinningWeights = new float[vertices.Length * 4];
                int[] skinningIndices = new int[vertices.Length * 4];

                FlexExt.CreateSoftMeshSkinning(ref vertices[0], vertices.Length, ref shapeCenters[0], shapeCenters.Length, m_skinningFalloff, m_skinningMaxDistance, ref skinningWeights[0], ref skinningIndices[0]);

                m_boneWeights = new BoneWeight[vertices.Length];
                for (int i = 0; i < vertices.Length; ++i)
                {
                    BoneWeight w = new BoneWeight();
                    w.boneIndex0 = skinningIndices[i * 4 + 0];
                    w.boneIndex1 = skinningIndices[i * 4 + 1];
                    w.boneIndex2 = skinningIndices[i * 4 + 2];
                    w.boneIndex3 = skinningIndices[i * 4 + 3];
                    w.weight0 = skinningWeights[i * 4 + 0];
                    w.weight1 = skinningWeights[i * 4 + 1];
                    w.weight2 = skinningWeights[i * 4 + 2];
                    w.weight3 = skinningWeights[i * 4 + 3];
                    m_boneWeights[i] = w;
                }
            }
        }

        const string CLONE_SUFIX = " (Flex Clone)";

        void CreateSkinningBones()
        {
            if (Application.isPlaying && m_actor && m_actor.asset && m_renderer && m_renderer.sharedMesh)
            {
                if (m_renderer.sharedMesh.name.Contains(CLONE_SUFIX))
                {
                    m_meshInstance = m_renderer.sharedMesh;
                }
                else
                {
                    m_meshInstance = Mesh.Instantiate(m_renderer.sharedMesh);
                    m_meshInstance.name = m_renderer.sharedMesh.name + CLONE_SUFIX;
                }

                Vector2[] uv = m_meshInstance.uv;
                Vector2[] uv2 = m_meshInstance.uv2;
                Vector2[] uv3 = m_meshInstance.uv3;
                Vector2[] uv4 = m_meshInstance.uv4;

                int bonesOffset = m_renderer.sharedMesh.bindposes.Length;

                List<Matrix4x4> bindposes = new List<Matrix4x4>(m_meshInstance.bindposes);
                bindposes.AddRange(m_bindposes);
                m_meshInstance.bindposes = bindposes.ToArray();

                BoneWeight[] boneWeights = m_meshInstance.boneWeights.Length > 0 ? m_meshInstance.boneWeights : new BoneWeight[m_boneWeights.Length];
                for (int i = 0; i < boneWeights.Length; ++i)
                {
                    if (m_boneWeights[i].boneIndex0 == -1 && m_boneWeights[i].boneIndex1 == -1 &&
                        m_boneWeights[i].boneIndex2 == -1 && m_boneWeights[i].boneIndex3 == -1) continue;

                    boneWeights[i].boneIndex0 = Mathf.Max(0, m_boneWeights[i].boneIndex0) + bonesOffset;
                    boneWeights[i].weight0 = m_boneWeights[i].weight0;
                    boneWeights[i].boneIndex1 = Mathf.Max(0, m_boneWeights[i].boneIndex1) + bonesOffset;
                    boneWeights[i].weight1 = m_boneWeights[i].weight1;
                    boneWeights[i].boneIndex2 = Mathf.Max(0, m_boneWeights[i].boneIndex2) + bonesOffset;
                    boneWeights[i].weight2 = m_boneWeights[i].weight2;
                    boneWeights[i].boneIndex3 = Mathf.Max(0, m_boneWeights[i].boneIndex3) + bonesOffset;
                    boneWeights[i].weight3 = m_boneWeights[i].weight3;
                }
                m_meshInstance.boneWeights = boneWeights;

                m_meshInstance.uv = uv;
                m_meshInstance.uv2 = uv2;
                m_meshInstance.uv3 = uv3;
                m_meshInstance.uv4 = uv4;

                Vector3[] shapeCenters = m_actor.asset.shapeCenters;
                m_skinningBones = new Transform[shapeCenters.Length];
                for (int i = 0; i < shapeCenters.Length; ++i)
                {
                    GameObject bone = new GameObject("Bone" + i);
                    bone.transform.parent = transform;
                    bone.transform.localPosition = shapeCenters[i];
                    bone.transform.localRotation = Quaternion.identity;
                    bone.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                    m_skinningBones[i] = bone.transform;
                }
                List<Transform> bones = new List<Transform>(m_renderer.bones);
                bones.AddRange(m_skinningBones);
                m_renderer.bones = bones.ToArray();
                m_renderer.sharedMesh = m_meshInstance;

                m_meshInstance.RecalculateBounds();
                m_renderer.localBounds = m_meshInstance.bounds;
            }
        }

        void DestroySkinningBones()
        {
            foreach (Transform t in m_skinningBones)
                if (t) Destroy(t.gameObject);

            m_skinningBones = new Transform[0];

            if (m_meshInstance)
                Destroy(m_meshInstance);
        }

        void OnFlexUpdate(FlexContainer.ParticleData _particleData)
        {
            if (m_actor && m_actor.handle && m_skinningBones.Length > 0)
            {
                FlexExt.Instance instance = m_actor.handle.instance;
                int boneCount = m_skinningBones.Length;
                Vector3 bonePosition = Vector3.zero;
                Quaternion boneRotation = Quaternion.identity;

                for (int i = 0; i < boneCount; ++i)
                {
                    FlexUtils.FastCopy(instance.shapeTranslations, sizeof(float) * 3 * i, ref bonePosition, 0, sizeof(float) * 3);
                    m_skinningBones[i].position = bonePosition;
                    FlexUtils.FastCopy(instance.shapeRotations, sizeof(float) * 4 * i, ref boneRotation, 0, sizeof(float) * 4);
                    m_skinningBones[i].rotation = boneRotation;
                }
            }
        }

        FlexSoftActor m_actor;
        SkinnedMeshRenderer m_renderer;
        Mesh m_meshInstance;
        Transform[] m_skinningBones = new Transform[0];

        [SerializeField, Tooltip("The speed at which the bone's influence on a vertex falls off with distance")]
        float m_skinningFalloff = 1.0f;
        [SerializeField, Tooltip("The maximum distance a bone can be from a vertex before it will not influence it any more")]
        float m_skinningMaxDistance = 0.5f;

        [SerializeField]
        GameObject m_skinningTarget;
        [SerializeField]
        Matrix4x4[] m_bindposes = new Matrix4x4[0];
        [SerializeField]
        BoneWeight[] m_boneWeights = new BoneWeight[0];

        #endregion
    }
}
