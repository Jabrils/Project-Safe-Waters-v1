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

using NUnit.Framework;
using NVIDIA.Flex;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class FlexUtilsTests
{
    [Test]
    public void GetD3D11DeviceTest()
    {
        ComputeBuffer buffer = new ComputeBuffer(1, 4);
        IntPtr device = FlexUtils.DeviceFromResource(buffer.GetNativeBufferPtr());
        buffer.Release();
        Assert.AreNotEqual(default(IntPtr), device);
    }

    [Test]
    public void FastCopyTest()
    {
        Vector2[] src = { new Vector2(0, 1), new Vector2(2, 3), new Vector2(4, 5), };
        int size = Marshal.SizeOf(src[0]) * src.Length;

        IntPtr dst = Marshal.AllocHGlobal(size);
        FlexUtils.FastCopy(ref src[0], 0, dst, 0, size);

        Vector2[] tst = new Vector2[src.Length];
        FlexUtils.FastCopy(dst, 0, ref tst[0], 0, size);

        Marshal.FreeHGlobal(dst);

        for (int i = 0; i < src.Length; ++i)
            Assert.AreEqual(src[i], tst[i]);
    }

    [Test]
    public void PickParticleTest()
    {
        Vector4[] particles = new Vector4[3] { new Vector4(0, 0, 0, 1), new Vector4(1, 0, 0, 1), new Vector4(2, 0, 0, 1) };
        int[] phases = new int[3];

        {
            Vector3 origin = new Vector3(1, 1, 0);
            Vector3 direction = new Vector3(0, -1, 0);
            int index = FlexUtils.PickParticle(ref origin, ref direction, ref particles[0], ref phases[0], 3, 0.1f);
            Assert.AreEqual(1, index);
        }

        {
            Vector3 origin = new Vector3(2, 1, 0);
            Vector3 direction = new Vector3(0, -1, 0);
            int index = FlexUtils.PickParticle(ref origin, ref direction, ref particles[0], ref phases[0], 3, 0.1f);
            Assert.AreEqual(2, index);
        }

        {
            Vector3 origin = new Vector3(-1, 0, 0);
            Vector3 direction = new Vector3(1, 0, 0);
            int index = FlexUtils.PickParticle(ref origin, ref direction, ref particles[0], ref phases[0], 3, 0.1f);
            Assert.AreEqual(0, index);
        }
    }

    [Test]
    public void ConvexPlanesTest()
    {
        Vector3[] meshVertices = new Vector3[] { new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f) };
        Vector3 localScale = Vector3.one * 0.5f;
        int[] meshTriangles = new int[] { 0, 2, 3, 0, 3, 1, 8, 4, 5, 8, 5, 9, 10, 6, 7, 10, 7, 11, 12, 13, 14, 12, 14, 15, 16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23 };
        Vector4[] convexPlanes = new Vector4[meshTriangles.Length / 3];
        Vector3[] bounds = new Vector3[2];

        int planesCount = FlexUtils.ConvexPlanes(ref meshVertices[0], ref localScale, ref meshTriangles[0], meshTriangles.Length / 3, ref convexPlanes[0], ref bounds[0]);

        Assert.AreEqual(6, planesCount);
    }
}
