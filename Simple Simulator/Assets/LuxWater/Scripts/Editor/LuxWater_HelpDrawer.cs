using UnityEngine;
using System.Collections;
using UnityEditor;

public class LuxWaterHelpDrawer : MaterialPropertyDrawer
{
    override public void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        float brightness = 1.45f;
        if (!EditorGUIUtility.isProSkin) {
            brightness = 1.0f;
        }
        Color HelpCol = new Color(0.30f * brightness, 0.47f * brightness, 1.0f * brightness, 1.0f * brightness);
        GUIStyle hStyle = GUI.skin.GetStyle("HelpBox");
        Color tCol = hStyle.normal.textColor;
        hStyle.normal.textColor = HelpCol;
        RectOffset tPadd = hStyle.padding;
        hStyle.padding = new RectOffset(18, 0, 1, 3);

        Color col = GUI.contentColor;
        Color colbg = GUI.backgroundColor;

        GUI.contentColor = HelpCol;
        GUI.backgroundColor = Color.clear;

        GUILayout.Space(-4);
        EditorGUILayout.TextArea(label, hStyle);

    //  Reset
        GUI.contentColor = col;
        GUI.backgroundColor = colbg;
        hStyle.normal.textColor = tCol;
        hStyle.padding = tPadd;
    }
}