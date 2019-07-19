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
using UnityEngine;

namespace NVIDIA.Flex
{
    public class FlexBuffer
    {
        #region Constructor

        public FlexBuffer(Flex.Library _library, int _count, int _stride, Flex.BufferType _type = Flex.BufferType.Host)
        {
            m_handle = Flex.AllocBuffer(_library, _count, _stride, _type);
            m_count = _count; m_stride = _stride;
        }

        ~FlexBuffer()
        {
            Release();
        }

        #endregion

        #region Properties

        public Flex.Buffer handle
        {
            get { Unmap(); return m_handle; }
        }

        public int count
        {
            get { return m_count; }
        }

        public int stride
        {
            get { return m_stride; }
        }

        #endregion

        #region Methods

        public void Get(int _index, ref int _value) { Map(); FlexUtils.FastCopy(m_pointer, _index * m_stride, ref _value, 0, m_stride); }
        public void Set(int _index, int _value) { Map(); FlexUtils.FastCopy(ref _value, 0, m_pointer, _index * m_stride, m_stride); }
        public void Get(int _start, int _count, int[] _data) { Map(); FlexUtils.FastCopy(m_pointer, _start * m_stride, ref _data[0], 0, _count * m_stride); }
        public void Set(int _start, int _count, int[] _data) { Map(); FlexUtils.FastCopy(ref _data[0], 0, m_pointer, _start * m_stride, _count * m_stride); }

        public void Get(int _index, ref float _value) { Map(); FlexUtils.FastCopy(m_pointer, _index * m_stride, ref _value, 0, m_stride); }
        public void Set(int _index, float _value) { Map(); FlexUtils.FastCopy(ref _value, 0, m_pointer, _index * m_stride, m_stride); }
        public void Get(int _start, int _count, float[] _data) { Map(); FlexUtils.FastCopy(m_pointer, _start * m_stride, ref _data[0], 0, _count * m_stride); }
        public void Set(int _start, int _count, float[] _data) { Map(); FlexUtils.FastCopy(ref _data[0], 0, m_pointer, _start * m_stride, _count * m_stride); }

        public void Get(int _index, ref Vector3 _value) { Map(); FlexUtils.FastCopy(m_pointer, _index * m_stride, ref _value, 0, m_stride); }
        public void Set(int _index, Vector3 _value) { Map(); FlexUtils.FastCopy(ref _value, 0, m_pointer, _index * m_stride, m_stride); }
        public void Get(int _start, int _count, Vector3[] _data) { Map(); FlexUtils.FastCopy(m_pointer, _start * m_stride, ref _data[0], 0, _count * m_stride); }
        public void Set(int _start, int _count, Vector3[] _data) { Map(); FlexUtils.FastCopy(ref _data[0], 0, m_pointer, _start * m_stride, _count * m_stride); }

        public void Get(int _index, ref Vector4 _value) { Map(); FlexUtils.FastCopy(m_pointer, _index * m_stride, ref _value, 0, m_stride); }
        public void Set(int _index, Vector4 _value) { Map(); FlexUtils.FastCopy(ref _value, 0, m_pointer, _index * m_stride, m_stride); }
        public void Get(int _start, int _count, Vector4[] _data) { Map(); FlexUtils.FastCopy(m_pointer, _start * m_stride, ref _data[0], 0, _count * m_stride); }
        public void Set(int _start, int _count, Vector4[] _data) { Map(); FlexUtils.FastCopy(ref _data[0], 0, m_pointer, _start * m_stride, _count * m_stride); }

        public void Get(int _index, ref Quaternion _value) { Map(); FlexUtils.FastCopy(m_pointer, _index * m_stride, ref _value, 0, m_stride); }
        public void Set(int _index, Quaternion _value) { Map(); FlexUtils.FastCopy(ref _value, 0, m_pointer, _index * m_stride, m_stride); }
        public void Get(int _start, int _count, Quaternion[] _data) { Map(); FlexUtils.FastCopy(m_pointer, _start * m_stride, ref _data[0], 0, _count * m_stride); }
        public void Set(int _start, int _count, Quaternion[] _data) { Map(); FlexUtils.FastCopy(ref _data[0], 0, m_pointer, _start * m_stride, _count * m_stride); }

        public void Get(int _index, ref Flex.CollisionGeometry _value) { Map(); FlexUtils.FastCopy(m_pointer, _index * m_stride, ref _value, 0, m_stride); }
        public void Set(int _index, Flex.CollisionGeometry _value) { Map(); FlexUtils.FastCopy(ref _value, 0, m_pointer, _index * m_stride, m_stride); }
        public void Get(int _start, int _count, Flex.CollisionGeometry[] _data) { Map(); FlexUtils.FastCopy(m_pointer, _start * m_stride, ref _data[0], 0, _count * m_stride); }
        public void Set(int _start, int _count, Flex.CollisionGeometry[] _data) { Map(); FlexUtils.FastCopy(ref _data[0], 0, m_pointer, _start * m_stride, _count * m_stride); }

        public void Release()
        {
            if (m_handle)
            {
                Unmap();
                Flex.FreeBuffer(m_handle);
                m_handle.Clear();
            }
        }

        #endregion

        #region Private

        void Map()
        {
            if (m_pointer == default(IntPtr))
            {
                m_pointer = Flex.Map(m_handle);
            }
        }

        void Unmap()
        {
            if (m_pointer != default(IntPtr))
            {
                Flex.Unmap(m_handle);
                m_pointer = default(IntPtr);
            }
        }

        Flex.Buffer m_handle;
        int m_count, m_stride;
        IntPtr m_pointer;

        #endregion
    }
}