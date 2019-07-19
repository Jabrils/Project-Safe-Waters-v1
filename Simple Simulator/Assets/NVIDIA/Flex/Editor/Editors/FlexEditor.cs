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

using NVIDIA.Flex;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FlexEditor
{
    #region Constants

    const string MENU_ROOT = "NVIDIA/Flex/";

    #endregion

    #region Menus

    [MenuItem(MENU_ROOT + "Create Asset/Flex Container", false, 100)]
    [MenuItem("Assets/Create/NVIDIA/Flex/Flex Container", false, 100)]
    static void CreateFlexContainerAsset()
    {
        CreateAsset<FlexContainer>("New FlexContainer");
    }

    [MenuItem(MENU_ROOT + "Create Asset/Flex Array Asset", false, 110)]
    [MenuItem("Assets/Create/NVIDIA/Flex/Flex Array Asset", false, 110)]
    static void CreateFlexArrayAssetAsset()
    {
        CreateAsset<FlexArrayAsset>("New FlexArrayAsset");
    }

    [MenuItem(MENU_ROOT + "Create Asset/Flex Source Asset", false, 120)]
    [MenuItem("Assets/Create/NVIDIA/Flex/Flex Source Asset", false, 120)]
    static void CreateFlexSourceAssetAsset()
    {
        CreateAsset<FlexSourceAsset>("New FlexSourceAsset");
    }

    [MenuItem(MENU_ROOT + "Create Asset/Flex Solid Asset", false, 130)]
    [MenuItem("Assets/Create/NVIDIA/Flex/Flex Solid Asset", false, 130)]
    static void CreateFlexSolidAssetAsset()
    {
        CreateAsset<FlexSolidAsset>("New FlexSolidAsset");
    }

    [MenuItem(MENU_ROOT + "Create Asset/Flex Soft Asset", false, 140)]
    [MenuItem("Assets/Create/NVIDIA/Flex/Flex Soft Asset", false, 140)]
    static void CreateFlexSoftAssetAsset()
    {
        CreateAsset<FlexSoftAsset>("New FlexSoftAsset");
    }

    [MenuItem(MENU_ROOT + "Create Asset/Flex Cloth Asset", false, 150)]
    [MenuItem("Assets/Create/NVIDIA/Flex/Flex Cloth Asset", false, 150)]
    static void CreateFlexClothAssetAsset()
    {
        CreateAsset<FlexClothAsset>("New FlexClothAsset");
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Array Actor", false, 100)]
    static void AddFlexArrayActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexArrayActor>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Array Actor", true)]
    static bool CanAddFlexArrayActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() == null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Source Actor", false, 110)]
    static void AddFlexSourceActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexSourceActor>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Source Actor", true)]
    static bool CanAddFlexSourceActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() == null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Solid Actor", false, 120)]
    static void AddFlexSolidActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexSolidActor>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Solid Actor", true)]
    static bool CanAddFlexSolidActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() == null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Soft Actor", false, 130)]
    static void AddFlexSoftActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexSoftActor>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Soft Actor", true)]
    static bool CanAddFlexSoftActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() == null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Cloth Actor", false, 140)]
    static void AddFlexClothActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexClothActor>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Cloth Actor", true)]
    static bool CanAddFlexClothActorComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() == null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Particle Controller", false, 150)]
    static void AddFlexParticleControllerComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexParticleController>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Particle Controller", true)]
    static bool CanAddFlexParticleControllerComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() != null
                      && o.GetComponent<ParticleSystem>() != null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Cloth Deformation", false, 160)]
    static void AddFlexClothDeformationComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexClothDeformation>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Cloth Deformation", true)]
    static bool CanAddFlexClothDeformationComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexClothActor>() != null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Soft Skinning", false, 170)]
    static void AddFlexSoftSkinningComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexSoftSkinning>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Soft Skinning", true)]
    static bool CanAddFlexSoftSkinningComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexSoftActor>() != null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem(MENU_ROOT + "Add Component/Flex Fluid Renderer", false, 180)]
    static void AddFlexFluidRendererComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            o.AddComponent<FlexFluidRenderer>();
        }
    }
    [MenuItem(MENU_ROOT + "Add Component/Flex Fluid Renderer", true)]
    static bool CanAddFlexFluidRendererComponent()
    {
        foreach (GameObject o in Selection.gameObjects)
        {
            bool valid = o.GetComponent<FlexActor>() != null;
            if (!valid) return false;
        }
        return Selection.gameObjects.Length > 0;
    }

    #endregion

    #region Methods

    public static T CreateAsset<T>(string _name = "") where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "") path = "Assets";
        else if (Path.GetExtension(path) != "") path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

        if (_name == "") _name = typeof(T).ToString();
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + _name + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        return asset;
    }

    #endregion
}
