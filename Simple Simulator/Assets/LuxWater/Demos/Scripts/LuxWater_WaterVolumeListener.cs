using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuxWater_WaterVolumeListener : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
		LuxWater.LuxWater_WaterVolume.OnEnterWaterVolume += Enter;
		LuxWater.LuxWater_WaterVolume.OnExitWaterVolume += Exit;
	}

	void OnDisable () {
		LuxWater.LuxWater_WaterVolume.OnEnterWaterVolume -= Enter;
		LuxWater.LuxWater_WaterVolume.OnExitWaterVolume -= Exit;
	}
	

	void Enter() {
		Debug.Log("Entered.");
	}

	void Exit() {
		Debug.Log("Exited.");
	}
}
