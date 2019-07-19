using UnityEngine;
using UnityEditor;
using System;

public class LuxWaterMaterialEditor : ShaderGUI {
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        
        Material targetMat = materialEditor.target as Material;

    //  Render the default gui
        base.OnGUI(materialEditor, properties);
            
    //  Calculate all needed values
        Vector4 speeds = targetMat.GetVector("_BumpSpeed");
        Vector4 rotations = targetMat.GetVector("_BumpRotation");

        Vector3 speed0 = new Vector3 (speeds.x, 0.0f, 0.0f) * speeds.w;
        speed0 = Quaternion.AngleAxis(rotations.x + rotations.w, Vector3.up) * speed0;
        Vector3 speed1 = new Vector3 (speeds.y, 0.0f, 0.0f) * speeds.w;
        speed1 = Quaternion.AngleAxis(rotations.y + rotations.w, Vector3.up) * speed1;
        Vector3 speed2 = new Vector3 (speeds.z, 0.0f, 0.0f) * speeds.w;
        speed2 = Quaternion.AngleAxis(rotations.z + rotations.w, Vector3.up) * speed2;

        targetMat.SetVector("_FinalBumpSpeed01", new Vector4(speed0.x, speed0.z, speed1.x, speed1.z) );
        targetMat.SetVector("_FinalBumpSpeed23", new Vector2(speed2.x, speed2.z) );


    //  Gerstner Waves
        Vector4 frequencies = targetMat.GetVector("_GFrequency");
        float multiplier = targetMat.GetFloat("_GGlobalFrequency");
        targetMat.SetVector("_GFinalFrequency", frequencies * multiplier);

        speeds = targetMat.GetVector("_GSpeed");
        multiplier = targetMat.GetFloat("_GGlobalSpeed");
        targetMat.SetVector("_GFinalSpeed", speeds * multiplier);

        rotations = targetMat.GetVector("_GRotation");
        speeds = targetMat.GetVector("_GSpeed");
        float globalRotation = targetMat.GetFloat("_GGlobalRotation");
        speed0 = Quaternion.AngleAxis(rotations.x + globalRotation, Vector3.up) * new Vector3 (1, 0, 0);
        speed1 = Quaternion.AngleAxis(rotations.y + globalRotation, Vector3.up) * new Vector3 (1, 0, 0);
        speed2 = Quaternion.AngleAxis(rotations.z + globalRotation, Vector3.up) * new Vector3 (1, 0, 0);
        Vector3 speed3 = Quaternion.AngleAxis(rotations.w + globalRotation, Vector3.up) * new Vector3 (1, 0, 0);

        targetMat.SetVector("_GDirectionAB", new Vector4(speed0.x, speed0.z, speed1.x, speed1.z) );
        targetMat.SetVector("_GDirectionCD", new Vector4(speed2.x, speed2.z, speed3.x, speed3.z) );
    }
}