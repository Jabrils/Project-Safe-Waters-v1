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

public class FlexTests
{
    #region Tests

    [Test]
    public void TestInitShutdownLibrary()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        int flexVersion = Flex.GetVersion();
        Assert.AreEqual(Flex.FLEX_VERSION, flexVersion);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestGetLibraryVersion()
    {
        Flex.Library lib = Flex.Init();

        int flexVersion = Flex.GetVersion();
        Assert.AreEqual(Flex.FLEX_VERSION, flexVersion);

        Flex.Shutdown(lib);
    }

    [Test]
    public void TestCreateDestroySolver()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);

        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);
        Flex.DestroySolver(solver);

        Flex.Shutdown(lib);
    }

    [Test]
    public void TestRegisterSolverCallback()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);

        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.SolverCallback callback;
        callback.userData = new IntPtr(1);
        callback.function = (p) => { Assert.AreEqual(callback.userData, p.userData); };
        Flex.SolverCallback oldCallback = Flex.RegisterSolverCallback(solver, callback, Flex.SolverCallbackStage.SubstepBegin);
        Assert.AreNotEqual(callback.userData, oldCallback.userData);

        Flex.DestroySolver(solver);

        Flex.Shutdown(lib);
    }

    [Test]
    public void TestUpdateSolver()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.UpdateSolver(solver, 0.016f, 1);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetGetParams()
    {
        Vector3 GRAVITY = new Vector3(1, 2, 3);

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();

        Flex.GetParams(solver, ref prms);
        Assert.AreNotEqual(GRAVITY, prms.gravity);

        prms.gravity = GRAVITY;

        Flex.SetParams(solver, ref prms);
        Assert.AreEqual(GRAVITY, prms.gravity);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetGetActive()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.SetActiveCount(solver, 2);

        Flex.Buffer active = CreateBuffer(lib, 2, 1, new int[]
        {
            2, 1
        });
        Flex.SetActive(solver, active);

        Assert.AreEqual(2, Flex.GetActiveCount(solver));

        Flex.FreeBuffer(active);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetGetParticles()
    {
        Vector3 GRAVITY = new Vector3(1, 2, 3);
        float DELTA_T = 0.016f;

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();
        Flex.GetParams(solver, ref prms);
        prms.gravity = GRAVITY;
        Flex.SetParams(solver, ref prms);

        Flex.Buffer particles = CreateBuffer(lib, 1, 4, new float[]
        {
            0.0f, 0.0f, 0.0f, 1.0f,
        });
        Flex.SetParticles(solver, particles);

        Flex.SetActiveCount(solver, 1);

        Flex.Buffer active = CreateBuffer(lib, 1, 1, new int[]
        {
            0,
        });
        Flex.SetActive(solver, active);

        Flex.UpdateSolver(solver, DELTA_T, 1);

        Flex.GetParticles(solver, particles);
        float[] values; ReadBuffer(lib, particles, 1, 4, out values);

        for (int i = 0; i < 3; ++i)
            Assert.AreEqual(GRAVITY * DELTA_T * DELTA_T, new Vector3(values[0], values[1], values[2]));

        Flex.FreeBuffer(particles);
        Flex.FreeBuffer(active);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetGetParticlesVelocities()
    {
        Vector3 GRAVITY = new Vector3(1, 2, 3);
        float DELTA_T = 0.016f;

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();
        Flex.GetParams(solver, ref prms);
        prms.gravity = GRAVITY;
        Flex.SetParams(solver, ref prms);

        Flex.Buffer particles = CreateBuffer(lib, 1, 4, new float[]
        {
            0.0f, 0.0f, 0.0f, 1.0f,
        });
        Flex.SetParticles(solver, particles);

        Flex.Buffer velocities = CreateBuffer(lib, 1, 3, new float[]
        {
            0.0f, 0.0f, 0.0f,
        });
        Flex.SetVelocities(solver, velocities);

        Flex.SetActiveCount(solver, 1);

        Flex.Buffer active = CreateBuffer(lib, 1, 1, new int[]
        {
            0
        });
        Flex.SetActive(solver, active);

        Flex.UpdateSolver(solver, DELTA_T, 1);

        Flex.GetVelocities(solver, velocities);
        float[] values; ReadBuffer(lib, velocities, 1, 3, out values);

        Assert.AreEqual(GRAVITY * DELTA_T, new Vector3(values[0], values[1], values[2]));

        Flex.FreeBuffer(particles);
        Flex.FreeBuffer(velocities);
        Flex.FreeBuffer(active);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetParticlesPhases()
    {
        float DELTA_T = 0.016f;
        float INTERACTION_DISTANCE = 0.5f;
        float SOLID_REST_DISTANCE = 0.2f;

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();
        Flex.GetParams(solver, ref prms);
        prms.radius = INTERACTION_DISTANCE;
        prms.solidRestDistance = SOLID_REST_DISTANCE;
        Flex.SetParams(solver, ref prms);

        Flex.Buffer particles = CreateBuffer(lib, 2, 4, new float[]
        {
           -0.001f, 0.0f, 0.0f, 1.0f,
            0.001f, 0.0f, 0.0f, 1.0f,
        });
        Flex.SetParticles(solver, particles);

        Flex.Buffer phases = CreateBuffer(lib, 2, 1, new int[]
        {
            Flex.MakePhase(1, Flex.Phase.SelfCollide),
            Flex.MakePhase(1, Flex.Phase.SelfCollide),
        });
        Flex.SetPhases(solver, phases);

        Flex.SetActiveCount(solver, 2);

        Flex.Buffer active = CreateBuffer(lib, 2, 1, new int[]
        {
            0, 1,
        });
        Flex.SetActive(solver, active);

        Flex.UpdateSolver(solver, DELTA_T, 1);

        Flex.GetParticles(solver, particles);
        float[] values; ReadBuffer(lib, particles, 2, 4, out values);

        Assert.AreEqual(SOLID_REST_DISTANCE, Vector3.Distance(new Vector3(values[0], values[1], values[2]), new Vector3(values[4], values[5], values[6])));

        Flex.FreeBuffer(particles);
        Flex.FreeBuffer(phases);
        Flex.FreeBuffer(active);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetSpringConstraints()
    {
        float DELTA_T = 0.016f;
        float INTERACTION_DISTANCE = 0.5f;
        float SOLID_REST_DISTANCE = 0.2f;
        float SPRING_LENGTH = 1.0f;

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();
        Flex.GetParams(solver, ref prms);
        prms.radius = INTERACTION_DISTANCE;
        prms.solidRestDistance = SOLID_REST_DISTANCE;
        Flex.SetParams(solver, ref prms);

        Flex.Buffer particles = CreateBuffer(lib, 2, 4, new float[]
        {
           -0.001f, 0.0f, 0.0f, 1.0f,
            0.001f, 0.0f, 0.0f, 1.0f,
        });
        Flex.SetParticles(solver, particles);

        Flex.Buffer indices = CreateBuffer(lib, 2, 1, new int[] { 0, 1 });
        Flex.Buffer lengths = CreateBuffer(lib, 1, 1, new float[] { SPRING_LENGTH });
        Flex.Buffer stiffness = CreateBuffer(lib, 1, 1, new float[] { 1.0f });
        Flex.SetSprings(solver, indices, lengths, stiffness, 1);

        Flex.SetActiveCount(solver, 2);

        Flex.Buffer active = CreateBuffer(lib, 2, 1, new int[] { 0, 1 });
        Flex.SetActive(solver, active);

        Flex.UpdateSolver(solver, DELTA_T, 1);

        Flex.GetParticles(solver, particles);
        float[] values; ReadBuffer(lib, particles, 2, 4, out values);

        Assert.AreEqual(SPRING_LENGTH, Vector3.Distance(new Vector3(values[0], values[1], values[2]), new Vector3(values[4], values[5], values[6])), 0.001f);

        Flex.FreeBuffer(particles);
        Flex.FreeBuffer(indices);
        Flex.FreeBuffer(lengths);
        Flex.FreeBuffer(stiffness);
        Flex.FreeBuffer(active);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestSetRigidsGetTransforms()
    {
        float DELTA_T = 0.016f;
        float INTERACTION_DISTANCE = 0.5f;
        float SOLID_REST_DISTANCE = 0.2f;
        Vector3 GRAVITY = new Vector3(1, 2, 3);

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 1000;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();
        Flex.GetParams(solver, ref prms);
        prms.radius = INTERACTION_DISTANCE;
        prms.solidRestDistance = SOLID_REST_DISTANCE;
        prms.gravity = GRAVITY;
        Flex.SetParams(solver, ref prms);

        Flex.Buffer particles = CreateBuffer(lib, 8, 4, new float[]
        {
            0.1f, 0.1f,  0.1f, 1.0f,   -0.1f, 0.1f,  0.1f, 1.0f,   -0.1f, -0.1f,  0.1f, 1.0f,   0.1f, -0.1f,  0.1f, 1.0f,
            0.1f, 0.1f, -0.1f, 1.0f,   -0.1f, 0.1f, -0.1f, 1.0f,   -0.1f, -0.1f, -0.1f, 1.0f,   0.1f, -0.1f, -0.1f, 1.0f,
        });
        Flex.SetParticles(solver, particles);

        Flex.Buffer active = CreateBuffer(lib, 8, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        Flex.SetActive(solver, active);

        Flex.Buffer offsets = CreateBuffer(lib, 2, 1, new int[] { 0, 8 });
        Flex.Buffer indices = CreateBuffer(lib, 8, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
        Flex.Buffer positions = CreateBuffer(lib, 8, 3, new float[]
        {
            0.1f, 0.1f,  0.1f,   -0.1f, 0.1f,  0.1f,   -0.1f, -0.1f,  0.1f,   0.1f, -0.1f,  0.1f,
            0.1f, 0.1f, -0.1f,   -0.1f, 0.1f, -0.1f,   -0.1f, -0.1f, -0.1f,   0.1f, -0.1f, -0.1f,
        });
        const float N = 0.57735026918962576450914878050196f; // 1 / sqrt(3)
        Flex.Buffer normals = CreateBuffer(lib, 8, 4, new float[]
        {
            N, N,  N, 0,   -N, N,  N, 0,   -N, -N,  N, 0,   N, -N,  N, 0,
            N, N, -N, 0,   -N, N, -N, 0,   -N, -N, -N, 0,   N, -N, -N, 0,
        });
        Flex.Buffer stiffness = CreateBuffer(lib, 1, 1, new float[] { 1.0f });
        Flex.Buffer thresholds = CreateBuffer(lib, 1, 1, new float[] { 1.0f });
        Flex.Buffer creeps = CreateBuffer(lib, 1, 1, new float[] { 1.0f });
        Flex.Buffer rotations = CreateBuffer(lib, 1, 4, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
        Flex.Buffer translations = CreateBuffer(lib, 1, 3, new float[] { 0.0f, 0.0f, 0.0f });

        Flex.SetRigids(solver, offsets, indices, positions, normals, stiffness, thresholds, creeps, rotations, translations, 1, 8);

        Flex.UpdateSolver(solver, DELTA_T, 1);

        Flex.Buffer nullBuffer = default(Flex.Buffer);
        Flex.GetRigids(solver, nullBuffer, nullBuffer, nullBuffer, nullBuffer, nullBuffer, nullBuffer, nullBuffer, rotations, translations);

        float[] values; ReadBuffer(lib, translations, 1, 3, out values);
        Vector3 expectedPosition = GRAVITY * DELTA_T * DELTA_T;
        Vector3 currentPosition = new Vector3(values[0], values[1], values[2]);
        Assert.AreEqual(expectedPosition.magnitude, currentPosition.magnitude, 0.001f);

        Flex.FreeBuffer(translations);
        Flex.FreeBuffer(rotations);
        Flex.FreeBuffer(creeps);
        Flex.FreeBuffer(thresholds);
        Flex.FreeBuffer(stiffness);
        Flex.FreeBuffer(normals);
        Flex.FreeBuffer(positions);
        Flex.FreeBuffer(indices);
        Flex.FreeBuffer(offsets);

        Flex.FreeBuffer(active);
        Flex.FreeBuffer(particles);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestParamsPlaneCollision()
    {
        float DELTA_T = 0.016f;
        float INTERACTION_DISTANCE = 0.5f;
        float SOLID_REST_DISTANCE = 0.2f;
        Vector3 GRAVITY = new Vector3(0, -9.81f, 0);

        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.SolverDesc desc = default(Flex.SolverDesc);
        Flex.SetSolverDescDefaults(ref desc);
        desc.maxParticles = 1000;
        desc.maxDiffuseParticles = 0;
        Flex.Solver solver = Flex.CreateSolver(lib, ref desc);

        Flex.Params prms = new Flex.Params();
        Flex.GetParams(solver, ref prms);
        prms.radius = INTERACTION_DISTANCE;
        prms.solidRestDistance = SOLID_REST_DISTANCE;
        prms.gravity = GRAVITY;
        prms.plane0 = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
        prms.numPlanes = 1;
        Flex.SetParams(solver, ref prms);

        Flex.Buffer particles = CreateBuffer(lib, 1, 4, new float[] { 0.0f, 2.0f,  0.0f, 1.0f });
        Flex.SetParticles(solver, particles);

        Flex.SetActiveCount(solver, 1);

        Flex.Buffer active = CreateBuffer(lib, 1, 1, new int[] { 0 });
        Flex.CopyDesc cpyDsc; cpyDsc.srcOffset = cpyDsc.dstOffset = 0; cpyDsc.elementCount = 1;
        Flex.SetActive(solver, active, ref cpyDsc);

        for (int i = 0; i < 100; ++i)
            Flex.UpdateSolver(solver, DELTA_T, 1);

        Flex.GetParticles(solver, particles);
        float[] values; ReadBuffer(lib, particles, 1, 4, out values);

        Assert.AreEqual(0, values[1], 0.001f);

        Flex.FreeBuffer(active);
        Flex.FreeBuffer(particles);

        Flex.DestroySolver(solver);
        Flex.Shutdown(lib);
    }

    [Test]
    public void TestAllocFreeBuffer()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);

        Flex.Buffer buffer0 = Flex.AllocBuffer(lib, 1000, 4, Flex.BufferType.Host);
        Flex.Buffer buffer1 = Flex.AllocBuffer(lib, 1000, 4, Flex.BufferType.Device);

        Flex.FreeBuffer(buffer0);
        Flex.FreeBuffer(buffer1);

        Flex.Shutdown(lib);
    }

    [Test]
    public void TestMapUnmapBuffer()
    {
        Flex.Library lib = Flex.Init(Flex.FLEX_VERSION, ErrorCallback);
        Flex.Buffer buffer = Flex.AllocBuffer(lib, 1000, sizeof(float) * 4, Flex.BufferType.Host);

        float[] positions = new float[1000 * 4];
        for (int i = 0; i < positions.Length; i += 4)
        {
            positions[i + 0] = 0.1f * i;
            positions[i + 1] = positions[i + 3] = 0.0f;
            positions[i + 3] = 0.1f;
        }

        IntPtr ptr = Flex.Map(buffer, 0);
        Marshal.Copy(positions, 0, ptr, positions.Length);
        Flex.Unmap(buffer);

        Flex.FreeBuffer(buffer);
        Flex.Shutdown(lib);
    }

    #endregion

    #region Private

    [MonoPInvokeCallback(typeof(Flex.ErrorCallback))]
    static void ErrorCallback(Flex.ErrorSeverity type, IntPtr msg, IntPtr file, int line)
    {
        Assert.Fail("" + type + " - " + Marshal.PtrToStringAnsi(msg) + "\nFile - " + Marshal.PtrToStringAnsi(file) + " (" + line + ")");
    }

    static Flex.Buffer CreateBuffer(Flex.Library lib, int elementCount, int elementSize)
    {
        return Flex.AllocBuffer(lib, elementCount, sizeof(int) * elementSize, Flex.BufferType.Host);
    }

    static Flex.Buffer CreateBuffer(Flex.Library lib, int elementCount, int elementSize, int[] data)
    {
        Flex.Buffer buffer = Flex.AllocBuffer(lib, elementCount, sizeof(int) * elementSize, Flex.BufferType.Host);
        if (data.Length > 0)
        {
            IntPtr ptr = Flex.Map(buffer, Flex.MapFlags.Wait);
            Marshal.Copy(data, 0, ptr, data.Length);
            Flex.Unmap(buffer);
        }
        return buffer;
    }

    static Flex.Buffer CreateBuffer(Flex.Library lib, int elementCount, int elementSize, float[] data)
    {
        Flex.Buffer buffer = Flex.AllocBuffer(lib, elementCount, sizeof(float) * elementSize, Flex.BufferType.Host);
        if (data.Length > 0)
        {
            IntPtr ptr = Flex.Map(buffer, Flex.MapFlags.Wait);
            Marshal.Copy(data, 0, ptr, data.Length);
            Flex.Unmap(buffer);
        }
        return buffer;
    }

    static Flex.Buffer CreateBuffer<Type>(Flex.Library lib, Type[] data)
    {
        int elementCount = data.Length;
        int elementSize = Marshal.SizeOf(data[0]);
        Flex.Buffer buffer = Flex.AllocBuffer(lib, elementCount, elementSize, Flex.BufferType.Host);
        if (data.Length > 0)
        {
            IntPtr ptr = Flex.Map(buffer, Flex.MapFlags.Wait);
            for (int i = 0; i < elementCount; ++i)
                Marshal.StructureToPtr(data[i], new IntPtr(ptr.ToInt64() + i * elementSize), false);
            Flex.Unmap(buffer);
        }
        return buffer;
    }

    static void ReadBuffer(Flex.Library lib, Flex.Buffer buffer, int elementCount, int elementSize, out int[] data)
    {
        data = new int[elementCount * elementSize];
        IntPtr ptr = Flex.Map(buffer, Flex.MapFlags.Wait);
        Marshal.Copy(ptr, data, 0, data.Length);
        Flex.Unmap(buffer);
    }

    static void ReadBuffer(Flex.Library lib, Flex.Buffer buffer, int elementCount, int elementSize, out float[] data)
    {
        data = new float[elementCount * elementSize];
        IntPtr ptr = Flex.Map(buffer, Flex.MapFlags.Wait);
        Marshal.Copy(ptr, data, 0, data.Length);
        Flex.Unmap(buffer);
    }

    #endregion
}
