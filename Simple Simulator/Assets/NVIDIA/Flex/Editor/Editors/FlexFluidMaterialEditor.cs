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

using UnityEditor;
using UnityEngine;

public class FlexFluidMaterialEditor : MaterialEditor
{
    MaterialProperty m_fluidColor;

    public override void OnEnable()
    {
        base.OnEnable();
        m_fluidColor = GetMaterialProperty(targets, "_FluidColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ColorProperty(m_fluidColor, "Fluid Color");

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }

        // render the default inspector
        //base.OnInspectorGUI();

        // if we are not visible... return
        //if (!isVisible)
        //    return;


        //// get the current keywords from the material
        //Material targetMat = target as Material;
        //string[] keyWords = targetMat.shaderKeywords;

        //// see if redify is set, then show a checkbox
        //bool redify = keyWords.Contains("REDIFY_ON");
        //EditorGUI.BeginChangeCheck();
        //redify = EditorGUILayout.Toggle("Redify material", redify);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    // if the checkbox is changed, reset the shader keywords
        //    var keywords = new List<string> { redify ? "REDIFY_ON" : "REDIFY_OFF" };
        //    targetMat.shaderKeywords = keywords.ToArray();
        //    EditorUtility.SetDirty(targetMat);
        //}
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {

    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
    }
}
