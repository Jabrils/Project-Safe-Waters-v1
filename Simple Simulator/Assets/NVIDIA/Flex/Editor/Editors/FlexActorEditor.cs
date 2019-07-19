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

using UnityEditor;
using UnityEngine;

namespace NVIDIA.Flex
{
    public class FlexActorEditor : Editor
    {
        SerializedProperty m_container;
        SerializedProperty m_particleGroup;
        SerializedProperty m_selfCollide;
        SerializedProperty m_selfCollideFilter;
        SerializedProperty m_fluid;
        SerializedProperty m_massScale;
        SerializedProperty m_drawParticles;

        protected SerializedProperty m_recreateActor;

        protected virtual void OnEnable()
        {
            m_container = serializedObject.FindProperty("m_container");
            m_particleGroup = serializedObject.FindProperty("m_particleGroup");
            m_selfCollide = serializedObject.FindProperty("m_selfCollide");
            m_selfCollideFilter = serializedObject.FindProperty("m_selfCollideFilter");
            m_fluid = serializedObject.FindProperty("m_fluid");
            m_massScale = serializedObject.FindProperty("m_massScale");
            m_drawParticles = serializedObject.FindProperty("m_drawParticles");

            m_recreateActor = serializedObject.FindProperty("m_recreateActor");
        }

        protected void ContainerUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_container);
            if (EditorGUI.EndChangeCheck()) m_recreateActor.intValue = 2;
        }

        protected void ParticlesUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_particleGroup);
            EditorGUILayout.PropertyField(m_selfCollide);
            EditorGUILayout.PropertyField(m_selfCollideFilter);
            EditorGUILayout.PropertyField(m_fluid);
            EditorGUILayout.PropertyField(m_massScale);
            if (EditorGUI.EndChangeCheck()) m_recreateActor.intValue = 1;
        }

        protected void DebugUI()
        {
            EditorGUILayout.PropertyField(m_drawParticles);
        }
    }
}
