using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLaser : MonoBehaviour {
	void Start () {
		
	}

	void Update(){
		float rangeA = 5f, rangeB = 10f;
		Vector3 origin = transform.position + transform.forward;
		Vector3 endPos = origin + (transform.forward * rangeA);

		Vector3[] points = new Vector3[3];
		points[0] = origin;


		Debug.DrawLine(origin,endPos,Color.red);

		RaycastHit hit = new RaycastHit();
		bool isHit = Physics.Raycast(origin,transform.forward,out hit,rangeA);

		if(isHit){
			Debug.Log("-----------------------------");
			//Debug.Log(hit.distance);
			//Debug.Log(hit.normal);
			//Debug.Log(hit.point);
			endPos = hit.point;
		}
	
		Debug.DrawLine(endPos, endPos + (Vector3.down * rangeB),Color.green);

		points[1] = endPos;
		isHit = Physics.Raycast(endPos,Vector3.down,out hit,rangeB);

		if(isHit){
			//endPos = hit.point;
			points[2] = hit.point;
			drawLines(points);
		}
	}

	void drawLines(Vector3[] ary){
		LineRenderer rLine = GetComponent<LineRenderer>();
		//rLine.numPositions = 3;
		//rLine.SetPosition(0,ary[0]);
		//rLine.SetPosition(1,ary[1]);
		//rLine.SetPosition(2,ary[2]);


		float step = 1f/20f;
		Vector3 pos;
		rLine.numPositions = 20;
		rLine.SetPosition(0,ary[0]);

		for(int i=1; i < 19; i++){
			pos = GetPoint(ary[0],ary[1],ary[1],ary[2],step*i);
			rLine.SetPosition(i,pos);
		}
		rLine.SetPosition(19,ary[2]);
	}


	Vector3 GetPoint(Vector3 p0,Vector3 p1, Vector3 p2, Vector3 p3,float t){
		//return Vector3.Lerp(Vector3.Lerp(p0,p1,t),Vector3.Lerp(p1,p2,t),t);
		t = Mathf.Clamp01(t);
		float tInvert = 1f - t;
		return tInvert * tInvert * tInvert * p0 +
				3f * tInvert * tInvert * t * p1 +
				3f * tInvert * t * t * p2 + 
				t * t * t * p3;
	}

}
