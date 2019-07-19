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
using System.Runtime.InteropServices;
using UnityEngine;

namespace NVIDIA.Flex
{
    public static class FlexUtils
    {
        const string FLEXUTILS_DLL = "flexUtils";

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsDeviceFromResource")]
        public static extern IntPtr DeviceFromResource(IntPtr resource);

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref int src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref int dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref float src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref float dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref Vector2 src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref Vector2 dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref Vector3 src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref Vector3 dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref Vector4 src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref Vector4 dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref Quaternion src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref Quaternion dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref Flex.CollisionGeometry src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref Flex.CollisionGeometry dst, int dstOfs, int size);

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref FlexExt.Instance src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref FlexExt.Instance dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(ref FlexExt.Asset src, int srcOfs, IntPtr dst, int dstOfs, int size);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsFastCopy")]
        public static extern void FastCopy(IntPtr src, int srcOfs, ref FlexExt.Asset dst, int dstOfs, int size);

        public static void FastCopy(int[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * 4); }
        public static void FastCopy(IntPtr source, int[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * 4); }
        public static void FastCopy(float[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * 4); }
        public static void FastCopy(IntPtr source, float[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * 4); }
        public static void FastCopy(Vector2[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * 12); }
        public static void FastCopy(IntPtr source, Vector2[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * 12); }
        public static void FastCopy(Vector3[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * 12); }
        public static void FastCopy(IntPtr source, Vector3[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * 12); }
        public static void FastCopy(Vector4[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * 16); }
        public static void FastCopy(IntPtr source, Vector4[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * 16); }
        public static void FastCopy(Quaternion[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * 16); }
        public static void FastCopy(IntPtr source, Quaternion[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * 16); }
        static int collisionGeometrySize = Marshal.SizeOf(default(Flex.CollisionGeometry));
        public static void FastCopy(Flex.CollisionGeometry[] source, IntPtr destination) { FastCopy(ref source[0], 0, destination, 0, source.Length * collisionGeometrySize); }
        public static void FastCopy(IntPtr source, Flex.CollisionGeometry[] destination) { FastCopy(source, 0, ref destination[0], 0, destination.Length * collisionGeometrySize); }

        static int instanceSize = Marshal.SizeOf(default(FlexExt.Instance));
        public static void FastCopy(ref FlexExt.Instance source, IntPtr destination) { FastCopy(ref source, 0, destination, 0, instanceSize); }
        public static void FastCopy(IntPtr source, ref FlexExt.Instance destination) { FastCopy(source, 0, ref destination, 0, instanceSize); }
        static int assetSize = Marshal.SizeOf(default(FlexExt.Asset));
        public static void FastCopy(ref FlexExt.Asset source, IntPtr destination) { FastCopy(ref source, 0, destination, 0, assetSize); }
        public static void FastCopy(IntPtr source, ref FlexExt.Asset destination) { FastCopy(source, 0, ref destination, 0, assetSize); }

        public static void SafeFastCopy<T>(T[] source, IntPtr destination) where T : struct
        {
            long ptr = destination.ToInt64(), stride = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < source.Length; ++i)
                Marshal.StructureToPtr(source[i], new IntPtr(ptr + stride * i), false);
        }
        public static void SafeFastCopy<T>(IntPtr source, T[] destination) where T : struct
        {
            long ptr = source.ToInt64(), stride = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < destination.Length; ++i)
                destination[i] = (T)Marshal.PtrToStructure(new IntPtr(ptr + stride * i), typeof(T));
        }

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsPickParticle")]
        public static extern int PickParticle(ref Vector3 origin, ref Vector3 dir, ref Vector4 particles, ref int phases, int n, float radius);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsPickParticle")]
        public static extern int PickParticle(ref Vector3 origin, ref Vector3 dir, ref Vector4 particles, IntPtr phases, int n, float radius);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsPickParticle")]
        public static extern int PickParticle(ref Vector3 origin, ref Vector3 dir, IntPtr particles, IntPtr phases, int n, float radius);

        public static int PickParticle(Ray ray, Vector4[] particles, float radius)
        {
            Vector3 origin = ray.origin, dir = ray.direction;
            return PickParticle(ref origin, ref dir, ref particles[0], default(IntPtr), particles.Length, radius);
        }

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsConvexPlanes")]
        public static extern int ConvexPlanes(ref Vector3 meshVertices, ref Vector3 localScale, ref int meshTriangles, int triangleCount, ref Vector4 planes, ref Vector3 bounds);

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsClothRefPoints")]
        public static extern void ClothRefPoints(ref Vector4 particles, int count, ref int refPoints);

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsComputeBounds")]
        public static extern void ComputeBounds(ref Vector4 particles, ref int indices, int count, ref Vector3 boundsMin, ref Vector3 boundsMax);
        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsComputeBounds")]
        public static extern void ComputeBounds(IntPtr particles, ref int indices, int count, ref Vector3 boundsMin, ref Vector3 boundsMax);

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsUpdateSourceParticles")]
        public static extern int UpdateSourceParticles(ref int indices, ref float ages, int count, IntPtr particles, float dT, float massScale);

        [DllImport(FLEXUTILS_DLL, EntryPoint = "flexUtilsUpdateClothVertices")]
        public static extern void UpdateClothVertices(ref int particles, ref int vertices, int count, ref int indices, IntPtr particlePositions, IntPtr particleNormals, ref Matrix4x4 transform, ref Vector3 vertexPositions, ref Vector3 vertexNormals);
    }
}