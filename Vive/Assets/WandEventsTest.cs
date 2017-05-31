using UnityEngine;
using System.Collections;

public class WandController : MonoBehaviour {
	private SteamVR_TrackedObject mTrackedObj;
	private SteamVR_Controller.Device mDevice;

	// Use this for initialization
	void Start (){
		mTrackedObj = GetComponent<SteamVR_TrackedObject>();
		mDevice = SteamVR_Controller.Input((int)mTrackedObj.index);
	}

	// Update is called once per frame
	void Update (){
		/* 
		if(mDevice.GetHairTrigger()){
			Vector2 trigAxis = mDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
			Debug.Log("Hair Trigger " + trigAxis.x.ToString("F1"));
		}
		*/

		//if(mDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) Debug.Log("Trigger Press");
		//if(mDevice.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) Debug.Log("Trigger Press Down");
		//if(mDevice.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) Debug.Log("Trigger Press Up");

		//if(mDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip)) Debug.Log("Grip Press");

	}
}
