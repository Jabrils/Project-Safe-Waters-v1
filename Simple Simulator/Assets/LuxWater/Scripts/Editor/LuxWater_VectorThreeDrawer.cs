using UnityEngine;
using System.Collections;
using UnityEditor;

public class LuxWaterVectorThreeDrawer : MaterialPropertyDrawer {

	override public void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {

	// 	Needed since Unity 2019
		EditorGUIUtility.labelWidth = 0;

		Vector4 vec4value = prop.vectorValue;
		Vector3 vec3value = new Vector3(vec4value.x, vec4value.y, vec4value.z);

		GUILayout.Space(-12);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(label);
				GUILayout.Space(-8);
				vec3value = EditorGUILayout.Vector3Field ("", vec3value);
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		GUILayout.Space(2);
		if (EditorGUI.EndChangeCheck ()) {
			vec4value.x = vec3value.x;
			vec4value.y = vec3value.y;
			vec4value.z = vec3value.z;
			prop.vectorValue = vec4value;
		}

	}
	//public override float GetPropertyHeight (MaterialProperty prop, string label, MaterialEditor editor) {
	//	return base.GetPropertyHeight (prop, label, editor);
	//}
}