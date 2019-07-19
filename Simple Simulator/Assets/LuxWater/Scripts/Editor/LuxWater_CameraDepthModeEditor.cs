using UnityEngine;
using System.Collections;
using UnityEditor;

namespace LuxWater {

	[CustomEditor(typeof(LuxWater_CameraDepthMode))]
	public class LuxWater_CameraDepthModeEditor : Editor {
	    public override void OnInspectorGUI() {
	        LuxWater_CameraDepthMode myTarget = (LuxWater_CameraDepthMode)target;
	        DrawDefaultInspector();
	        if(myTarget.GrabDepthTexture) {
	        	//var testMat = new Material( Shader.Find("Custom/metalDepth"));
	        	//bool isCorrect = testMat.IsKeywordEnabled("LUXWATERMETALDEFERRED");
	        	if(myTarget.ShowShaderWarning) {
	        		EditorGUILayout.HelpBox("Please make sure that you have changed the WaterSurface Shader according to the docs.", MessageType.Warning);
	        		if (GUILayout.Button("Hide Messsgae")) {
	        			myTarget.ShowShaderWarning = false;
	        		}
	        	}
	        }
	    }
	}
}