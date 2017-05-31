using UnityEngine;
using System;

public enum BezierControlPointMode{ Free,Aligned,Mirrored }

public class BezierSpline : MonoBehaviour {
	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;

	public bool Loop{
		get{ return loop; }
		set{
			loop = value;
			if(value == true){
				modes[modes.Length-1] = modes[0];
				SetControlPoint(0,points[0]);
			}
		}
	}


	public int ControlPointCount{ get{ return points.Length; } }

	public Vector3 GetControlPoint(int i){
		return points[i];
	}

	public void SetControlPoint(int i, Vector3 p){
		//If moving a main point, move its control points as well.
		if(i % 3 == 0){
			Vector3 delta = p - points[i];

			if(loop){
				if(i == 0){
					points[1] += delta; //Move next point
					points[points.Length-2] += delta; //Move second to last point
					points[points.Length-1] = p;	//Make last point eq first
				}else if(i == points.Length-1){
					points[0] = p; //Make first eq last
					points[1] += delta;	//move control
					points[i-1] += delta;
				}else{
					points[i-1] += delta;
					points[i+1] += delta;
				}
			}else{
				if(i > 0) points[i-1] += delta;
				if(i + 1 < points.Length) points[i+1] += delta;
			}
		}

		points[i] = p;
		EnforceMode(i);
	}

	public void Reset(){
		points = new Vector3[]{
			new Vector3(1f,0f,0f),
			new Vector3(2f,0f,0f),
			new Vector3(3f,0f,0f),
			new Vector3(4f,0f,0f)
		};

		modes = new BezierControlPointMode[]{ BezierControlPointMode.Free, BezierControlPointMode.Free };
	}

	public void AddCurve(){
		Vector3 p = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);

		p.x += 1f;
		points[points.Length - 3] = p;
		p.x += 1f;
		points[points.Length - 2] = p;
		p.x += 1f;
		points[points.Length - 1] = p;

		Array.Resize(ref modes, modes.Length+1);
		modes[modes.Length-1] = modes[modes.Length-2];
		EnforceMode(points.Length-4);

		if(loop){
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

	public Vector3 GetPoint(float t){
		int i;
		if( t >= 1f){
			t = 1f;
			i = points.Length - 4;
		}else{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}

		return transform.TransformPoint(Bezier.GetPoint(points[i],points[i+1],points[i+2],points[i+3],t));
	}

	public Vector3 GetVelocity(float t){
		int i;
		if( t >= 1f){
			t = 1f;
			i = points.Length - 4;
		}else{
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}

		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i],points[i+1],points[i+2],points[i+3],t)) - transform.position;
	}

	public BezierControlPointMode GetControlPointMode(int i){
		return modes[(i+1)/3];
	}

	public void SetControlPointMode(int i, BezierControlPointMode mode){
		int modeIndex = (i + 1) / 3;
		modes[modeIndex] = mode;

		if(loop){
			if(modeIndex == 0) modes[modes.Length - 1] = mode;
			else if(modeIndex == modes.Length - 1) modes[0] = mode;
		}

		EnforceMode(i);
	}

	private void EnforceMode(int i){
		int modeIndex = (i + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if(mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length-1)) return;

		int middleIndex = modeIndex * 3,
			fixedIndex,enforcedIndex;

		if(i <= middleIndex){
			fixedIndex = middleIndex - 1;
			if(fixedIndex < 0) fixedIndex = points.Length - 2;

			enforcedIndex = middleIndex + 1;
			if(enforcedIndex >= points.Length) enforcedIndex = 1;
		}else{
			fixedIndex = middleIndex + 1;
			if(fixedIndex >= points.Length) fixedIndex = 1;

			enforcedIndex = middleIndex - 1;
			if(enforcedIndex < 0) enforcedIndex = points.Length - 2;
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];

		if(mode == BezierControlPointMode.Aligned){
			//To keep it mirror in direction, but using the distance of the point before changing it
			//allows to keep the other points together.
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle,points[enforcedIndex]);	
		}

		points[enforcedIndex] = middle + enforcedTangent;
	}

	public Vector3 GetDirection(float t){ return GetVelocity(t).normalized; }


	public int CurveCount{ get{ return (points.Length-1) / 3; } }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
