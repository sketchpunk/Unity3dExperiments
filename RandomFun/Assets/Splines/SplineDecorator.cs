using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineDecorator : MonoBehaviour {
	public BezierSpline spline;
	public int frequency;
	public bool lookForward = true;
	public Transform[] items;

	private void Awake(){
		if(frequency <=0 || items == null || items.Length == 0) return;

		//float stepSize = 1f / (frequency * items.Length);
		float stepSize = frequency * items.Length;
		if(spline.Loop || stepSize == 1) stepSize = 1f/stepSize;
		else stepSize = 1f / (stepSize-1);

		for(int p=0, f=0; f < frequency; f++ ){
			for(int i=0; i < items.Length; i++, p++){
				Transform item = Instantiate(items[i]) as Transform;
				Vector3 pos = spline.GetPoint(p*stepSize);
				item.transform.localPosition = pos;

				if(lookForward) item.transform.LookAt(pos + spline.GetDirection(p * stepSize));
				item.transform.parent = transform;
			}
		}
	}
}
