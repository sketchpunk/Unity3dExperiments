using UnityEngine;
using System.Collections;
using Valve.VR;

/*#############################################################
NOTES:
After playing with how SteamVR handles getting the state of the controller, I tried the SteamVR_Unity_Toolkit.
I like the idea of it being event driven but I did not like the complexity of the toolkit. Its another layer ontop
of the steamVR layer, plus I never liked Unity's way of getting state of controls which SteamVR models itself after.

After looking at all the source I put together something that blends the ideals of both but trying to be as simple
as possible in implementation. First of all, The Controller manager is static, should only exist once in the game like
a singleton design pattern. This manager will handle monitoring the control state then broadcast events as different state
changes. I collect the raw state for each controller like how steamvr does it, handle the data then pass the data with delegates
like the toolkit does it. 

In theory this should simplify handling the controller since it is doing the bare minumum without having two layers of code running
together to perform the same function.
#############################################################*/

#region event delegates
public delegate void ViveTriggerEventHandler(uint index,int state,float axis);
public delegate void ViveButtonEventHandler(uint index,ulong btn,int state);
public delegate void ViveTouchpadEventHandler(uint index,int state,float x, float y);
#endregion

public class ViveControllerMan{
	#region Private Classes
	//tried using a struct but for some reason could not save current state to previous, only works if its a class.
	private class ViveControllerData{
		public uint index;
		public VRControllerState_t curState, prevState;
		public ViveControllerData(uint i){ index = i; curState = new VRControllerState_t(); prevState = new VRControllerState_t(); }
		public void Update(){ prevState = curState; SteamVR.instance.hmd.GetControllerState(index,ref curState); }
	}
	#endregion

	#region Variables and Constants
	public const int STATE_DOWN = 1, STATE_UP = 2, STATE_ACTIVE = 3;

	private const ulong BTN_TRIGGER = SteamVR_Controller.ButtonMask.Trigger,
						BTN_GRIP = SteamVR_Controller.ButtonMask.Grip,
						BTN_TOUCHPAD = SteamVR_Controller.ButtonMask.Touchpad,
						BTN_MENU = SteamVR_Controller.ButtonMask.ApplicationMenu;

	private static ViveControllerData[] mControllers;
	private static bool mIsInitalized = false;
	private static float mTriggerMin = 0.1f;
	#endregion

	#region Event Handlers
	public static event ViveTriggerEventHandler TriggerState;
	public static event ViveButtonEventHandler ButtonState;   //Handle Grip, Menu, Touchpad Click
	public static event ViveTouchpadEventHandler TouchpadState;
	#endregion

	#region Device Events
	private static void OnDeviceConnected(params object[] args){
		var i = (int)args[0];
		var connected = (bool)args[1];

		//Ideally, Should use a generic list and add/remove devices as they connect but eh. Static array is cheaper memory wise.
		if(SteamVR.instance.hmd.GetTrackedDeviceClass((uint)i) == Valve.VR.ETrackedDeviceClass.Controller){
			if(mControllers[0].index == 0) mControllers[0].index = (uint)i;
			else if(mControllers[1].index == 0) mControllers[1].index = (uint)i;  
		}
	}
	#endregion

	#region Manage Controller Data and States
	public static void Init(){
		if(mIsInitalized) return;
		mIsInitalized = true;

		mControllers = new ViveControllerData[]{new ViveControllerData(0),new ViveControllerData(0)};
		SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
	} 

	//rAxis0 = touchpad, rAxis1 = trigger, rAxis2, rAxis3, rAxis4 = unknown;
	//Touchpad Axis is -1 to 1 for x,y. 0,0 is center of pad. When not touching eq 0,0
	//Debug.Log(mCurState.ulButtonPressed + " " + BTN_TRIGGER + " " + (mCurState.ulButtonPressed & BTN_TRIGGER)  + " " + mCurState.rAxis1.x);
	public static void UpdateState(){
		int state = 0;
		ViveControllerData con;

		//Loop through available controllers to trigger out the events for each one.
		for(int i = 0; i < 2; i++){
			con = mControllers[i];
			if(con.index == 0) continue;

			con.Update();

			/*----------------------------------------------------------------
			TRIGGER STATE
			NOTES: When using buttonPressed with the trigger, it only registers when the axis is around 0.5ish but 
			when needing hair trigger state, need to check on rAxis1.x for the current amount of squeeze on the trigger.
			Using Axis to determine state can be used in more gradular ways, like having it anywhere between 0.1-0.9 can be one function when >= 1 be another.
			----------------------------------------------------------------*/
			//state = (((mCurState.ulButtonPressed & BTN_TRIGGER) != 0)? 1 : 0) | (((mPrevState.ulButtonPressed & BTN_TRIGGER) != 0)? 2 : 0); //Down=1,Up=2,StillDown=3
			state = ((con.curState.rAxis1.x >= mTriggerMin)? 1 : 0) | ((con.prevState.rAxis1.x >= mTriggerMin)? 2 : 0) ; //Down=1,Up=2,StillDown=3
			switch(state){
				case 1: Debug.Log("Trigger Down"); break;
				case 2: Debug.Log("Trigger Up"); break;
				//case 3: Debug.Log("Trigger Still Pressed"); break;
			}
			if((state == 1 || state == 2) && TriggerState != null) TriggerState(con.index,state,con.curState.rAxis1.x);

			/*----------------------------------------------------------------
			GRIP STATE
			----------------------------------------------------------------*/
			state = (((con.curState.ulButtonPressed & BTN_GRIP) != 0)? 1 : 0) | (((con.prevState.ulButtonPressed & BTN_GRIP) != 0)? 2 : 0);
			switch(state){
				case 1: Debug.Log("Grip Squeeze"); break;
				case 2: Debug.Log("Grip Release"); break;
				//case 3: Debug.Log("Grip Still Squeezing"); break;
			}
			if((state == 1 || state == 2) && ButtonState != null) ButtonState(con.index,BTN_GRIP,state);

			/*----------------------------------------------------------------
			TOUCHPAD STATE
			----------------------------------------------------------------*/
			state = (((con.curState.ulButtonPressed & BTN_TOUCHPAD) != 0)? 1 : 0) | (((con.prevState.ulButtonPressed & BTN_TOUCHPAD) != 0)? 2 : 0);
			switch(state){
				case 1: Debug.Log("Touchpad Click Down"); break;
				case 2: Debug.Log("Touchpad Click Up"); break;
				//case 3: Debug.Log("Touchpad Still click down"); break;
			}
			if((state == 1 || state == 2) && ButtonState != null) ButtonState(con.index,BTN_TOUCHPAD,state);

			state = (((con.curState.ulButtonTouched & BTN_TOUCHPAD) != 0)? 1 : 0) | (((con.prevState.ulButtonTouched & BTN_TOUCHPAD) != 0)? 2 : 0);
			switch(state){
				case 1: Debug.Log("Touchpad Start Touch"); break;
				case 2: Debug.Log("Touchpad End Touch"); break;
				//case 3: Debug.Log("Touchpad still touching"); break;
			}

			if(state != 0 && TouchpadState != null) TouchpadState(con.index,state,con.curState.rAxis0.x,con.curState.rAxis0.y);

			/*----------------------------------------------------------------
			MENU BUTTON STATE
			----------------------------------------------------------------*/
			state = (((con.curState.ulButtonPressed & BTN_MENU) != 0)? 1 : 0) | (((con.prevState.ulButtonPressed & BTN_MENU) != 0)? 2 : 0);
			switch(state){
				case 1: Debug.Log("Menu Down"); break;
				case 2: Debug.Log("Menu Up"); break;
				//case 3: Debug.Log("Menu Still Down"); break;
			}
			if((state == 1 || state == 2) && ButtonState != null) ButtonState(con.index,BTN_MENU,state);
		}
	}
	#endregion
}
