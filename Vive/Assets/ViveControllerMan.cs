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
public delegate void ViveButtonEventHandler(uint index,int state);
public delegate void ViveTouchpadEventHandler(uint index,int state,float x, float y);
public delegate void ViveTouchpadClickEventHandler(uint index,int state,float x, float y);
#endregion

public class ViveControllerMan{
	#region Private Classes
	//tried using a struct but for some reason could not save current state to previous, only works if its a class.
	private class ViveControllerData{
		public uint index;
		public VRControllerState_t curState, prevState;
		public ViveControllerData(uint i){ index = i; curState = new VRControllerState_t(); prevState = new VRControllerState_t(); }
		public void Update(){ 
			prevState = curState;
			SteamVR.instance.hmd.GetControllerState(index,ref curState); //NOTE, SteamVR uses GetControllerStateWithPose, but at the moment I have no need to track position of controllers with event handling.
		}
	}
	#endregion

	#region Variables and Constants
	public const int STATE_DOWN = 1, STATE_UP = 2, STATE_ACTIVE = 3; //Something to use to compare states in the events

	//Variables names are long, to make the code more readable I made a copy of the values
	private const ulong BTN_TRIGGER = SteamVR_Controller.ButtonMask.Trigger,
						BTN_GRIP = SteamVR_Controller.ButtonMask.Grip,
						BTN_TOUCHPAD = SteamVR_Controller.ButtonMask.Touchpad,
						BTN_MENU = SteamVR_Controller.ButtonMask.ApplicationMenu;

	private static ViveControllerData[] mControllers;	//Hold a list of controllers, this manager should track 2 controllers if active.
	private static bool mIsInitalized = false;			//Just prevent this object from initializing more then once
	private static float mTriggerMin = 0.1f;			//Minimal value the trigger x axis should be to be considered as starting to press it.
	private static float mDoubleTapMax = 0.18f;			//Time between Touch Up Events, this gives a simple example of registering a Double Tap
	private static float mLastTapTime = 0;				//Keep track of the time between Touch Up, needed to determine Double Tap
	#endregion

	#region Event Handlers
	public static event ViveTriggerEventHandler TriggerState;
	public static event ViveButtonEventHandler GripState, MenuState, DoubleTap;
	public static event ViveTouchpadEventHandler TouchpadState;
	public static event ViveTouchpadClickEventHandler TouchpadClickState;
	#endregion

	#region Device Events
	private static void OnDeviceConnected(params object[] args){
		var i = (uint) ((int)args[0]); //Its an int originally But steamVR uses uint when you pass it. So have to unbox THEN cast it to what steamvr functions use. Valve, fix it :)
		//var connected = (bool)args[1]; //TODO, Should handle devices disconnecting and reconnecting and removing it off the array to stop tracking

		//Ideally, Should use a generic list and add/remove devices as they connect but eh. Static array is cheaper memory wise.
		if(SteamVR.instance.hmd.GetTrackedDeviceClass(i) == Valve.VR.ETrackedDeviceClass.Controller){
			if(mControllers[0].index == 0) mControllers[0].index = i;
			else if(mControllers[1].index == 0) mControllers[1].index = i;  
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
			if(mControllers[i].index == 0) continue;
			con = mControllers[i];
			con.Update();

			/*----------------------------------------------------------------
			TRIGGER STATE
			NOTES: When using buttonPressed with the trigger, it only registers when the axis is around 0.5ish but 
			when needing hair trigger state, need to check on rAxis1.x for the current amount of squeeze on the trigger.
			Using Axis to determine state can be used in more gradular ways, like having it anywhere between 0.1-0.9 can be one function when >= 1 be another.
			----------------------------------------------------------------*/
			if(TriggerState != null){ //Only check if handlers exist
				//state = (((mCurState.ulButtonPressed & BTN_TRIGGER) != 0)? 1 : 0) | (((mPrevState.ulButtonPressed & BTN_TRIGGER) != 0)? 2 : 0); //Down=1,Up=2,StillDown=3
				state = ((con.curState.rAxis1.x >= mTriggerMin)? 1 : 0) | ((con.prevState.rAxis1.x >= mTriggerMin)? 2 : 0) ; //Down=1,Up=2,StillDown=3
				if(state % 3 != 0) TriggerState(con.index,state,con.curState.rAxis1.x); //TODO: At some point I need a more analog event, right now its treating trigger in a binary way.
			}

			/*----------------------------------------------------------------
			GRIP STATE */
			if(GripState != null){ //Only check if handlers exist
				state = (((con.curState.ulButtonPressed & BTN_GRIP) != 0)? 1 : 0) | (((con.prevState.ulButtonPressed & BTN_GRIP) != 0)? 2 : 0);
				if(state % 3 != 0) GripState(con.index,state);
			}

			/*----------------------------------------------------------------
			MENU BUTTON STATE */
			if(MenuState != null){
				state = (((con.curState.ulButtonPressed & BTN_MENU) != 0)? 1 : 0) | (((con.prevState.ulButtonPressed & BTN_MENU) != 0)? 2 : 0);
				if(state % 3 != 0) MenuState(con.index,state);
			}

			/*----------------------------------------------------------------
			TOUCHPAD CLICK STATE */
			if(TouchpadClickState != null){
				state = (((con.curState.ulButtonPressed & BTN_TOUCHPAD) != 0)? 1 : 0) | (((con.prevState.ulButtonPressed & BTN_TOUCHPAD) != 0)? 2 : 0);
				if(state % 3 != 0) TouchpadClickState(con.index,state,con.curState.rAxis0.x,con.curState.rAxis0.y);
			}

			/*----------------------------------------------------------------
			TOUCHPAD TOUCH STATE */
			if(TouchpadState != null || DoubleTap != null){
				state = (((con.curState.ulButtonTouched & BTN_TOUCHPAD) != 0)? 1 : 0) | (((con.prevState.ulButtonTouched & BTN_TOUCHPAD) != 0)? 2 : 0);
				if(state != 0 && TouchpadState != null) TouchpadState(con.index,state,con.curState.rAxis0.x,con.curState.rAxis0.y);

				/*NOTES : Touchpad opens up possibility of specific gestures like Double Tap, Long Tap, and swipping.
				For now I had the idea of double tapping the pad would bring up a menu. Other geatures I'll build in when needed.*/
				if(DoubleTap != null && state == 2){ //Manage Double Tap Event
					if(Time.time-mLastTapTime <= mDoubleTapMax) DoubleTap(con.index,3);
					mLastTapTime = Time.time;
				}
			}
		}
	}

	#endregion
}
