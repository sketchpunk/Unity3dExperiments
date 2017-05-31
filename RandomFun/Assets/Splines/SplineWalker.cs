using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SplineWalkerMode{ Once, Loop, PingPong };

public class SplineWalker : MonoBehaviour {
	public BezierSpline spline;
	public float duration = 5f;

	public bool lookForward = true;

	public SplineWalkerMode mode = SplineWalkerMode.Loop;

	private float progress;

	private bool goingForward = true;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(goingForward){
			progress += Time.deltaTime / duration;
			if(progress > 1f){
				switch(mode){
					case SplineWalkerMode.Once:		progress = 1f; break;
					case SplineWalkerMode.Loop:		progress -= 1f; break;
					case SplineWalkerMode.PingPong:	progress = 2f - progress; goingForward = false; break;
				}
			}
		}else{
			progress -= Time.deltaTime / duration;
			if(progress < 0f){
				progress = -progress;
				goingForward = true;
			}
		}

		Vector3 pos = spline.GetPoint(progress);
		transform.localPosition = pos;
		if(lookForward) transform.LookAt(pos + spline.GetDirection(progress));
	}
}
