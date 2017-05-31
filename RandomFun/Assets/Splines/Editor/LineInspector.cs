using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor {

	void OnSceneGUI () {
		Line line = target as Line;

		//Handle draws at world space, the points in the line will be treated as and will not take the gameobject transform into account
		//So using the points as a local space coord, we can have the transform translate it to world space, basicly its really just
		//using the current transform as an offset to be added to the points.
		Transform tran = line.transform;
		Vector3 p0 = tran.TransformPoint(line.p0);
		Vector3 p1 = tran.TransformPoint(line.p1);

		//pivot global or local, Need to set rotation correctly so everything aligns to the correct direction on the gameobject
		//Quaternion rot = tran.rotation;
		Quaternion rot = (Tools.pivotRotation == PivotRotation.Local)? tran.rotation : Quaternion.identity;

		Handles.color = Color.white;
		Handles.DrawLine(p0,p1);	
		//Handles.DoPositionHandle(p0,rot);
		//Handles.DoPositionHandle(p1,rot);

		//Check if Handlers have been moved.
		EditorGUI.BeginChangeCheck();
		p0 = Handles.DoPositionHandle(p0, rot);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(line,"Move Point");		//Set it up so it can be undone
			EditorUtility.SetDirty(line);				//Set the change as dirty so the editor knows something changes and will ask to save on exit.
			line.p0 = tran.InverseTransformPoint(p0);	//If changed, save world space position to local.

		}

		EditorGUI.BeginChangeCheck();
		p1 = Handles.DoPositionHandle(p1,rot);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(line,"Move Point");
			EditorUtility.SetDirty(line);
			line.p1 = tran.InverseTransformPoint(p1);
		}
	}



}
