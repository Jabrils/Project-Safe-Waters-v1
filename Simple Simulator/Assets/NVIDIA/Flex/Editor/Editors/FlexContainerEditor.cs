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
    [CustomEditor(typeof(FlexContainer))]
    public class FlexContainerEditor : Editor
    {
        SerializedProperty m_simpleMode;

        SerializedProperty m_simpleMaxParticles;
        SerializedProperty m_simpleSubstepCount;
        SerializedProperty m_simpleIterationCount;
        SerializedProperty m_simpleGravity;
        SerializedProperty m_simpleParticleSize;
        SerializedProperty m_simpleParticleFriction;
        SerializedProperty m_simpleParticleRestitution;
        SerializedProperty m_simpleParticleAdhesion;
        SerializedProperty m_simpleParticleDamping;
        SerializedProperty m_simpleClothWind;
        SerializedProperty m_simpleClothDrag;
        SerializedProperty m_simpleClothLift;
        SerializedProperty m_simpleFluidCohesion;
        SerializedProperty m_simpleFluidTension;
        SerializedProperty m_simpleFluidViscosity;
        SerializedProperty m_simpleFluidVorticity;
        SerializedProperty m_simpleFluidAnisotropy;
        SerializedProperty m_simpleFluidMinScale;
        SerializedProperty m_simpleFluidMaxScale;

        SerializedProperty m_maxParticles;
        //SerializedProperty m_maxDiffuse;
        SerializedProperty m_maxNeighbors;
        SerializedProperty m_maxContacts;
        SerializedProperty m_recreateSolver;
        SerializedProperty m_substepCount;
        SerializedProperty m_iterationCount;
        SerializedProperty m_gravity;
        SerializedProperty m_radius;
        SerializedProperty m_solidRest;
        SerializedProperty m_fluidRest;
        SerializedProperty m_staticFriction;
        SerializedProperty m_dynamicFriction;
        SerializedProperty m_particleFriction;
        SerializedProperty m_restitution;
        SerializedProperty m_adhesion;
        SerializedProperty m_sleepThreshold;
        SerializedProperty m_maxSpeed;
        SerializedProperty m_maxAcceleration;
        SerializedProperty m_shockPropagation;
        SerializedProperty m_dissipation;
        SerializedProperty m_damping;
        SerializedProperty m_wind;
        SerializedProperty m_drag;
        SerializedProperty m_lift;
        SerializedProperty m_fluid;
        SerializedProperty m_cohesion;
        SerializedProperty m_surfaceTension;
        SerializedProperty m_viscosity;
        SerializedProperty m_vorticityConfinement;
        SerializedProperty m_anisotropyScale;
        SerializedProperty m_anisotropyMin;
        SerializedProperty m_anisotropyMax;
        SerializedProperty m_smoothing;
        SerializedProperty m_solidPressure;
        SerializedProperty m_freeSurfaceDrag;
        SerializedProperty m_buoyancy;
        //SerializedProperty m_diffuseThreshold;
        //SerializedProperty m_diffuseBuoyancy;
        //SerializedProperty m_diffuseDrag;
        //SerializedProperty m_diffuseBallistic;
        //SerializedProperty m_diffuseSortAxis;
        //SerializedProperty m_diffuseLifetime;
        //SerializedProperty m_plasticThreshold;
        //SerializedProperty m_plasticCreep;
        SerializedProperty m_collisionDistance;
        SerializedProperty m_particleCollisionMargin;
        SerializedProperty m_shapeCollisionMargin;
        SerializedProperty m_planes;
        SerializedProperty m_relaxationMode;
        SerializedProperty m_relaxationFactor;

        SerializedProperty m_fluidMaterial;

        SerializedProperty m_showTimers;

        protected virtual void OnEnable()
        {
            m_simpleMode = serializedObject.FindProperty("m_simpleMode");

            m_simpleMaxParticles = serializedObject.FindProperty("m_simpleMaxParticles");
            m_simpleSubstepCount = serializedObject.FindProperty("m_simpleSubstepCount");
            m_simpleIterationCount = serializedObject.FindProperty("m_simpleIterationCount");
            m_simpleGravity = serializedObject.FindProperty("m_simpleGravity");
            m_simpleParticleSize = serializedObject.FindProperty("m_simpleParticleSize");
            m_simpleParticleFriction = serializedObject.FindProperty("m_simpleParticleFriction");
            m_simpleParticleRestitution = serializedObject.FindProperty("m_simpleParticleRestitution");
            m_simpleParticleAdhesion = serializedObject.FindProperty("m_simpleParticleAdhesion");
            m_simpleParticleDamping = serializedObject.FindProperty("m_simpleParticleDamping");
            m_simpleClothWind = serializedObject.FindProperty("m_simpleClothWind");
            m_simpleClothDrag = serializedObject.FindProperty("m_simpleClothDrag");
            m_simpleClothLift = serializedObject.FindProperty("m_simpleClothLift");
            m_simpleFluidCohesion = serializedObject.FindProperty("m_simpleFluidCohesion");
            m_simpleFluidTension = serializedObject.FindProperty("m_simpleFluidTension");
            m_simpleFluidViscosity = serializedObject.FindProperty("m_simpleFluidViscosity");
            m_simpleFluidVorticity = serializedObject.FindProperty("m_simpleFluidVorticity");
            m_simpleFluidAnisotropy = serializedObject.FindProperty("m_simpleFluidAnisotropy");
            m_simpleFluidMinScale = serializedObject.FindProperty("m_simpleFluidMinScale");
            m_simpleFluidMaxScale = serializedObject.FindProperty("m_simpleFluidMaxScale");

            m_maxParticles = serializedObject.FindProperty("m_maxParticles");
            //m_maxDiffuse = serializedObject.FindProperty("m_maxDiffuse");
            m_maxNeighbors = serializedObject.FindProperty("m_maxNeighbors");
            m_maxContacts = serializedObject.FindProperty("m_maxContacts");
            m_recreateSolver = serializedObject.FindProperty("m_recreateSolver");
            m_substepCount = serializedObject.FindProperty("m_substepCount");
            m_iterationCount = serializedObject.FindProperty("m_iterationCount");
            m_gravity = serializedObject.FindProperty("m_gravity");
            m_radius = serializedObject.FindProperty("m_radius");
            m_solidRest = serializedObject.FindProperty("m_solidRest");
            m_fluidRest = serializedObject.FindProperty("m_fluidRest");
            m_staticFriction = serializedObject.FindProperty("m_staticFriction");
            m_dynamicFriction = serializedObject.FindProperty("m_dynamicFriction");
            m_particleFriction = serializedObject.FindProperty("m_particleFriction");
            m_restitution = serializedObject.FindProperty("m_restitution");
            m_adhesion = serializedObject.FindProperty("m_adhesion");
            m_sleepThreshold = serializedObject.FindProperty("m_sleepThreshold");
            m_maxSpeed = serializedObject.FindProperty("m_maxSpeed");
            m_maxAcceleration = serializedObject.FindProperty("m_maxAcceleration");
            m_shockPropagation = serializedObject.FindProperty("m_shockPropagation");
            m_dissipation = serializedObject.FindProperty("m_dissipation");
            m_damping = serializedObject.FindProperty("m_damping");
            m_wind = serializedObject.FindProperty("m_wind");
            m_drag = serializedObject.FindProperty("m_drag");
            m_lift = serializedObject.FindProperty("m_lift");
            m_fluid = serializedObject.FindProperty("m_fluid");
            m_cohesion = serializedObject.FindProperty("m_cohesion");
            m_surfaceTension = serializedObject.FindProperty("m_surfaceTension");
            m_viscosity = serializedObject.FindProperty("m_viscosity");
            m_vorticityConfinement = serializedObject.FindProperty("m_vorticityConfinement");
            m_anisotropyScale = serializedObject.FindProperty("m_anisotropyScale");
            m_anisotropyMin = serializedObject.FindProperty("m_anisotropyMin");
            m_anisotropyMax = serializedObject.FindProperty("m_anisotropyMax");
            m_smoothing = serializedObject.FindProperty("m_smoothing");
            m_solidPressure = serializedObject.FindProperty("m_solidPressure");
            m_freeSurfaceDrag = serializedObject.FindProperty("m_freeSurfaceDrag");
            m_buoyancy = serializedObject.FindProperty("m_buoyancy");
            //m_diffuseThreshold = serializedObject.FindProperty("m_diffuseThreshold");
            //m_diffuseBuoyancy = serializedObject.FindProperty("m_diffuseBuoyancy");
            //m_diffuseDrag = serializedObject.FindProperty("m_diffuseDrag");
            //m_diffuseBallistic = serializedObject.FindProperty("m_diffuseBallistic");
            //m_diffuseSortAxis = serializedObject.FindProperty("m_diffuseSortAxis");
            //m_diffuseLifetime = serializedObject.FindProperty("m_diffuseLifetime");
            //m_plasticThreshold = serializedObject.FindProperty("m_plasticThreshold");
            //m_plasticCreep = serializedObject.FindProperty("m_plasticCreep");
            m_collisionDistance = serializedObject.FindProperty("m_collisionDistance");
            m_particleCollisionMargin = serializedObject.FindProperty("m_particleCollisionMargin");
            m_shapeCollisionMargin = serializedObject.FindProperty("m_shapeCollisionMargin");
            m_planes = serializedObject.FindProperty("m_planes");
            m_relaxationMode = serializedObject.FindProperty("m_relaxationMode");
            m_relaxationFactor = serializedObject.FindProperty("m_relaxationFactor");

            m_fluidMaterial = serializedObject.FindProperty("m_fluidMaterial");

            m_showTimers = serializedObject.FindProperty("m_showTimers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button(m_simpleMode.boolValue ? "Switch to advanced mode" : "Revert to simple mode"))
            {
                m_simpleMode.boolValue = !m_simpleMode.boolValue;
                serializedObject.ApplyModifiedProperties();
                GUI.changed = false;
            }

            EditorGUILayout.Space();

            if (m_simpleMode.hasMultipleDifferentValues)
            {
                EditorGUILayout.LabelField("Different modes");
            }
            else
            {
                if (m_simpleMode.boolValue)
                {
                    EditorGUILayout.PropertyField(m_simpleMaxParticles, new GUIContent("Max Particles"));
                    m_recreateSolver.boolValue = GUI.changed;

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(m_simpleSubstepCount, new GUIContent("Substep Count"));
                    EditorGUILayout.PropertyField(m_simpleIterationCount, new GUIContent("Iteration Count"));
                    EditorGUILayout.PropertyField(m_simpleGravity, new GUIContent("Global Gravity"));

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Particle");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_simpleParticleSize, new GUIContent("Size"));
                    EditorGUILayout.PropertyField(m_simpleParticleFriction, new GUIContent("Friction"));
                    EditorGUILayout.PropertyField(m_simpleParticleRestitution, new GUIContent("Restitution"));
                    EditorGUILayout.PropertyField(m_simpleParticleAdhesion, new GUIContent("Adhesion"));
                    EditorGUILayout.PropertyField(m_simpleParticleDamping, new GUIContent("Damping"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Cloth");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_simpleClothWind, new GUIContent("Wind"));
                    EditorGUILayout.PropertyField(m_simpleClothDrag, new GUIContent("Drag"));
                    EditorGUILayout.PropertyField(m_simpleClothLift, new GUIContent("Lift"));
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Fluid");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_simpleFluidCohesion, new GUIContent("Cohesion"));
                    EditorGUILayout.PropertyField(m_simpleFluidTension, new GUIContent("Tension"));
                    EditorGUILayout.PropertyField(m_simpleFluidViscosity, new GUIContent("Viscosity"));
                    EditorGUILayout.PropertyField(m_simpleFluidVorticity, new GUIContent("Vorticity"));
                    EditorGUILayout.PropertyField(m_fluidMaterial, new GUIContent("Material"));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_simpleFluidAnisotropy, new GUIContent("Anisotropy"));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_simpleFluidMinScale, new GUIContent("Min Scale"));
                    EditorGUILayout.PropertyField(m_simpleFluidMaxScale, new GUIContent("Max Scale"));
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.LabelField("Definition");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_maxParticles);
                    //EditorGUILayout.PropertyField(m_maxDiffuse);
                    EditorGUILayout.PropertyField(m_maxNeighbors);
                    EditorGUILayout.PropertyField(m_maxContacts);
                    m_recreateSolver.boolValue = GUI.changed;
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Simulation");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_substepCount);
                    EditorGUILayout.PropertyField(m_iterationCount);
                    EditorGUILayout.PropertyField(m_gravity);
                    EditorGUILayout.PropertyField(m_radius);
                    EditorGUILayout.PropertyField(m_solidRest);
                    EditorGUILayout.PropertyField(m_fluidRest);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Common");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_staticFriction);
                    EditorGUILayout.PropertyField(m_dynamicFriction);
                    EditorGUILayout.PropertyField(m_particleFriction);
                    EditorGUILayout.PropertyField(m_restitution);
                    EditorGUILayout.PropertyField(m_adhesion);
                    EditorGUILayout.PropertyField(m_sleepThreshold);
                    EditorGUILayout.PropertyField(m_maxSpeed);
                    EditorGUILayout.PropertyField(m_maxAcceleration);
                    EditorGUILayout.PropertyField(m_shockPropagation);
                    EditorGUILayout.PropertyField(m_dissipation);
                    EditorGUILayout.PropertyField(m_damping);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Cloth");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_wind);
                    EditorGUILayout.PropertyField(m_drag);
                    EditorGUILayout.PropertyField(m_lift);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(m_fluid);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_cohesion);
                    EditorGUILayout.PropertyField(m_surfaceTension);
                    EditorGUILayout.PropertyField(m_viscosity);
                    EditorGUILayout.PropertyField(m_vorticityConfinement);
                    EditorGUILayout.PropertyField(m_anisotropyScale);
                    EditorGUILayout.PropertyField(m_anisotropyMin);
                    EditorGUILayout.PropertyField(m_anisotropyMax);
                    EditorGUILayout.PropertyField(m_smoothing);
                    EditorGUILayout.PropertyField(m_solidPressure);
                    EditorGUILayout.PropertyField(m_freeSurfaceDrag);
                    EditorGUILayout.PropertyField(m_buoyancy);
                    EditorGUI.indentLevel--;

                    //EditorGUILayout.LabelField("Diffuse");

                    //EditorGUI.indentLevel++;
                    //EditorGUILayout.PropertyField(m_diffuseThreshold);
                    //EditorGUILayout.PropertyField(m_diffuseBuoyancy);
                    //EditorGUILayout.PropertyField(m_diffuseDrag);
                    //EditorGUILayout.PropertyField(m_diffuseBallistic);
                    //EditorGUILayout.PropertyField(m_diffuseSortAxis);
                    //EditorGUILayout.PropertyField(m_diffuseLifetime);
                    //EditorGUI.indentLevel--;

                    //EditorGUILayout.LabelField("Rigid");

                    //EditorGUI.indentLevel++;
                    //EditorGUILayout.PropertyField(m_plasticThreshold);
                    //EditorGUILayout.PropertyField(m_plasticCreep);
                    //EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Collision");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_collisionDistance);
                    EditorGUILayout.PropertyField(m_particleCollisionMargin);
                    EditorGUILayout.PropertyField(m_shapeCollisionMargin);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(m_planes.FindPropertyRelative("Array.size"), new GUIContent("Planes", "Collision planes in the form ax + by + cz + d = 0"));
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < m_planes.arraySize; ++i)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Plane " + i, GUILayout.Width(EditorGUIUtility.labelWidth));
                        int indentLevel = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        float labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 13;
                        EditorGUILayout.PropertyField(m_planes.GetArrayElementAtIndex(i).FindPropertyRelative("x"), new GUIContent("A"), GUILayout.MinWidth(18));
                        EditorGUILayout.PropertyField(m_planes.GetArrayElementAtIndex(i).FindPropertyRelative("y"), new GUIContent("B"), GUILayout.MinWidth(18));
                        EditorGUILayout.PropertyField(m_planes.GetArrayElementAtIndex(i).FindPropertyRelative("z"), new GUIContent("C"), GUILayout.MinWidth(18));
                        EditorGUILayout.PropertyField(m_planes.GetArrayElementAtIndex(i).FindPropertyRelative("w"), new GUIContent("D"), GUILayout.MinWidth(18));
                        EditorGUIUtility.labelWidth = labelWidth;
                        EditorGUI.indentLevel = indentLevel;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Relaxation");

                    EditorGUI.indentLevel++;
                    EditorGUILayout.IntPopup(m_relaxationMode, new GUIContent[] { new GUIContent("Global"), new GUIContent("Local") }, new int[] { 0, 1 });
                    EditorGUILayout.PropertyField(m_relaxationFactor);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Fluid Rendering");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_fluidMaterial);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Debug Info");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_showTimers);
                    EditorGUI.indentLevel--;
                }
            }

            if (GUI.changed) serializedObject.ApplyModifiedProperties();
        }
    }
}
