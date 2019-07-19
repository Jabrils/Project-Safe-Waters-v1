using UnityEngine;
using System.Collections;
using UnityEditor;

public class LuxWaterGFDrawer : MaterialPropertyDrawer {

	override public void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {

	// 	Needed since Unity 2019
		EditorGUIUtility.labelWidth = 0;

		float floatvalue = prop.floatValue;
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("        Global Factor");
				GUILayout.Space(-8);
				floatvalue = EditorGUILayout.FloatField ("", floatvalue);
			EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		GUILayout.Space(8);
		if (EditorGUI.EndChangeCheck ()) {
			prop.floatValue = floatvalue;
		}

	}
	//public override float GetPropertyHeight (MaterialProperty prop, string label, MaterialEditor editor) {
	//	return base.GetPropertyHeight (prop, label, editor);
	//}
}