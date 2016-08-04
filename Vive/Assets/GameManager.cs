using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour{
	void Awake(){
		//The Idea is to have a single object that ties together sytems, a game manager.
		ViveControllerMan.Init();
		ViveControllerMan.TriggerState += new ViveTriggerEventHandler(ViveTriggerEvent);
		ViveControllerMan.GripState += new ViveButtonEventHandler(ViveGripEvent);
	}

	private void ViveTriggerEvent(uint index,int state,float axis){
		Debug.Log("Trigger " + state + " axis " + axis);
	}

	private void ViveGripEvent(uint index,int state){
		Debug.Log("Grip " + state);
	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update(){
		ViveControllerMan.UpdateState();
	}

}
