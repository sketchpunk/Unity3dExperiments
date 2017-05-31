using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveInspector : Editor {

	private BezierCurve curve;
	private Transform tran;
	private Quaternion handleRotation;

	private const int lineSteps = 10;
	private const float directionScale = 0.5f;

	void OnSceneGUI (){
		curve = target as BezierCurve;
		tran = curve.transform;
		handleRotation = (Tools.pivotRotation == PivotRotation.Local)? tran.rotation : Quaternion.identity;

		if(curve.points.Length == 3){
			Vector3 p0 = ShowPoint(0),
					p1 = ShowPoint(1),
					p2 = ShowPoint(2);

			Handles.color = Color.gray;
			Handles.DrawLine(p0,p1);
			Handles.DrawLine(p1,p2);

			Handles.color = Color.green;
			Vector3 lineEnd,lineStart = curve.GetPoint(0f);
			Handles.DrawLine(lineStart, lineStart + curve.GetDirection(0f));
			for(int i=1;  i <= lineSteps; i++){
				lineEnd = curve.GetPoint( i / (float)lineSteps );
				Handles.color = Color.white;
				Handles.DrawLine(lineStart,lineEnd);
				Handles.color = Color.green;
				Handles.DrawLine(lineEnd, lineEnd + curve.GetDirection (1 / (float)lineSteps ));

				lineStart = lineEnd;
			}
		}else{
			Vector3 p0 = ShowPoint(0),
					p1 = ShowPoint(1),
					p2 = ShowPoint(2),
					p3 = ShowPoint(3);

			Handles.color = Color.gray;
			Handles.DrawLine(p0,p1);
			Handles.DrawLine(p2,p3);

			ShowDirections();
			Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
		}

		/*
		Handles.color = Color.green;
		Vector3 lineEnd,lineStart = curve.GetPoint(0f);
		for(int i=1;  i <= lineSteps; i++){
			lineEnd = curve.GetPoint( i / (float)lineSteps );
			Handles.DrawLine(lineStart,lineEnd);
			lineStart = lineEnd;
		}
		*/
	}

	private  void ShowDirections(){
		Handles.color = Color.green;
		Vector3 p = curve.GetPoint(0f);
		Handles.DrawLine(p, p + curve.GetDirection(0f) * directionScale);
		for(int i=1; i <= lineSteps; i++){
			p = curve.GetPoint( i / (float) lineSteps );
			Handles.DrawLine(p,p + curve.GetDirection( i / (float)lineSteps ));
		}
	}

	private Vector3 ShowPoint(int i){
		Vector3 p = tran.TransformPoint(curve.points[i]);

		EditorGUI.BeginChangeCheck();
		p = Handles.DoPositionHandle(p,handleRotation);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(curve,"Move Point");
			EditorUtility.SetDirty(curve);
			curve.points[i] = tran.InverseTransformPoint(p);
		}

		return p;
	}

}
