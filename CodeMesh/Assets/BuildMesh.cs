using UnityEngine;
using System.Collections;

[ExecuteInEditMode] 
public class BuildMesh : MonoBehaviour {


	// Use this for initialization
	void Start () {
		MeshFilter mf = GetComponent<MeshFilter>();
		Mesh mesh = mf.sharedMesh;

		//Setup Verts
		Vector3[] verts = new Vector3[]{
			//Front
			new Vector3(-1,1,1),
			new Vector3(1,1,1),
			new Vector3(-1,-1,1),
			new Vector3(1,-1,1),

			//back
			new Vector3(1,1,-1),
			new Vector3(-1,1,-1),
			new Vector3(1,-1,-1),
			new Vector3(-1,-1,-1),

			//left
			new Vector3(-1,1,-1),
			new Vector3(-1,1,1),
			new Vector3(-1,-1,-1),
			new Vector3(-1,-1,1),

			//Right
			new Vector3(1,1,1),
			new Vector3(1,1,-1),
			new Vector3(1,-1,1),
			new Vector3(1,-1,-1),

			//Top
			new Vector3(-1,1,-1),
			new Vector3(1,1,-1),
			new Vector3(-1,1,1),
			new Vector3(1,1,1),

			//Bottom
			new Vector3(-1,-1,1),
			new Vector3(1,-1,1),
			new Vector3(-1,-1,-1),
			new Vector3(1,-1,-1)
		};

		//Triangles - 3 points, clickwise to show forward normal
		int[] tris = new int[]{
			0,2,3,
			3,1,0,

			4,6,7,
			7,5,4,

			8,10,11,
			11,9,8,

			12,14,15,
			15,13,12,

			16,18,19,
			19,17,16,

			20,22,23,
			23,21,20,
		};

		//UVs -- 0,0 is bottom left, 1,1 is top right
		Vector2[] uvs = new Vector2[]{
			new Vector2(0,1),
			new Vector2(0,0),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,1),
			new Vector2(0,0),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,1),
			new Vector2(0,0),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,1),
			new Vector2(0,0),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,1),
			new Vector2(0,0),
			new Vector2(1,1),
			new Vector2(1,0),

			new Vector2(0,1),
			new Vector2(0,0),
			new Vector2(1,1),
			new Vector2(1,0)
		};

		mesh.Clear();
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.uv = uvs;
		mesh.Optimize();
		mesh.RecalculateNormals();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
