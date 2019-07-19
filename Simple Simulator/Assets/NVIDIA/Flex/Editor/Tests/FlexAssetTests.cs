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

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NVIDIA.Flex;

public class FlexAssetTests
{
    [Test]
    public void TestFlexArrayAssetCreateDestroy()
    {
        FlexArrayAsset asset = ScriptableObject.CreateInstance<FlexArrayAsset>();

        //Vector4[] particles = new Vector4[] { new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), new Vector4(0, 0, 1, 1) };

        //asset.particles = particles;

        //Vector4[] checkParticles = asset.particles;

        //for (int i = 0; i < particles.Length; ++i)
        //    Assert.AreEqual(particles[i], checkParticles[i]);

        Object.DestroyImmediate(asset);
    }

    [Test]
    public void TestFlexArrayAssetBuildFromMesh()
    {
        Vector3[] vertices = new Vector3[] { new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, -1, -1) };
        int[] triangles = new int[] { 0, 2, 1, 0, 1, 3, 0, 3, 2, 1, 2, 3 };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        FlexArrayAsset asset = ScriptableObject.CreateInstance<FlexArrayAsset>();

        Assert.IsTrue(asset.handle);
        Assert.AreEqual(0, asset.particles.Length);

        asset.boundaryMesh = mesh;
        asset.meshLocalScale = Vector3.one;
        asset.meshExpansion = 0;
        asset.particleSpacing = 0.1f;

        asset.Rebuild();

        Assert.IsTrue(asset.handle);
        Assert.AreEqual(2680, asset.particles.Length);

        Object.DestroyImmediate(asset);
        Object.DestroyImmediate(mesh);
    }

    [Test]
    public void TestFlexSolidAssetBuildFromMesh()
    {
        var vertices = new Vector3[] { new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1), new Vector3(1, -1, -1) };
        var triangles = new int[] { 0, 2, 1, 0, 1, 3, 0, 3, 2, 1, 2, 3 };

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        var asset = ScriptableObject.CreateInstance<FlexSolidAsset>();

        Assert.IsTrue(asset.handle);
        Assert.AreEqual(0, asset.particles.Length);

        asset.boundaryMesh = mesh;
        asset.meshLocalScale = Vector3.one;
        asset.meshExpansion = 0;
        asset.particleSpacing = 0.1f;

        asset.Rebuild();

        Assert.IsTrue(asset.handle);
        Assert.AreEqual(2680, asset.particles.Length);
        Assert.AreEqual(1, asset.shapeCenters.Length);

        Object.DestroyImmediate(asset);
        Object.DestroyImmediate(mesh);
    }
}
