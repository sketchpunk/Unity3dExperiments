using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//http://catlikecoding.com/unity/tutorials/curves-and-splines/

public static class Bezier{
	public static Vector3 GetPoint(Vector3 p0,Vector3 p1, Vector3 p2, float t){
		//return Vector3.Lerp(Vector3.Lerp(p0,p1,t),Vector3.Lerp(p1,p2,t),t);
		t = Mathf.Clamp01(t);
		float tInvert = 1f - t;
		return tInvert * tInvert * p0 + 2f * tInvert * t * p1 + t * t * p2;
	}

	public static Vector3 GetFirstDerivative(Vector3 p0,Vector3 p1, Vector3 p2, float t){
		return 2f * (1f - t) * (p1-p0) + 2f * t * (p2 - p1);
	}

	public static Vector3 GetPoint(Vector3 p0,Vector3 p1, Vector3 p2, Vector3 p3,float t){
		//return Vector3.Lerp(Vector3.Lerp(p0,p1,t),Vector3.Lerp(p1,p2,t),t);
		t = Mathf.Clamp01(t);
		float tInvert = 1f - t;
		return tInvert * tInvert * tInvert * p0 +
				3f * tInvert * tInvert * t * p1 +
				3f * tInvert * t * t * p2 + 
				t * t * t * p3;
	}

	public static Vector3 GetFirstDerivative(Vector3 p0,Vector3 p1, Vector3 p2, Vector3 p3, float t){
		t = Mathf.Clamp01(t);
		float tInvert = 1f - t;
		return	3f * tInvert * tInvert * (p1-p0) +
				6f * tInvert * t * (p2-p1) +
				3f * t * t * (p3 - p2);
	}
}

public class BezierCurve : MonoBehaviour {
	public Vector3[] points;

	public void Reset(){
		points = new Vector3[]{
			new Vector3(1f,0f,0f),
			new Vector3(2f,0f,0f),
			new Vector3(3f,0f,0f),
			new Vector3(4f,0f,0f)
		};
	}

	public Vector3 GetPoint(float t){
		if(points.Length == 3) return transform.TransformPoint(Bezier.GetPoint(points[0],points[1],points[2],t));
		else return transform.TransformPoint(Bezier.GetPoint(points[0],points[1],points[2],points[3],t));
	}

	public Vector3 GetVelocity(float t){
		Vector3 p = (points.Length == 3)?
			Bezier.GetFirstDerivative(points[0],points[1],points[2],t) :
			Bezier.GetFirstDerivative(points[0],points[1],points[2],points[3],t);

		return transform.TransformPoint(p) - transform.position;
	}

	public Vector3 GetDirection(float t){ return GetVelocity(t).normalized; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
