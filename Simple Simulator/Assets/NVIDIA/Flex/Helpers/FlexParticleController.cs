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
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    [AddComponentMenu("NVIDIA/Flex/Flex Particle Controller")]
    public class FlexParticleController : MonoBehaviour
    {
        #region Messages

        void OnEnable()
        {
            m_actor = GetComponent<FlexActor>();
            if (m_actor)
            {
                m_actor.onFlexUpdate += OnFlexUpdate;
                m_particleSystem = GetComponent<ParticleSystem>();
                if (m_particleSystem)
                {
                    m_particleSystem.Emit(m_actor.indexCount);
                    m_particleSystem.Stop();
                }
            }
        }

        void OnDisable()
        {
            if (m_actor)
            {
                m_actor.onFlexUpdate -= OnFlexUpdate;
                m_actor = null;
            }
        }

        void Update()
        {
            var main = m_particleSystem.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.maxParticles = (m_actor && m_actor.asset) ? m_actor.asset.maxParticles : 0;
            var emission = m_particleSystem.emission;
            emission.enabled = false;
            var shape = m_particleSystem.shape;
            shape.enabled = false;
            //m_particleSystem.Stop();
        }

        void LateUpdate()
        {
            m_particleSystem.SetParticles(m_particles, m_particles.Length);
            //FlexActor actor = GetComponent<FlexActor>();
            //if (actor is FlexSourceActor)
            //{
            //    FlexSourceActor sourceActor = actor as FlexSourceActor;
            //    float time = Time.time - Time.fixedTime;
            //    int[] indices = sourceActor.indices;
            //    int indexCount = sourceActor.indexCount;
            //    float[] ages = sourceActor.ages;
            //    m_particleSystem.Clear();
            //    m_particleSystem.Emit(indices.Length);
            //    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[indexCount];
            //    m_particleSystem.GetParticles(particles);
            //    for (int i = 0; i < indexCount; ++i)
            //    {
            //        ParticleSystem.Particle p = particles[i];
            //        //p.velocity = sourceActor.container.GetVelocity(indices[i]); ;
            //        //p.position = (Vector3)sourceActor.container.GetParticle(indices[i]) + p.velocity * (time - Time.fixedDeltaTime);
            //        p.remainingLifetime = ages[i] - time;
            //        particles[i] = p;
            //    }
            //    m_particleSystem.SetParticles(particles, particles.Length);
            //}
            //else if (actor)
            //{
            //    var main = m_particleSystem.main;
            //    //float time = Time.time - Time.fixedTime;
            //    int[] indices = actor.indices;
            //    int indexCount = actor.indexCount;
            //    //m_particleSystem.Clear();
            //    //m_particleSystem.Emit(indices.Length);
            //    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[indexCount];
            //    m_particleSystem.GetParticles(particles);
            //    for (int i = 0; i < indexCount; ++i)
            //    {
            //        ParticleSystem.Particle p = particles[i];
            //        //p.velocity = actor.container.GetVelocity(indices[i]);
            //        //p.position = (Vector3)actor.container.GetParticle(indices[i]) + p.velocity * (time - Time.fixedDeltaTime);
            //        p.remainingLifetime = m_particleSystem.main.startLifetime.Evaluate(0);
            //        p.startLifetime = p.remainingLifetime;
            //        p.startColor = main.startColor.Evaluate(0);
            //        p.startSize = main.startSize.Evaluate(0);
            //        particles[i] = p;
            //    }
            //    m_particleSystem.SetParticles(particles, particles.Length);
            //}
        }

        #endregion

        #region Private

        void OnFlexUpdate(FlexContainer.ParticleData _particleData)
        {
            FlexActor actor = GetComponent<FlexActor>();
            if (actor is FlexSourceActor)
            {
                FlexSourceActor sourceActor = actor as FlexSourceActor;
                var main = m_particleSystem.main;
                float time = Time.time - Time.fixedTime;
                int[] indices = sourceActor.indices;
                int indexCount = sourceActor.indexCount;
                float[] ages = sourceActor.ages;
                m_particleSystem.Clear();
                m_particleSystem.Emit(indices.Length);
                if (m_particles.Length != indexCount) m_particles = new ParticleSystem.Particle[indexCount];
                float startLifetime = main.startLifetime.Evaluate(0);
                Color32 startColor = main.startColor.Evaluate(0);
                float startSize = main.startSize.Evaluate(0);
                for (int i = 0; i < indexCount; ++i)
                {
                    ParticleSystem.Particle p = m_particles[i];
                    p.velocity = _particleData.GetVelocity(indices[i]); ;
                    p.position = (Vector3)_particleData.GetParticle(indices[i]) + p.velocity * (time - Time.fixedDeltaTime);
                    p.remainingLifetime = ages[i] - time;
                    p.startLifetime = startLifetime;
                    p.startColor = startColor;
                    p.startSize = startSize;
                    m_particles[i] = p;
                }
                //m_particleSystem.SetParticles(m_particles, m_particles.Length);
            }
            else if (actor)
            {
                var main = m_particleSystem.main;
                float time = Time.time - Time.fixedTime;
                int[] indices = actor.indices;
                int indexCount = actor.indexCount;
                //m_particleSystem.Clear();
                //m_particleSystem.Emit(indices.Length);
                if (m_particles.Length != indexCount) m_particles = new ParticleSystem.Particle[indexCount];
                Color32 startColor = main.startColor.Evaluate(0);
                float startSize = main.startSize.Evaluate(0);
                for (int i = 0; i < indexCount; ++i)
                {
                    ParticleSystem.Particle p = m_particles[i];
                    p.velocity = _particleData.GetVelocity(indices[i]);
                    p.position = (Vector3)_particleData.GetParticle(indices[i]) + p.velocity * (time - Time.fixedDeltaTime);
                    p.remainingLifetime = m_particleSystem.main.startLifetime.Evaluate(0);
                    p.startLifetime = p.remainingLifetime;
                    p.startColor = startColor;
                    p.startSize = startSize;
                    m_particles[i] = p;
                }
                //m_particleSystem.SetParticles(m_particles, m_particles.Length);
            }
            m_particleSystem.SetParticles(m_particles, m_particles.Length);
        }

        FlexActor m_actor;
        ParticleSystem m_particleSystem;
        ParticleSystem.Particle[] m_particles = new ParticleSystem.Particle[0];

        #endregion
    }
}
