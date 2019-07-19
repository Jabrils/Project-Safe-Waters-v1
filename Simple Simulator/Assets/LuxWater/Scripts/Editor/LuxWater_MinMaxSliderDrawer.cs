using UnityEngine;
using System.Collections;
using UnityEditor;

public class LuxWater_MinMaxSliderDrawer : MaterialPropertyDrawer {

	override public void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
		Vector4 vec2value = prop.vectorValue;

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(label);
				EditorGUILayout.MinMaxSlider ("", ref vec2value.x, ref vec2value.y, 0.0f, 1.0f);
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		GUILayout.Space(2);
		if (EditorGUI.EndChangeCheck ()) {
			prop.vectorValue = vec2value;
		}

	}
}
