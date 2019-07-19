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
    public class FlexAsset : ScriptableObject
    {
        #region Properties

        public Vector4[] particles { get { return m_particles; } }
        public int maxParticles { get { return m_maxParticles; } }
        public int[] springIndices { get { return m_springIndices; } }
        public float[] springCoefficients { get { return m_springCoefficients; } }
        public float[] springRestLengths { get { return m_springRestLengths; } }
        public int[] shapeIndices { get { return m_shapeIndices; } }
        public int[] shapeOffsets { get { return m_shapeOffsets; } }
        public float[] shapeCoefficients { get { return m_shapeCoefficients; } }
        public Vector3[] shapeCenters { get { return m_shapeCenters; } }
        public int[] triangleIndices { get { return m_triangleIndices; } }
        public int[] fixedParticles { get { return m_fixedParticles; } }

        public FlexExt.Asset.Handle handle { get { OnEnable(); return m_assetHandle; } }

        #endregion

        #region Events

        public delegate void OnBeforeRebuildFn();
        public OnBeforeRebuildFn onBeforeRebuild;

        public delegate void OnAfterRebuildFn();
        public OnAfterRebuildFn onAfterRebuild;

        #endregion

        #region Methods

        public bool FixedParticle(int _index, bool _yes)
        {
            int exists =  Array.IndexOf(m_fixedParticles, _index);
            if (exists == -1 && _yes)
            {
                Array.Resize(ref m_fixedParticles, m_fixedParticles.Length + 1);
                m_fixedParticles[m_fixedParticles.Length - 1] = _index;
                return true;
            }
            else if (exists != -1 && !_yes)
            {
                m_fixedParticles[exists] = m_fixedParticles[m_fixedParticles.Length - 1];
                Array.Resize(ref m_fixedParticles, m_fixedParticles.Length - 1);
                return true;
            }
            return false;
        }

        public void ClearFixedParticles()
        {
            m_fixedParticles = new int[0];
        }

        public void Rebuild()
        {
            if (onBeforeRebuild != null) onBeforeRebuild();

            DestroyAsset();
            RebuildAsset();
            CreateAsset();

            if (onAfterRebuild != null) onAfterRebuild();
        }

        #endregion

        #region Messages

        void OnEnable()
        {
            CreateAsset();
        }

        void OnDisable()
        {
            DestroyAsset();
        }

        void OnValidate()
        {
            ValidateFields();
            if (m_rebuildAsset) Rebuild();
        }

        #endregion

        #region Protected

        protected virtual void ValidateFields()
        {
            m_maxParticles = Mathf.Max(m_maxParticles, 1);
        }

        protected virtual void RebuildAsset()
        {
            m_rebuildAsset = false;
        }

        protected void StoreAsset(FlexExt.Asset _asset)
        {
            m_particles = new Vector4[_asset.numParticles];
            if (_asset.numParticles > 0) FlexUtils.SafeFastCopy(_asset.particles, m_particles);
            m_maxParticles = _asset.maxParticles;

            m_springIndices = new int[_asset.numSprings * 2];
            if (_asset.numSprings > 0) FlexUtils.SafeFastCopy(_asset.springIndices, m_springIndices);
            m_springCoefficients = new float[_asset.numSprings];
            if (_asset.numSprings > 0) FlexUtils.SafeFastCopy(_asset.springCoefficients, m_springCoefficients);
            m_springRestLengths = new float[_asset.numSprings];
            if (_asset.numSprings > 0) FlexUtils.SafeFastCopy(_asset.springRestLengths, m_springRestLengths);

            m_shapeIndices = new int[_asset.numShapeIndices];
            if (_asset.numShapeIndices > 0) FlexUtils.SafeFastCopy(_asset.shapeIndices, m_shapeIndices);
            m_shapeOffsets = new int[_asset.numShapes];
            if (_asset.numShapes > 0) FlexUtils.SafeFastCopy(_asset.shapeOffsets, m_shapeOffsets);
            m_shapeCoefficients = new float[_asset.numShapes];
            if (_asset.numShapes > 0) FlexUtils.SafeFastCopy(_asset.shapeCoefficients, m_shapeCoefficients);
            m_shapeCenters = new Vector3[_asset.numShapes];
            if (_asset.numShapes > 0) FlexUtils.SafeFastCopy(_asset.shapeCenters, m_shapeCenters);

            m_triangleIndices = new int[_asset.numTriangles * 3];
            if (_asset.numTriangles > 0) FlexUtils.SafeFastCopy(_asset.triangleIndices, m_triangleIndices);

            m_inflatable = _asset.inflatable;
            m_inflatableVolume = _asset.inflatableVolume;
            m_inflatablePressure = _asset.inflatablePressure;
            m_inflatableStiffness = _asset.inflatableStiffness;
        }

        protected void CreateAsset()
        {
            m_asset = default(FlexExt.Asset);
            if (m_particles.Length > 0)
            {
                m_asset.particles = Marshal.AllocHGlobal(m_particles.Length * Marshal.SizeOf(default(Vector4)));
                FlexUtils.SafeFastCopy(m_particles, m_asset.particles);
                m_asset.numParticles = m_particles.Length;
            }
            m_asset.maxParticles = Mathf.Max(m_maxParticles, m_particles.Length);
            if (m_springIndices.Length > 0 && m_springCoefficients.Length > 0 && m_springRestLengths.Length > 0)
            {
                m_asset.springIndices = Marshal.AllocHGlobal(m_springIndices.Length * Marshal.SizeOf(default(int)));
                FlexUtils.SafeFastCopy(m_springIndices, m_asset.springIndices);
                m_asset.springCoefficients = Marshal.AllocHGlobal(m_springCoefficients.Length * Marshal.SizeOf(default(float)));
                FlexUtils.SafeFastCopy(m_springCoefficients, m_asset.springCoefficients);
                m_asset.springRestLengths = Marshal.AllocHGlobal(m_springRestLengths.Length * Marshal.SizeOf(default(float)));
                FlexUtils.SafeFastCopy(m_springRestLengths, m_asset.springRestLengths);
                m_asset.numSprings = m_springRestLengths.Length;
            }
            if (m_shapeIndices.Length > 0 && m_shapeOffsets.Length > 0 && m_shapeCoefficients.Length > 0 && m_shapeCenters.Length > 0)
            {
                m_asset.shapeIndices = Marshal.AllocHGlobal(m_shapeIndices.Length * Marshal.SizeOf(default(int)));
                FlexUtils.SafeFastCopy(m_shapeIndices, m_asset.shapeIndices);
                m_asset.numShapeIndices = m_shapeIndices.Length;
                m_asset.shapeOffsets = Marshal.AllocHGlobal(m_shapeOffsets.Length * Marshal.SizeOf(default(int)));
                FlexUtils.SafeFastCopy(m_shapeOffsets, m_asset.shapeOffsets);
                m_asset.shapeCoefficients = Marshal.AllocHGlobal(m_shapeCoefficients.Length * Marshal.SizeOf(default(float)));
                FlexUtils.SafeFastCopy(m_shapeCoefficients, m_asset.shapeCoefficients);
                m_asset.shapeCenters = Marshal.AllocHGlobal(m_shapeCenters.Length * Marshal.SizeOf(default(Vector3)));
                FlexUtils.SafeFastCopy(m_shapeCenters, m_asset.shapeCenters);
                m_asset.numShapes = m_shapeCenters.Length;
            }
            if (m_triangleIndices.Length > 0)
            {
                m_asset.triangleIndices = Marshal.AllocHGlobal(m_triangleIndices.Length * Marshal.SizeOf(default(int)));
                FlexUtils.SafeFastCopy(m_triangleIndices, m_asset.triangleIndices);
                m_asset.numTriangles = m_triangleIndices.Length / 3;
            }
            // @@@ Inflatables crash on some GPUs
            bool disableInflatables = !SystemInfo.graphicsDeviceVendor.Contains("NVIDIA");
            // @@@
            m_asset.inflatable = m_inflatable && !disableInflatables;
            m_asset.inflatableVolume = m_inflatableVolume;
            m_asset.inflatablePressure = m_inflatablePressure;
            m_asset.inflatableStiffness = m_inflatableStiffness;

            m_assetHandle.Allocate();
            m_assetHandle.asset = m_asset;
        }

        protected void DestroyAsset()
        {
            if (m_assetHandle)
            {
                if (m_asset.particles != default(IntPtr)) Marshal.FreeHGlobal(m_asset.particles);
                if (m_asset.springIndices != default(IntPtr)) Marshal.FreeHGlobal(m_asset.springIndices);
                if (m_asset.springCoefficients != default(IntPtr)) Marshal.FreeHGlobal(m_asset.springCoefficients);
                if (m_asset.springRestLengths != default(IntPtr)) Marshal.FreeHGlobal(m_asset.springRestLengths);
                if (m_asset.shapeIndices != default(IntPtr)) Marshal.FreeHGlobal(m_asset.shapeIndices);
                if (m_asset.shapeOffsets != default(IntPtr)) Marshal.FreeHGlobal(m_asset.shapeOffsets);
                if (m_asset.shapeCoefficients != default(IntPtr)) Marshal.FreeHGlobal(m_asset.shapeCoefficients);
                if (m_asset.shapeCenters != default(IntPtr)) Marshal.FreeHGlobal(m_asset.shapeCenters);
                if (m_asset.triangleIndices != default(IntPtr)) Marshal.FreeHGlobal(m_asset.triangleIndices);

                m_assetHandle.Free();
                m_assetHandle.Clear();
            }
        }

        #endregion

        #region Private

        FlexExt.Asset m_asset = new FlexExt.Asset();
        FlexExt.Asset.Handle m_assetHandle;

        [SerializeField]
        Vector4[] m_particles = new Vector4[0];
        [SerializeField]
        int m_maxParticles = 1000;
        [SerializeField]
        int[] m_springIndices = new int[0];
        [SerializeField]
        float[] m_springCoefficients = new float[0];
        [SerializeField]
        float[] m_springRestLengths = new float[0];
        [SerializeField]
        int[] m_shapeIndices = new int[0];
        [SerializeField]
        int[] m_shapeOffsets = new int[0];
        [SerializeField]
        float[] m_shapeCoefficients = new float[0];
        [SerializeField]
        Vector3[] m_shapeCenters = new Vector3[0];
        [SerializeField]
        int[] m_triangleIndices = new int[0];
        [SerializeField]
        bool m_inflatable = false;
        [SerializeField]
        float m_inflatableVolume = 0.0f;
        [SerializeField]
        float m_inflatablePressure = 0.0f;
        [SerializeField]
        float m_inflatableStiffness = 0.0f;
        [SerializeField]
        int[] m_fixedParticles = new int[0];

        [SerializeField]
        bool m_rebuildAsset = false;

        #endregion
    }
}
