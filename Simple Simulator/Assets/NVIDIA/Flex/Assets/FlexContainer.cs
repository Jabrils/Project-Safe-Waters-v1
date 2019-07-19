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

using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    public class FlexContainer : ScriptableObject
    {
        #region Classes

        public class ParticleData
        {
            public FlexContainer container;
            public FlexExt.ParticleData particleData;

            static Vector4 vec4Buffer = default(Vector4);
            static Vector3 vec3Buffer = default(Vector3);

            public void GetParticles(int _start, int _count, Vector4[] _particles)
            {
                FlexUtils.FastCopy(particleData.particles, _start * 16, ref _particles[0], 0, _count * 16);
            }
            public void SetParticles(int _start, int _count, Vector4[] _particles)
            {
                FlexUtils.FastCopy(ref _particles[0], 0, particleData.particles, _start * 16, _count * 16);
            }
            public Vector4 GetParticle(int _index)
            {
                FlexUtils.FastCopy(particleData.particles, _index * 16, ref vec4Buffer, 0, 16);
                return vec4Buffer;
            }
            public void SetParticle(int _index, Vector4 _particle)
            {
                FlexUtils.FastCopy(ref _particle, 0, particleData.particles, _index * 16, 16);
            }
            public void GetRestParticles(int _start, int _count, Vector4[] _restParticles)
            {
                FlexUtils.FastCopy(particleData.restParticles, _start * 16, ref _restParticles[0], 0, _count * 16);
            }
            public void SetRestParticles(int _start, int _count, Vector4[] _restParticles)
            {
                FlexUtils.FastCopy(ref _restParticles[0], 0, particleData.restParticles, _start * 16, _count * 16);
            }
            public Vector4 GetRestParticle(int _index)
            {
                FlexUtils.FastCopy(particleData.restParticles, _index * 16, ref vec4Buffer, 0, 16);
                return vec4Buffer;
            }
            public void SetRestParticle(int _index, Vector4 _restParticles)
            {
                FlexUtils.FastCopy(ref _restParticles, 0, particleData.restParticles, _index * 16, 16);
            }
            public void GetVelocities(int _start, int _count, Vector3[] _velocities)
            {
                FlexUtils.FastCopy(particleData.velocities, _start * 12, ref _velocities[0], 0, _count * 12);
            }
            public void SetVelocities(int _start, int _count, Vector3[] _velocities)
            {
                FlexUtils.FastCopy(ref _velocities[0], 0, particleData.velocities, _start * 12, _count * 12);
            }
            public Vector3 GetVelocity(int _index)
            {
                FlexUtils.FastCopy(particleData.velocities, _index * 12, ref vec3Buffer, 0, 12);
                return vec3Buffer;
            }
            public void SetVelocity(int _index, Vector3 _velocity)
            {
                FlexUtils.FastCopy(ref _velocity, 0, particleData.velocities, _index * 12, 12);
            }
            public void GetPhases(int _start, int _count, int[] _phases)
            {
                FlexUtils.FastCopy(particleData.phases, _start * 4, ref _phases[0], 0, _count * 4);
            }
            public void SetPhases(int _start, int _count, int[] _phases)
            {
                FlexUtils.FastCopy(ref _phases[0], 0, particleData.phases, _start * 4, _count * 4);
            }
            public int GetPhase(int _index)
            {
                int phase = 0;
                FlexUtils.FastCopy(particleData.phases, _index * 4, ref phase, 0, 4);
                return phase;
            }
            public void SetPhase(int _index, int _phase)
            {
                FlexUtils.FastCopy(ref _phase, 0, particleData.phases, _index * 4, 4);
            }
            public void GetNormals(int _start, int _count, Vector4[] _normals)
            {
                FlexUtils.FastCopy(particleData.normals, _start * 16, ref _normals[0], 0, _count * 16);
            }
            public void SetNormals(int _start, int _count, Vector4[] _normals)
            {
                FlexUtils.FastCopy(ref _normals[0], 0, particleData.normals, _start * 16, _count * 16);
            }
            public Vector4 GetNormal(int _index)
            {
                FlexUtils.FastCopy(particleData.normals, _index * 16, ref vec4Buffer, 0, 16);
                return vec4Buffer;
            }
            public void SetNormal(int _index, Vector4 _normal)
            {
                FlexUtils.FastCopy(ref _normal, 0, particleData.normals, _index * 16, 16);
            }
            public int PickParticle(Vector3 _origin, Vector3 _direction)
            {
                return FlexUtils.PickParticle(ref _origin, ref _direction, particleData.particles, particleData.phases, container.maxParticles, container.solidRest * 0.5f);
            }
        }

        #endregion

        #region Properties

        public static Flex.Library library { get { return sm_libraryHandle; } }

        public FlexExt.Container handle { get { return m_containerHandle; } }

        public Flex.Solver solver { get { return m_solverHandle; } }

        public int substepCount { get { return m_substepCount; } set { m_substepCount = Mathf.Max(value, 1); } }

        public int iterationCount { get { return m_iterationCount; } set { m_iterationCount = Mathf.Max(value, 1); } }

        public float radius { get { return m_radius; } set { m_radius = Mathf.Max(value, 0.001f); } }

        public float solidRest { get { return m_solidRest; } set { m_solidRest = Mathf.Max(value, 0.001f); } }

        public float fluidRest { get { return m_fluidRest; } set { m_fluidRest = Mathf.Max(value, 0.001f); } }

        public float staticFriction { get { return m_staticFriction; } set { m_staticFriction = Mathf.Max(value, 0.0f); } }

        public float dynamicFriction { get { return m_dynamicFriction; } set { m_dynamicFriction = Mathf.Clamp(value, 0.0f, m_staticFriction); } }

        public float particleFriction { get { return m_particleFriction; } set { m_particleFriction = Mathf.Max(value, 0.0f); } }

        public float restitution { get { return m_restitution; } set { m_restitution = Mathf.Clamp01(value); } }

        public float adhesion { get { return m_adhesion; } set { m_adhesion = Mathf.Max(value, 0.0f); } }

        public float sleepThreshold { get { return m_sleepThreshold; } set { m_sleepThreshold = Mathf.Max(value, 0.0f); } }

        public float maxSpeed { get { return m_adhesion; } set { m_maxSpeed = Mathf.Max(value, 0.0f); } }

        public float maxAcceleration { get { return m_maxAcceleration; } set { m_maxAcceleration = Mathf.Max(value, 0.0f); } }

        public float shockPropagation { get { return m_shockPropagation; } set { m_shockPropagation = Mathf.Max(value, 0.0f); } }

        public float dissipation { get { return m_dissipation; } set { m_dissipation = Mathf.Max(value, 0.0f); } }

        public float damping { get { return m_damping; } set { m_damping = Mathf.Max(value, 0.0f); } }

        public float drag { get { return m_drag; } set { m_drag = Mathf.Max(value, 0.0f); } }

        public float lift { get { return m_lift; } set { m_lift = Mathf.Max(value, 0.0f); } }

        public float cohesion { get { return m_cohesion; } set { m_cohesion = Mathf.Max(value, 0.0f); } }

        public float surfaceTension { get { return m_surfaceTension; } set { m_surfaceTension = Mathf.Max(value, 0.0f); } }

        public float viscosity { get { return m_viscosity; } set { m_viscosity = Mathf.Max(value, 0.0f); } }

        public float vorticityConfinement { get { return m_vorticityConfinement; } set { m_vorticityConfinement = Mathf.Max(value, 0.0f); } }

        public float anisotropyScale { get { return m_anisotropyScale; } set { m_anisotropyScale = Mathf.Max(value, 0.0f); } }

        public float anisotropyMin { get { return m_anisotropyMin; } set { m_anisotropyMin = Mathf.Clamp01(value); } }

        public float anisotropyMax { get { return m_anisotropyMax; } set { m_anisotropyMax = Mathf.Clamp01(value); } }

        public float smoothing { get { return m_smoothing; } set { m_smoothing = Mathf.Max(value, 0.0f); } }

        public float solidPressure { get { return m_solidPressure; } set { m_solidPressure = Mathf.Max(value, 0.0f); } }

        public float freeSurfaceDrag { get { return m_freeSurfaceDrag; } set { m_freeSurfaceDrag = Mathf.Max(value, 0.0f); } }

        public float buoyancy { get { return m_buoyancy; } set { m_buoyancy = Mathf.Clamp(value, -10.0f, 10.0f); } }

        public float collisionDistance { get { return m_collisionDistance; } set { m_collisionDistance = Mathf.Max(value, 0.0f); } }

        public float particleCollisionMargin { get { return m_particleCollisionMargin; } set { m_particleCollisionMargin = Mathf.Max(value, 0.0f); } }

        public float shapeCollisionMargin { get { return m_shapeCollisionMargin; } set { m_shapeCollisionMargin = Mathf.Max(value, 0.0f); } }

        public float relaxationFactor { get { return m_relaxationFactor; } set { m_relaxationFactor = Mathf.Max(value, 0.0f); } }

        public ComputeBuffer particleBuffer { get { return m_particleBuffer; } }

        public int[] fluidIndices { get { return m_fluidIndices; } }
        public int fluidIndexCount { get { return m_fluidIndexCount; } }

        public int maxParticles { get { return m_maxParticles; } }

        public Material fluidMaterial { get { return m_fluidMaterial; } }

        #endregion

        #region Events

        public delegate void OnFlexUpdateFn(ParticleData _particleData);
        public event OnFlexUpdateFn onFlexUpdate;

        public delegate void OnBeforeRecreateFn();
        public event OnBeforeRecreateFn onBeforeRecreate;

        public delegate void OnAfterRecreateFn();
        public event OnAfterRecreateFn onAfterRecreate;

        public delegate void OnBeforeDestroyFn();
        public event OnBeforeDestroyFn onBeforeDestroy;

        #endregion

        #region Methods

        public void AddActor(FlexActor actor)
        {
            if (m_actorCount == 0) CreateContainer();
            ++m_actorCount;
        }

        public void RemoveActor(FlexActor actor)
        {
            --m_actorCount;
            if (m_actorCount == 0) DestroyContainer();
        }

        public FlexExt.Instance.Handle CreateInstance(FlexExt.Asset.Handle _assetHandle, Matrix4x4 _location, Vector3 _velocity, int _phase, float _massScale)
        {
            FlexExt.ParticleData particleData = FlexExt.MapParticleData(m_containerHandle);

            FlexExt.Instance.Handle instanceHandle = FlexExt.CreateInstance(m_containerHandle, ref particleData, _assetHandle, ref _location, _velocity.x, _velocity.y, _velocity.z, _phase, 1.0f / _massScale);
            UpdateBuffer(particleData);

            FlexExt.UnmapParticleData(m_containerHandle);

            return instanceHandle;
        }

        public void DestroyInstance(FlexExt.Instance.Handle _instanceHandle)
        {
            FlexExt.DestroyInstance(m_containerHandle, _instanceHandle);
        }

        public int[] AllocParticles(int _count)
        {
            int[] indices = new int[_count];
            int count = FlexExt.AllocParticles(m_containerHandle, _count, ref indices[0]);
            if (count < _count) Array.Resize(ref indices, count);
            return indices;
        }

        public void FreeParticles(int[] _indices)
        {
            FlexExt.FreeParticles(m_containerHandle, _indices.Length, ref _indices[0]);
        }

        public int AllocParticles(int[] _indices, int _offset, int _count)
        {
            return FlexExt.AllocParticles(m_containerHandle, _count, ref _indices[_offset]);
        }

        public void FreeParticles(int[] _indices, int _offset, int _count)
        {
            FlexExt.FreeParticles(m_containerHandle, _count, ref _indices[_offset]);
        }

        public void AddFluidIndices(int[] _fluidIndices, int _indexCount)
        {
            if (m_fluidIndexCount + _indexCount > m_fluidIndices.Length)
                Array.Resize(ref m_fluidIndices, m_fluidIndexCount + _indexCount);

            Array.Copy(_fluidIndices, 0, m_fluidIndices, m_fluidIndexCount, _indexCount);
            m_fluidIndexCount += _indexCount;
        }

        #endregion

        #region Messages

        void OnDestroy()
        {
            if (m_actorCount > 0)
            {
                onBeforeDestroy();
                if (m_actorCount > 0) Debug.LogError("Something wasn't destroyed");
                DestroyContainer();
            }

            while (m_destroyObjects.Count > 0)
            {
                DestroyImmediate(m_destroyObjects[0]);
                m_destroyObjects.RemoveAt(0);
            }
        }

        void OnValidate()
        {
            if (!m_containerHandle) return;

            if (m_simpleMode)
            {
                m_simpleMaxParticles = Mathf.Max(m_simpleMaxParticles, 1000);
                m_maxParticles = m_simpleMaxParticles;
                m_maxDiffuse = 0;
                m_maxNeighbors = 200;
                m_maxContacts = 6;

                if (m_recreateSolver)
                {
                    if (m_actorCount > 0) onBeforeRecreate();
                    DestroyContainer();
                    CreateContainer();
                    if (m_actorCount > 0) onAfterRecreate();
                    m_recreateSolver = false;
                }

                m_simpleSubstepCount = Mathf.Max(m_simpleSubstepCount, 1);
                m_substepCount = m_simpleSubstepCount;

                m_simpleIterationCount = Math.Max(m_simpleIterationCount, 1);
                m_iterationCount = m_simpleIterationCount;

                m_gravity = m_simpleGravity;

                m_simpleParticleSize = Mathf.Max(m_simpleParticleSize, 0.001f);
                m_radius = m_simpleParticleSize * 1.5f;
                m_solidRest = m_simpleParticleSize;
                m_fluidRest = m_simpleParticleSize * 0.95f;

                m_simpleParticleFriction = Mathf.Max(m_simpleParticleFriction, 0.0f);
                m_staticFriction = m_simpleParticleFriction;
                m_dynamicFriction = m_simpleParticleFriction;

                m_simpleParticleRestitution = Mathf.Clamp01(m_simpleParticleRestitution);
                m_restitution = m_simpleParticleRestitution;

                m_simpleParticleAdhesion = Mathf.Max(m_simpleParticleAdhesion, 0.0f);
                m_adhesion = m_simpleParticleAdhesion;

                m_sleepThreshold = 0.0f;
                m_maxSpeed = float.MaxValue;
                m_maxAcceleration = float.MaxValue;
                m_shockPropagation = 0.0f;
                m_dissipation = 0.0f;

                m_simpleParticleDamping = Mathf.Max(m_simpleParticleDamping, 0.0f);
                m_damping = m_simpleParticleDamping;

                m_wind = m_simpleClothWind;

                m_simpleClothDrag = Mathf.Max(m_simpleClothDrag, 0.0f);
                m_drag = m_simpleClothDrag;

                m_simpleClothLift = Mathf.Max(m_simpleClothLift, 0.0f);
                m_lift = m_simpleClothLift;

                m_fluid = true;

                m_simpleFluidCohesion = Mathf.Max(m_simpleFluidCohesion, 0.0f);
                m_cohesion = m_simpleFluidCohesion;

                m_simpleFluidTension = Mathf.Max(m_simpleFluidTension, 0.0f);
                m_surfaceTension = m_simpleFluidTension;

                m_simpleFluidViscosity = Mathf.Max(m_simpleFluidViscosity, 0.0f);
                m_viscosity = m_simpleFluidViscosity;

                m_simpleFluidVorticity = Mathf.Max(m_simpleFluidVorticity, 0.0f);
                m_vorticityConfinement = m_simpleFluidVorticity;

                m_simpleFluidAnisotropy = Mathf.Max(m_simpleFluidAnisotropy, 0.0f);
                m_anisotropyScale = m_simpleFluidAnisotropy;

                m_simpleFluidMinScale = Mathf.Clamp01(m_simpleFluidMinScale);
                m_anisotropyMin = m_simpleFluidMinScale;

                m_simpleFluidMaxScale = Mathf.Clamp01(m_simpleFluidMaxScale);
                m_anisotropyMax = m_simpleFluidMaxScale;

                m_smoothing = 0.0f;
                m_solidPressure = 0.0f;
                m_freeSurfaceDrag = 0.0f;
                m_buoyancy = 1.0f;

                m_collisionDistance = m_simpleParticleSize * 0.5f;
                m_particleCollisionMargin = 0.0f;
                m_shapeCollisionMargin = 0.0f;
                m_relaxationMode = Flex.RelaxationMode.Local;
                m_relaxationFactor = 1.0f;
            }

            m_maxParticles = Mathf.Max(m_maxParticles, 1000);
            m_maxDiffuse = Mathf.Max(m_maxDiffuse, 0);
            m_maxNeighbors = Mathf.Max(m_maxNeighbors, 0);
            m_maxContacts = Mathf.Max(m_maxContacts, 0);

            if (m_recreateSolver)
            {
                if (m_actorCount > 0) onBeforeRecreate();
                DestroyContainer();
                CreateContainer();
                if (m_actorCount > 0) onAfterRecreate();
                m_recreateSolver = false;
            }

            m_substepCount = Mathf.Max(m_substepCount, 1);
            m_iterationCount = Mathf.Max(m_iterationCount, 1);
            m_radius = Mathf.Max(m_radius, 0.001f);
            m_solidRest = Mathf.Max(m_solidRest, 0.001f);
            m_fluidRest = Mathf.Max(m_fluidRest, 0.001f);
            m_staticFriction = Mathf.Max(m_staticFriction, 0.0f);
            m_dynamicFriction = Mathf.Clamp(m_dynamicFriction, 0.0f, m_staticFriction);
            m_particleFriction = Mathf.Max(m_particleFriction, 0.0f);
            m_restitution = Mathf.Clamp01(m_restitution);
            m_adhesion = Mathf.Max(m_adhesion, 0.0f);
            m_sleepThreshold = Mathf.Max(m_sleepThreshold, 0.0f);
            m_maxSpeed = Mathf.Max(m_maxSpeed, 0.0f);
            m_maxAcceleration = Mathf.Max(m_maxAcceleration, 0.0f);
            m_shockPropagation = Mathf.Max(m_shockPropagation, 0.0f);
            m_dissipation = Mathf.Max(m_dissipation, 0.0f);
            m_damping = Mathf.Max(m_damping, 0.0f);
            m_drag = Mathf.Max(m_drag, 0.0f);
            m_lift = Mathf.Max(m_lift, 0.0f);
            m_cohesion = Mathf.Max(m_cohesion, 0.0f);
            m_surfaceTension = Mathf.Max(m_surfaceTension, 0.0f);
            m_viscosity = Mathf.Max(m_viscosity, 0.0f);
            m_vorticityConfinement = Mathf.Max(m_vorticityConfinement, 0.0f);
            m_anisotropyScale = Mathf.Max(m_anisotropyScale, 0.0f);
            m_anisotropyMin = Mathf.Clamp01(m_anisotropyMin);
            m_anisotropyMax = Mathf.Clamp01(m_anisotropyMax);
            m_smoothing = Mathf.Max(m_smoothing, 0.0f);
            m_solidPressure = Mathf.Max(m_solidPressure, 0.0f);
            m_freeSurfaceDrag = Mathf.Max(m_freeSurfaceDrag, 0.0f);
            m_buoyancy = Mathf.Clamp(m_buoyancy, -10.0f, 10.0f);
            m_diffuseThreshold = Mathf.Max(m_diffuseThreshold, 0.0f);
            m_diffuseBuoyancy = Mathf.Max(m_diffuseBuoyancy, 0.0f);
            m_diffuseDrag = Mathf.Max(m_diffuseDrag, 0.0f);
            m_diffuseBallistic = Mathf.Max(m_diffuseBallistic, 0);
            m_diffuseLifetime = Mathf.Max(m_diffuseLifetime, 0.0f);
            m_plasticThreshold = Mathf.Max(m_plasticThreshold, 0.0f);
            m_plasticCreep = Mathf.Max(m_plasticCreep, 0.0f);
            m_collisionDistance = Mathf.Max(m_collisionDistance, 0.0f);
            m_particleCollisionMargin = Mathf.Max(m_particleCollisionMargin, 0.0f);
            m_shapeCollisionMargin = Mathf.Max(m_shapeCollisionMargin, 0.0f);
            m_relaxationFactor = Mathf.Max(m_relaxationFactor, 0.0f);
        }

        void Reset()
        {
            m_fluidMaterial = Resources.Load<Material>("Materials/FlexDrawFluid");
        }

        #endregion

        #region Private

        [MonoPInvokeCallback(typeof(Flex.ErrorCallback))]
        static void ErrorCallback(Flex.ErrorSeverity type, IntPtr msg, IntPtr file, int line)
        {
            Debug.LogError("" + type + " - " + Marshal.PtrToStringAnsi(msg) + "\nFile - " + Marshal.PtrToStringAnsi(file) + " (" + line + ")");
        }

        static void AcquireLibrary()
        {
            if (sm_libraryRefCount++ == 0)
            {
#if FLEX_CUDA
                Flex.InitDesc desc = new Flex.InitDesc { computeType = Flex.ComputeType.CUDA, enableExtensions = false };
#else
                Flex.InitDesc desc = new Flex.InitDesc { computeType = Flex.ComputeType.D3D11, enableExtensions = false };
#endif
                sm_libraryHandle = Flex.Init(Flex.FLEX_VERSION, ErrorCallback, ref desc);
            }
        }

        static void ReleaseLibrary()
        {
            if (--sm_libraryRefCount == 0)
            {
                if (sm_libraryHandle) Flex.Shutdown(sm_libraryHandle);
                sm_libraryHandle.Clear();
            }
        }

        void CreateContainer()
        {
            AcquireLibrary();

            if (sm_libraryHandle)
            {
                Flex.SolverDesc desc = default(Flex.SolverDesc);
                Flex.SetSolverDescDefaults(ref desc);
                desc.maxParticles = m_maxParticles;
                desc.maxDiffuseParticles = m_maxDiffuse;
                desc.maxNeighborsPerParticle = m_maxNeighbors;
                desc.maxContactsPerParticle = m_maxContacts;
                m_solverHandle = Flex.CreateSolver(sm_libraryHandle, ref desc);
            }

            if (m_solverHandle)
            {
                m_containerHandle = FlexExt.CreateContainer(sm_libraryHandle, m_solverHandle, m_maxParticles);
            }

            if (m_containerHandle)
            {
                m_particleBuffer = new ComputeBuffer(m_maxParticles, 16);

                m_flexScene = new GameObject("FlexScene").AddComponent<FlexScene>();
                m_flexScene.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                m_flexScene.container = this;
                m_flexScene.fixedUpdate += FixedUpdate;
                m_flexScene.update += Update;
                m_flexScene.onGUI += OnGUI;
                GameObject detectShapesObject = new GameObject("FlexDetectShapes");
                detectShapesObject.hideFlags = HideFlags.DontSave;
                detectShapesObject.transform.parent = m_flexScene.transform;
                detectShapesObject.transform.position = Vector3.up * float.MaxValue; // @@@
                detectShapesObject.transform.rotation = Quaternion.identity;
                m_detectShapesHelper = detectShapesObject.AddComponent<_auxFlexDetectShapes>();
            }
        }

        void DestroyContainer()
        {
            if (m_containerHandle)
            {
                FlexExt.DestroyContainer(m_containerHandle);
                m_containerHandle.Clear();
            }

            if (m_solverHandle)
            {
                Flex.DestroySolver(m_solverHandle);
                m_solverHandle.Clear();
            }

            if (m_drawFluidHelper)
            {
                //DestroyImmediate(m_drawFluidHelper.gameObject);
                m_drawFluidHelper.enabled = false;
                m_drawFluidHelper.gameObject.SetActive(false);
                m_destroyObjects.Add(m_drawFluidHelper.gameObject);
                m_drawFluidHelper = null;
            }

            if (m_detectShapesHelper)
            {
                //DestroyImmediate(m_detectShapesHelper.gameObject);
                m_detectShapesHelper.gameObject.SetActive(false);
                m_destroyObjects.Add(m_detectShapesHelper.gameObject);
                m_detectShapesHelper = null;
            }

            if (m_flexScene)
            {
                m_flexScene.fixedUpdate -= FixedUpdate;
                m_flexScene.update -= Update;
                m_flexScene.onGUI -= OnGUI;
                m_flexScene.gameObject.SetActive(false);
                m_destroyObjects.Add(m_flexScene.gameObject);
                m_flexScene = null;
            }

            if (m_particleBuffer != null) m_particleBuffer.Release();
            m_particleBuffer = null;

            ReleaseLibrary();
        }

        void FixedUpdate()
        {
            UpdateParams();
            UpdateSolver();
        }

        void UpdateParams()
        {
            Flex.Params prms = new Flex.Params();
            Flex.GetParams(m_solverHandle, ref prms);
            prms.numIterations = m_iterationCount;
            prms.gravity = m_gravity;
            prms.radius = m_radius;
            prms.solidRestDistance = m_solidRest;
            prms.fluidRestDistance = m_fluidRest;
            prms.staticFriction = m_staticFriction;
            prms.dynamicFriction = m_dynamicFriction;
            prms.particleFriction = m_particleFriction;
            prms.restitution = m_restitution;
            prms.adhesion = m_adhesion;
            prms.sleepThreshold = m_sleepThreshold;
            prms.maxSpeed = m_maxSpeed;
            prms.maxAcceleration = m_maxAcceleration;
            prms.shockPropagation = m_shockPropagation;
            prms.dissipation = m_dissipation;
            prms.damping = m_damping;
            prms.wind = m_wind;
            prms.drag = m_drag;
            prms.lift = m_lift;
            //prms.fluid = m_fluid;
            prms.cohesion = m_cohesion;
            prms.surfaceTension = m_surfaceTension;
            prms.viscosity = m_viscosity;
            prms.vorticityConfinement = m_vorticityConfinement;
            prms.anisotropyScale = m_anisotropyScale;
            prms.anisotropyMin = m_anisotropyMin;
            prms.anisotropyMax = m_anisotropyMax;
            prms.smoothing = m_smoothing;
            prms.solidPressure = m_solidPressure;
            prms.freeSurfaceDrag = m_freeSurfaceDrag;
            prms.buoyancy = m_buoyancy;
            prms.diffuseThreshold = m_diffuseThreshold;
            prms.diffuseBuoyancy = m_diffuseBuoyancy;
            prms.diffuseDrag = m_diffuseDrag;
            prms.diffuseBallistic = m_diffuseBallistic;
            //prms.diffuseSortAxis = m_diffuseSortAxis;
            prms.diffuseLifetime = m_diffuseLifetime;
            //prms.plasticThreshold = m_plasticThreshold;
            //prms.plasticCreep = m_plasticCreep;
            prms.collisionDistance = m_collisionDistance;
            prms.particleCollisionMargin = m_particleCollisionMargin;
            prms.shapeCollisionMargin = m_shapeCollisionMargin;
            prms.numPlanes = m_planes.Length;
            prms.plane0 = m_planes.Length > 0 ? m_planes[0] : Vector4.zero;
            prms.plane1 = m_planes.Length > 1 ? m_planes[1] : Vector4.zero;
            prms.plane2 = m_planes.Length > 2 ? m_planes[2] : Vector4.zero;
            prms.plane3 = m_planes.Length > 3 ? m_planes[3] : Vector4.zero;
            prms.plane4 = m_planes.Length > 4 ? m_planes[4] : Vector4.zero;
            prms.plane5 = m_planes.Length > 5 ? m_planes[5] : Vector4.zero;
            prms.plane6 = m_planes.Length > 6 ? m_planes[6] : Vector4.zero;
            prms.plane7 = m_planes.Length > 7 ? m_planes[7] : Vector4.zero;
            prms.relaxationMode = m_relaxationMode;
            prms.relaxationFactor = m_relaxationFactor;
            Flex.SetParams(m_solverHandle, ref prms);
        }

        void UpdateSolverEditor()
        {
            FlexExt.TickContainer(m_containerHandle, Time.fixedDeltaTime, 0);

            ParticleData particleData = new ParticleData();
            particleData.container = this;
            particleData.particleData = FlexExt.MapParticleData(m_containerHandle);
            UpdateBuffer(particleData.particleData);

            m_fluidIndexCount = 0;
            if (onFlexUpdate != null) onFlexUpdate(particleData);
            UpdateDrawFluid(particleData);

            FlexExt.UnmapParticleData(m_containerHandle);
        }

        ParticleData m_particleData = new ParticleData();
        void UpdateSolver()
        {
            m_particleData.container = this;
            m_particleData.particleData = FlexExt.MapParticleData(m_containerHandle);
            UpdateBuffer(m_particleData.particleData);

#if UNITY_EDITOR
            if (m_showTimers) Flex.GetTimers(m_solverHandle, ref m_timers);
#endif
            FlexExt.UpdateInstances(m_containerHandle);

            UpdateDrawFluid(m_particleData);

            UpdateDetectShapes(m_particleData);

            m_fluidIndexCount = 0;
            if (onFlexUpdate != null) onFlexUpdate(m_particleData);

            FlexExt.UnmapParticleData(m_containerHandle);

#if UNITY_EDITOR
            FlexExt.TickContainer(m_containerHandle, Time.fixedDeltaTime, m_substepCount, m_showTimers);
#else
            FlexExt.TickContainer(m_containerHandle, Time.fixedDeltaTime, m_substepCount);
#endif
        }

        public void UpdateBuffer(FlexExt.ParticleData _particleData)
        {
            if (m_particleArray == null || m_particleArray.Length != m_maxParticles)
                m_particleArray = new Vector4[m_maxParticles];
            FlexUtils.FastCopy(_particleData.particles, m_particleArray);
            m_particleBuffer.SetData(m_particleArray);
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                UpdateParams();
                UpdateSolverEditor();
            }

            while (m_destroyObjects.Count > 0)
            {
                DestroyImmediate(m_destroyObjects[0]);
                m_destroyObjects.RemoveAt(0);
            }
        }

        void OnGUI()
        {
#if UNITY_EDITOR
            if (m_showTimers)
            {
                for (int i = 0; i < 2; ++i)
                {
                    float width = 185, height = 20, spacing = 15;
                    Rect rect = i == 0 ? new Rect(Screen.width - width + 1, 1, width, height) : new Rect(Screen.width - width, 0, width, height);
                    GUI.color = i == 0 ? Color.grey : Color.white;
                    GUI.Label(rect, string.Format("Predict             \t{0:F1} ms", m_timers.predict)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CreateCellIndices   \t{0:F1} ms", m_timers.createCellIndices)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SortCellIndices     \t{0:F1} ms", m_timers.sortCellIndices)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CreateGrid          \t{0:F1} ms", m_timers.createGrid)); rect.y += spacing;
                    GUI.Label(rect, string.Format("Reorder             \t{0:F1} ms", m_timers.reorder)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CollideParticles    \t{0:F1} ms", m_timers.collideParticles)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CollideShapes       \t{0:F1} ms", m_timers.collideShapes)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CollideTriangles    \t{0:F1} ms", m_timers.collideTriangles)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CollideFields       \t{0:F1} ms", m_timers.collideFields)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CalculateDensity    \t{0:F1} ms", m_timers.calculateDensity)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SolveDensities      \t{0:F1} ms", m_timers.solveDensities)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SolveVelocities     \t{0:F1} ms", m_timers.solveVelocities)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SolveShapes         \t{0:F1} ms", m_timers.solveShapes)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SolveSprings        \t{0:F1} ms", m_timers.solveSprings)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SolveContacts       \t{0:F1} ms", m_timers.solveContacts)); rect.y += spacing;
                    GUI.Label(rect, string.Format("SolveInflatables    \t{0:F1} ms", m_timers.solveInflatables)); rect.y += spacing;
                    GUI.Label(rect, string.Format("CalculateAnisotropy \t{0:F1} ms", m_timers.calculateAnisotropy)); rect.y += spacing;
                    GUI.Label(rect, string.Format("UpdateDiffuse       \t{0:F1} ms", m_timers.updateDiffuse)); rect.y += spacing;
                    GUI.Label(rect, string.Format("UpdateTriangles     \t{0:F1} ms", m_timers.updateTriangles)); rect.y += spacing;
                    GUI.Label(rect, string.Format("UpdateNormals       \t{0:F1} ms", m_timers.updateNormals)); rect.y += spacing;
                    GUI.Label(rect, string.Format("Finalize            \t{0:F1} ms", m_timers.finalize)); rect.y += spacing;
                    GUI.Label(rect, string.Format("UpdateBounds        \t{0:F1} ms", m_timers.updateBounds)); rect.y += spacing;
                    GUI.Label(rect, string.Format("Total               \t{0:F1} ms", m_timers.total)); rect.y += spacing;
                }
            }
#endif
        }

        void UpdateDrawFluid(ParticleData _particleData)
        {
            if (!m_fluid && m_drawFluidHelper)
            {
                DestroyImmediate(m_drawFluidHelper.gameObject);
                m_drawFluidHelper = null;
            }
            if (m_fluid && !m_drawFluidHelper)
            {
                GameObject drawFluidObject = new GameObject("FlexDrawFluid");
                drawFluidObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                drawFluidObject.transform.parent = m_flexScene.transform;
                m_drawFluidHelper = drawFluidObject.AddComponent<_auxFlexDrawFluid>();
            }
            if (m_drawFluidHelper)
                m_drawFluidHelper.UpdateMesh(_particleData);
        }

        void UpdateDetectShapes(ParticleData _particleData)
        {
            if (m_detectShapesHelper)
            {
                m_detectShapesHelper.UpdateShapes(_particleData);
            }
        }

        static Flex.Library sm_libraryHandle;
        static int sm_libraryRefCount = 0;

        Flex.Solver m_solverHandle;
        FlexExt.Container m_containerHandle;

        FlexScene m_flexScene = null;

        [NonSerialized]
        int m_actorCount = 0;

#if UNITY_EDITOR
        [NonSerialized]
        Flex.Timers m_timers = new Flex.Timers();
#endif

        [NonSerialized]
        int[] m_fluidIndices = new int[0];
        [NonSerialized]
        int m_fluidIndexCount = 0;
        [NonSerialized]
        _auxFlexDrawFluid m_drawFluidHelper = null;

        [NonSerialized]
        _auxFlexDetectShapes m_detectShapesHelper = null;

        Vector4[] m_particleArray = null;
        ComputeBuffer m_particleBuffer;

        List<GameObject> m_destroyObjects = new List<GameObject>();

        [SerializeField]
        bool m_simpleMode = true;

        // Simple

        [SerializeField]
        int m_simpleMaxParticles = 10000;
        [SerializeField]
        int m_simpleSubstepCount = 2;
        [SerializeField]
        int m_simpleIterationCount = 5;
        [SerializeField]
        Vector3 m_simpleGravity = Physics.gravity;
        [SerializeField]
        float m_simpleParticleSize = 0.1f;
        [SerializeField]
        float m_simpleParticleFriction = 0.1f;
        [SerializeField]
        float m_simpleParticleRestitution = 0.0f;
        [SerializeField]
        float m_simpleParticleAdhesion = 0.0f;
        [SerializeField]
        float m_simpleParticleDamping = 0.0f;
        [SerializeField]
        Vector3 m_simpleClothWind = Vector3.zero;
        [SerializeField]
        float m_simpleClothDrag = 0.0f;
        [SerializeField]
        float m_simpleClothLift = 0.0f;
        [SerializeField]
        float m_simpleFluidCohesion = 0.025f;
        [SerializeField]
        float m_simpleFluidTension = 0.01f;
        [SerializeField]
        float m_simpleFluidViscosity = 0.0f;
        [SerializeField]
        float m_simpleFluidVorticity = 0.0f;
        [SerializeField]
        float m_simpleFluidAnisotropy = 2.0f;
        [SerializeField]
        float m_simpleFluidMinScale = 0.2f;
        [SerializeField]
        float m_simpleFluidMaxScale = 1.0f;

        // Advanced

        [SerializeField, Tooltip("Maximum number of simulation particles possible for this solver")]
        int m_maxParticles = 10000;
        [SerializeField, Tooltip("Maximum number of diffuse (non-simulation) particles possible for this solver")]
        int m_maxDiffuse = 0;
        [SerializeField, Tooltip("Maximum number of neighbors per particle possible for this solver")]
        int m_maxNeighbors = 100;
        [SerializeField, Tooltip("Maximum number of collision contacts per particle possible for this solver")]
        int m_maxContacts = 6;
        [SerializeField]
        bool m_recreateSolver = false;
        [SerializeField, Tooltip("The time dt will be divided into the number of sub-steps given by this parameter")]
        int m_substepCount = 3;
        [SerializeField, Tooltip("Number of solver iterations to perform per-substep")]
        int m_iterationCount = 5;
        [SerializeField, Tooltip("Constant acceleration applied to all particles")]
        Vector3 m_gravity = Physics.gravity;
        [SerializeField, Tooltip("The maximum interaction radius for particles")]
        float m_radius = 0.2f;
        [SerializeField, Tooltip("The distance non-fluid particles attempt to maintain from each other, must be in the range (0, radius]")]
        float m_solidRest = 0.1f;
        [SerializeField, Tooltip("The distance fluid particles are spaced at the rest density, must be in the range (0, radius], for fluids this should generally be 50-70% of mRadius, for rigids this can simply be the same as the particle radius")]
        float m_fluidRest = 0.1f;
        [SerializeField, Tooltip("Coefficient of static friction used when colliding against shapes")]
        float m_staticFriction = 0.3f;
        [SerializeField, Tooltip("Coefficient of dynamic friction used when colliding against shapes")]
        float m_dynamicFriction = 0.2f;
        [SerializeField, Tooltip("Coefficient of friction used when colliding particles")]
        float m_particleFriction = 0.1f;
        [SerializeField, Tooltip("Coefficient of restitution used when colliding against shapes, particle collisions are always inelastic")]
        float m_restitution = 0.0f;
        [SerializeField, Tooltip("Controls how strongly particles stick to surfaces they hit, default 0.0, range [0.0, +inf]")]
        float m_adhesion = 0.0f;
        [SerializeField, Tooltip("Particles with a velocity magnitude < this threshold will be considered fixed")]
        float m_sleepThreshold = 0.0f;
        [SerializeField, Tooltip("The magnitude of particle velocity will be clamped to this value at the end of each step")]
        float m_maxSpeed = float.MaxValue;
        [SerializeField, Tooltip("The magnitude of particle acceleration will be clamped to this value at the end of each step (limits max velocity change per-second), useful to avoid popping due to large interpenetrations")]
        float m_maxAcceleration = float.MaxValue;
        [SerializeField, Tooltip("Artificially decrease the mass of particles based on height from a fixed reference point, this makes stacks and piles converge faster")]
        float m_shockPropagation = 0;
        [SerializeField, Tooltip("Damps particle velocity based on how many particle contacts it has")]
        float m_dissipation = 0;
        [SerializeField, Tooltip("Viscous drag force, applies a force proportional, and opposite to the particle velocity")]
        float m_damping = 0;
        [SerializeField, Tooltip("Constant acceleration applied to particles that belong to dynamic triangles, drag needs to be > 0 for wind to affect triangles")]
        Vector3 m_wind = Vector3.zero;
        [SerializeField, Tooltip("Drag force applied to particles belonging to dynamic triangles, proportional to velocity^2*area in the negative velocity direction")]
        float m_drag = 0;
        [SerializeField, Tooltip("Lift force applied to particles belonging to dynamic triangles, proportional to velocity^2*area in the direction perpendicular to velocity and (if possible), parallel to the plane normal")]
        float m_lift = 0;
        [SerializeField, Tooltip("If true then particles with phase 0 are considered fluid particles and interact using the position based fluids method")]
        bool m_fluid = true;
        [SerializeField, Tooltip("Control how strongly particles hold each other together, default: 0.025, range [0.0, +inf]")]
        float m_cohesion = 0.025f;
        [SerializeField, Tooltip("Controls how strongly particles attempt to minimize surface area, default: 0.0, range: [0.0, +inf]")]
        float m_surfaceTension = 0;
        [SerializeField, Tooltip("Smoothes particle velocities using XSPH viscosity")]
        float m_viscosity = 0;
        [SerializeField, Tooltip("Increases vorticity by applying rotational forces to particles")]
        float m_vorticityConfinement = 0;
        [SerializeField, Tooltip("Control how much anisotropy is present in resulting ellipsoids for rendering, if zero then anisotropy will not be calculated, see flexGetAnisotropy()")]
        float m_anisotropyScale = 2.0f;
        [SerializeField, Tooltip("Clamp the anisotropy scale to this fraction of the radius")]
        float m_anisotropyMin = 0.1f;
        [SerializeField, Tooltip("Clamp the anisotropy scale to this fraction of the radius")]
        float m_anisotropyMax = 1.0f;
        [SerializeField, Tooltip("Control the strength of Laplacian smoothing in particles for rendering, if zero then smoothed positions will not be calculated, see flexGetSmoothParticles()")]
        float m_smoothing = 0;
        [SerializeField, Tooltip("Add pressure from solid surfaces to particles")]
        float m_solidPressure = 0;
        [SerializeField, Tooltip("Drag force applied to boundary fluid particles")]
        float m_freeSurfaceDrag = 0;
        [SerializeField, Tooltip("Gravity is scaled by this value for fluid particles")]
        float m_buoyancy = 1.0f;
        [SerializeField, Tooltip("Particles with kinetic energy + divergence above this threshold will spawn new diffuse particles")]
        float m_diffuseThreshold = 0;
        [SerializeField, Tooltip("Scales force opposing gravity that diffuse particles receive")]
        float m_diffuseBuoyancy = 0;
        [SerializeField, Tooltip("Scales force diffuse particles receive in direction of neighbor fluid particles")]
        float m_diffuseDrag = 0;
        [SerializeField, Tooltip("The number of neighbors below which a diffuse particle is considered ballistic")]
        int m_diffuseBallistic = 0;
        //[SerializeField, Tooltip("Diffuse particles will be sorted by depth along this axis if non-zero")]
        //Vector3 m_diffuseSortAxis = Vector3.zero;
        [SerializeField, Tooltip("Time in seconds that a diffuse particle will live for after being spawned, particles will be spawned with a random lifetime in the range [0, mDiffuseLifetime]")]
        float m_diffuseLifetime = 0;
        [SerializeField, Tooltip("Particles belonging to rigid shapes that move with a position delta magnitude > threshold will be permanently deformed in the rest pose")]
        float m_plasticThreshold = 0;
        [SerializeField, Tooltip("Controls the rate at which particles in the rest pose are deformed for particles passing the deformation threshold")]
        float m_plasticCreep = 0;
        [SerializeField, Tooltip("Distance particles maintain against shapes, note that for robust collision against triangle meshes this distance should be greater than zero")]
        float m_collisionDistance = 0.05f;
        [SerializeField, Tooltip("Increases the radius used during neighbor finding, this is useful if particles are expected to move significantly during a single step to ensure contacts aren't missed on subsequent iterations")]
        float m_particleCollisionMargin = 0;
        [SerializeField, Tooltip("Increases the radius used during contact finding against kinematic shapes")]
        float m_shapeCollisionMargin = 0;
        [SerializeField, Tooltip("Collision planes in the form ax + by + cz + d = 0")]
        Vector4[] m_planes = new Vector4[0];
        [SerializeField, Tooltip("How the relaxation is applied inside the solver")]
        Flex.RelaxationMode m_relaxationMode = Flex.RelaxationMode.Local;
        [SerializeField, Tooltip("Control the convergence rate of the parallel solver, default: 1, values greater than 1 may lead to instability")]
        float m_relaxationFactor = 1.0f;

        [SerializeField]
        Material m_fluidMaterial;

#if UNITY_EDITOR
        [SerializeField]
        bool m_showTimers = false;
#endif

#endregion
    }
}
