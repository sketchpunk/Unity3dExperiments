using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineInspector : Editor {

	private BezierSpline spline;
	private Transform tran;
	private Quaternion handleRotation;

	private const int lineSteps = 10;
	private const float directionScale = 0.5f;
	private const int stepsPerCurve = 10;

	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;

	private int selectedIndex = -1;

	private static Color[] modeColors = { Color.white, Color.yellow, Color.cyan };


	void OnSceneGUI (){
		spline = target as BezierSpline;
		tran = spline.transform;
		handleRotation = (Tools.pivotRotation == PivotRotation.Local)? tran.rotation : Quaternion.identity;

		Vector3 p0,p1,p2,p3;

		for(int i=0; i < spline.ControlPointCount-1; i+=3){
			p0 = ShowPoint(i);
			p1 = ShowPoint(i+1);
			p2 = ShowPoint(i+2);
			p3 = ShowPoint(i+3);

			Handles.color = Color.gray;
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p2, p3);
			Handles.DrawBezier(p0,p3,p1,p2,Color.white,null,2f);
		}

		ShowDirections();

		/*
		Vector3 p0 = ShowPoint(0)

		Handles.color = Color.gray;
		Handles.DrawLine(p0,p1);
		Handles.DrawLine(p2,p3);

		ShowDirections();
		Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
		*/
	}

	public override void OnInspectorGUI(){
		//DrawDefaultInspector();
		spline = target as BezierSpline;

		EditorGUI.BeginChangeCheck();
		bool loop = EditorGUILayout.Toggle("Loop",spline.Loop);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(spline,"Toggle Loop");
			EditorUtility.SetDirty(spline);
			spline.Loop = loop;
		}

		if(selectedIndex >= 0 && selectedIndex < spline.ControlPointCount){
			DrawSelectedPointInspector();
		}

		if(GUILayout.Button("Add Curve")){
			Undo.RecordObject(spline,"Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
	}

	private void DrawSelectedPointInspector(){
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 p = EditorGUILayout.Vector3Field("Position",spline.GetControlPoint(selectedIndex));
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(spline,"Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex,p);
		}

		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)
			EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(spline,"Change Point Mode");
			spline.SetControlPointMode(selectedIndex,mode);
			EditorUtility.SetDirty(spline);
		}
	}

	private  void ShowDirections(){
		Handles.color = Color.red;
		Vector3 p = spline.GetPoint(0f);
		Handles.DrawLine(p, p + spline.GetDirection(0f) * directionScale);
		int steps = stepsPerCurve * spline.CurveCount;

		for(int i=1; i <= steps; i++){
			p = spline.GetPoint( i / (float) steps );
			Handles.DrawLine(p,p + spline.GetDirection( i / (float)steps ));
		}
	}

	private Vector3 ShowPoint(int i){
		Vector3 p = tran.TransformPoint(spline.GetControlPoint(i));

		float size = HandleUtility.GetHandleSize(p); //Scale the dots to be the same no matter how zoomed in/out.

		if(i == 0) size *= 2f;

		Handles.color = modeColors[ (int)spline.GetControlPointMode(i) ]; //Color.white;
		if(Handles.Button(p,handleRotation,size * handleSize,size * pickSize,Handles.DotCap)){
			selectedIndex = i;
			Repaint();
		}

		if(selectedIndex == i){
			EditorGUI.BeginChangeCheck();
			p = Handles.DoPositionHandle(p,handleRotation);
			if(EditorGUI.EndChangeCheck()){
				Undo.RecordObject(spline,"Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(i,tran.InverseTransformPoint(p));
			}
		}

		return p;
	}

}

