using UnityEngine;
using System.Collections;
using UnityEditor;

public class LuxWaterVectorFourDrawer : MaterialPropertyDrawer {

	override public void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
	
	// 	Needed since Unity 2019
		EditorGUIUtility.labelWidth = 0;

		Vector4 vec4value = prop.vectorValue;

		GUILayout.Space(-16);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(label);
				GUILayout.Space(-8);
				vec4value = EditorGUILayout.Vector4Field ("", vec4value);
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		GUILayout.Space(-16);
		if (EditorGUI.EndChangeCheck ()) {
			prop.vectorValue = vec4value;
		}

	}
	//public override float GetPropertyHeight (MaterialProperty prop, string label, MaterialEditor editor) {
	//	return base.GetPropertyHeight (prop, label, editor);
	//}
}