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
using NUnit.Framework;
using NVIDIA.Flex;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class FlexExtTests
{
    #region Tests

    [Test]
    public void TestCreateWeldedMeshIndices()
    {
        Vector3[] tetra = { new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, -1, -1) };
        Vector3[] vertices = { tetra[0], tetra[1], tetra[2], tetra[0], tetra[3], tetra[1], tetra[0], tetra[2], tetra[3], tetra[1], tetra[3], tetra[2] };
        int[] uniqueVerts = new int[vertices.Length];
        int[] originalToUniqueMap = new int[vertices.Length];
        float threshold = 0.001f;

        int numUniqueVerts = FlexExt.CreateWeldedMeshIndices(ref vertices[0], vertices.Length, ref uniqueVerts[0], ref originalToUniqueMap[0], threshold);

        Assert.AreEqual(tetra.Length, numUniqueVerts);
    }

    [Test]
    public void TestCreateClothFromMesh()
    {
        Vector4[] particles = { new Vector4(1,  1, 0, 1), new Vector4(0,  1, 0, 1), new Vector4(-1,  1, 0, 1),
                                new Vector4(1,  0, 0, 1), new Vector4(0,  0, 0, 1), new Vector4(-1,  0, 0, 1),
                                new Vector4(1, -1, 0, 1), new Vector4(0, -1, 0, 1), new Vector4(-1, -1, 0, 1) };
        int[] indices = { 0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4,
                          3, 4, 7, 3, 7, 6, 4, 5, 8, 4, 8, 7 };
        float stretchStiffness = 1.0f;
        float bendStiffness = 1.0f;
        float tetherStiffness = 1.0f;
        float tetherGive = 1.0f;
        float pressure = 0.0f;

        FlexExt.Asset.Handle handle = FlexExt.CreateClothFromMesh(ref particles[0], particles.Length, ref indices[0], indices.Length / 3, stretchStiffness, bendStiffness, tetherStiffness, tetherGive, pressure);
        if (handle.valid)
        {
            FlexExt.Asset asset = handle.asset;

            Assert.AreEqual(particles.Length, asset.numParticles);
            Assert.AreEqual(indices.Length / 3, asset.numTriangles);

            FlexExt.DestroyAsset(handle);
        }
    }

    [Test]
    public void TestCreateTearingClothFromMesh()
    {
        Vector4[] particles = { new Vector4(1,  1, 0, 1), new Vector4(0,  1, 0, 1), new Vector4(-1,  1, 0, 1),
                                new Vector4(1,  0, 0, 1), new Vector4(0,  0, 0, 1), new Vector4(-1,  0, 0, 1),
                                new Vector4(1, -1, 0, 1), new Vector4(0, -1, 0, 1), new Vector4(-1, -1, 0, 1) };
        int[] indices = { 0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4,
                          3, 4, 7, 3, 7, 6, 4, 5, 8, 4, 8, 7 };
        float stretchStiffness = 1.0f;
        float bendStiffness = 1.0f;
        float pressure = 0.0f;

        FlexExt.Asset.Handle handle = FlexExt.CreateTearingClothFromMesh(ref particles[0], particles.Length, particles.Length * 2, ref indices[0], indices.Length / 3, stretchStiffness, bendStiffness, pressure);
        FlexExt.Asset asset = handle.asset;

        Assert.AreEqual(particles.Length, asset.numParticles);
        Assert.AreEqual(indices.Length / 3, asset.numTriangles);

        FlexExt.DestroyTearingCloth(handle);
    }

    [Test]
    public void TestTearClothMesh()
    {
        Vector4[] particles = { new Vector4(1,  1, 0), new Vector4(0,  1, 0), new Vector4(-1,  1, 0),
                                new Vector4(1,  0, 0), new Vector4(0,  0, 0), new Vector4(-1,  0, 0),
                                new Vector4(1, -1, 0), new Vector4(0, -1, 0), new Vector4(-1, -1, 0) };
        int[] indices = { 0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4,
                          3, 4, 7, 3, 7, 6, 4, 5, 8, 4, 8, 7 };
        float stretchStiffness = 1.0f;
        float bendStiffness = 1.0f;
        float pressure = 0.0f;

        FlexExt.Asset.Handle handle = FlexExt.CreateTearingClothFromMesh(ref particles[0], particles.Length, particles.Length * 2, ref indices[0], indices.Length / 3, stretchStiffness, bendStiffness, pressure);

        float maxStrain = 0.0f;
        int maxSplits = 10;
        FlexExt.TearingParticleClone[] particlesCopies = new FlexExt.TearingParticleClone[10];
        int numParticlesCopies = 0;
        int maxCopies = 10;
        FlexExt.TearingMeshEdit[] triangleEdits = new FlexExt.TearingMeshEdit[10];
        int numTriangleEdits = 0;
        int maxEdits = 10;

        FlexExt.TearClothMesh(handle, maxStrain, maxSplits, ref particlesCopies[0], ref numParticlesCopies, maxCopies, ref triangleEdits[0], ref numTriangleEdits, maxEdits);
        // todo: make the mesh tear

        FlexExt.DestroyTearingCloth(handle);
    }

    [Test]
    public void TestCreateRigidFromMesh()
    {
        Vector3[] vertices = { new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, -1, -1) };
        int[] indices = { 0, 1, 2, 0, 3, 1, 0, 2, 3, 1, 3, 2 };
        float radius = 0.1f;
        float expand = 0.0f;

        FlexExt.Asset.Handle handle = FlexExt.CreateRigidFromMesh(ref vertices[0], vertices.Length, ref indices[0], indices.Length, radius, expand);
        FlexExt.Asset asset = handle.asset;

        Assert.AreEqual(2680, asset.numParticles);
        Assert.AreEqual(1, asset.numShapes);

        FlexExt.DestroyAsset(handle);
    }

    [Test]
    public void TestCreateSoftFromMesh()
    {
        Vector3[] vertices = { new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, -1, -1) };
        int[] indices = { 0, 1, 2, 0, 3, 1, 0, 2, 3, 1, 3, 2 };
        float particleSpacing = 0.1f;
        float volumeSampling = 0.1f;
        float surfaceSampling = 0.1f;
        float clusterSpacing = 0.1f;
        float clusterRadius = 0.3f;
        float clusterStiffness = 0.5f;
        float linkRadius = 0.2f;
        float linkStiffness = 0.5f;

        FlexExt.Asset.Handle handle = FlexExt.CreateSoftFromMesh(ref vertices[0], vertices.Length, ref indices[0], indices.Length, particleSpacing, volumeSampling, surfaceSampling, clusterSpacing, clusterRadius, clusterStiffness, linkRadius, linkStiffness);
        FlexExt.Asset asset = handle.asset;

        Assert.AreEqual(668, asset.numParticles);
        Assert.AreEqual(502, asset.numShapes);
        Assert.AreEqual(3856, asset.numSprings);

        FlexExt.DestroyAsset(handle);
    }

    [Test]
    public void TestCreateSoftMeshSkinning()
    {
        Vector3[] vertices = { new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, -1, -1) };
        Vector3[] bones = { new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 0, -1) };
        float falloff = 1.0f;
        float maxDistance = 2.5f;
        float[] skinningWeights = new float[vertices.Length * 4];
        int[] skinningIndices = new int[vertices.Length * 4];

        FlexExt.CreateSoftMeshSkinning(ref vertices[0], vertices.Length, ref bones[0], bones.Length, falloff, maxDistance, ref skinningWeights[0], ref skinningIndices[0]);

        Assert.AreEqual(0, skinningIndices[0]);
        Assert.AreEqual(1, skinningIndices[1]);
        Assert.AreEqual(2, skinningIndices[2]);
        Assert.AreEqual(-1, skinningIndices[3]);
        Assert.AreEqual(1.0f, skinningWeights[0] + skinningWeights[1] + skinningWeights[2] + skinningWeights[3]);
    }

    [Test]
    public void TestCreateDestroyContainer()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);

        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 10000);
        FlexExt.DestroyContainer(container);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestCreateDestroyContainerWithDevice()
    {
        Flex.InitDesc desc = new Flex.InitDesc
        {
            deviceIndex = -1,
            enableExtensions = false,
#if FLEX_CUDA
            computeType = Flex.ComputeType.CUDA,
#else
            computeType = Flex.ComputeType.D3D11,
#endif
            renderContext = default(IntPtr),
            renderDevice = default(IntPtr)
        };
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback, ref desc);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);

        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 10000);
        FlexExt.DestroyContainer(container);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestAllocateFreeParticles()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);
        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 1000);

        int n = 1000;
        int[] indices = new int[n];

        int m = FlexExt.AllocParticles(container, n, ref indices[0]);

        Assert.AreEqual(n, m);

        FlexExt.FreeParticles(container, n, ref indices[0]);

        FlexExt.DestroyContainer(container);
        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestLockUnlockParticleData()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);
        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 1000);

        FlexExt.ParticleData data = FlexExt.MapParticleData(container);

        Assert.AreNotEqual(IntPtr.Zero, data.particles);

        FlexExt.UnmapParticleData(container);

        FlexExt.DestroyContainer(container);
        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestLockUnlockTriangleData()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);
        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 1000);

        FlexExt.TriangleData data = FlexExt.MapTriangleData(container);
        data = default(FlexExt.TriangleData); // @@@

        Assert.AreEqual(IntPtr.Zero, data.indices);

        FlexExt.UnmapTriangleData(container);

        FlexExt.DestroyContainer(container);
        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestLockUnlockShapeData()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);
        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 1000);

        FlexExt.ShapeData data = FlexExt.MapShapeData(container);

        Assert.AreNotEqual(IntPtr.Zero, data.positions);

        FlexExt.UnmapShapeData(container);

        FlexExt.DestroyContainer(container);
        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestCreateDestroyInstance()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc slvDsc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref slvDsc);
        slvDsc.maxParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref slvDsc);
        FlexExt.Container container = FlexExt.CreateContainer(lib, solver, 1000);

        FlexExt.Asset.Handle asset = CreateTestClothAsset();

        FlexExt.ParticleData data = FlexExt.MapParticleData(container);

        Matrix4x4 transform = Matrix4x4.identity;

        FlexExt.Instance.Handle instance = FlexExt.CreateInstance(container, ref data, asset, ref transform, 1.0f, 1.0f, 1.0f, Flex.MakePhase(1, Flex.Phase.Default), 1.0f);

        Assert.AreEqual(asset.asset.numParticles, instance.instance.numParticles);

        FlexExt.DestroyInstance(container, instance);

        FlexExt.UnmapParticleData(container);

        FlexExt.DestroyAsset(asset);

        FlexExt.DestroyContainer(container);
        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    #endregion

    #region Private

    [MonoPInvokeCallback(typeof(Flex.ErrorCallback))]
    static void ErrorCallback(Flex.ErrorSeverity type, IntPtr msg, IntPtr file, int line)
    {
        Assert.Fail("" + type + " - " + Marshal.PtrToStringAnsi(msg) + "\nFile - " + Marshal.PtrToStringAnsi(file) + " (" + line + ")");
    }

    static FlexExt.Asset.Handle CreateTestClothAsset()
    {
        Vector4[] particles = { new Vector4( 1, 1, 0), new Vector4( 0, 1, 0), new Vector4(-1, 1, 0),
                                new Vector4( 1, 0, 0), new Vector4( 0, 0, 0), new Vector4(-1, 0, 0),
                                new Vector4( 1,-1, 0), new Vector4( 0,-1, 0), new Vector4(-1,-1, 0) };
        int[] indices = { 0, 1, 4, 0, 4, 3, 1, 2, 5, 1, 5, 4,
                          3, 4, 7, 3, 7, 6, 4, 5, 8, 4, 8, 7 };
        float stretchStiffness = 1.0f;
        float bendStiffness = 1.0f;
        float tetherStiffness = 1.0f;
        float tetherGive = 1.0f;
        float pressure = 0.0f;

        return FlexExt.CreateClothFromMesh(ref particles[0], particles.Length, ref indices[0], indices.Length / 3, stretchStiffness, bendStiffness, tetherStiffness, tetherGive, pressure);
    }

#endregion
}
