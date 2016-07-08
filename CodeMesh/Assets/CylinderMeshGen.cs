using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)) ]
public class CylinderMeshGen : MonoBehaviour{
	#region Public Vars
	[Tooltip("Set origin at the center of the mesh, else at the bottom.")]
	private bool IsOriginCenter = false;
	[Tooltip("How many points to use to make the circle"),Range(3,50)]
	public int points = 3;
	[Tooltip("How far from the center")]
	public float radius = 1;
	[Tooltip("What angle to start puttng the first vertice")]
	private float startingAngle = 90;  //TODO, Need to make the bottom vertices differently to be able to set the starting angle publicly.
	[Tooltip("Move the center vertice up or down")]
	public float centerPointOffset = 0;
	[Tooltip("Overall height of the mesh")]
	public float height = 1f;

	public bool debugPoints = false;
	#endregion

	#region Private Vars					
	private Mesh mMesh;
	private Vector3[] mVertices;
	private int[] mTriangles;								//Index if triangle to build the mesh
	private Vector2[] mUV;
	#endregion

	#region GameObject Events
	void Start(){
		mMesh = GetComponent<MeshFilter>().mesh;

		genVertices();
		genTriangles();
		applyToMesh();
		if(debugPoints) genNumberPoints();
	}
	//void Update (){}
	#endregion

	#region methods
		public void setHeight(float v){
			for(int i=0; i <= points; i++){
				mVertices[i].y = v;
			}
			mMesh.vertices = mVertices;
		}
	#endregion

	#region function
	/// <summary>Just Applies all the changes to the arrays onto the mesh</summary>
	private void applyToMesh(){
		mMesh.Clear();
		mMesh.vertices = mVertices;
		mMesh.triangles = mTriangles;
		mMesh.uv = mUV;
		mMesh.Optimize();
		mMesh.RecalculateNormals();
	}
	#endregion

	#region Mesh Generation
	/// <summary>Generate all the vertices, triangle, uv values needed to create a shape.</summary>
	private void genVertices(){
		float radPerPoint = Mathf.PI * 2 / points; //360 in radians divided by how many points to plot for the circle.
		float angle = startingAngle * Mathf.PI / 180; //Starting Angle
		float topPos = (IsOriginCenter)? height * 0.5f	: height;
		float botPos = (IsOriginCenter)? height * -0.5f	: 0f;
		int aryLen = (points+1) *2;

		mUV = new Vector2[aryLen];
		mUV[0].Set(1,1);

		mVertices = new Vector3[aryLen];
		mVertices[0].Set(0,topPos + centerPointOffset,0);

		for(int i = 1; i <= points; i++){
			mVertices[i].Set(Mathf.Cos(angle) * radius, topPos, Mathf.Sin(angle) * radius);
			mUV[i].Set(0,0);	
			angle -= radPerPoint;
		}

		genAngleFaceZ(0,points+1,points+1,Mathf.PI,botPos); //PI = 180 Degrees in Radians
		mVertices[points+1].y -= centerPointOffset; //Change the center point of the bottom face
	}

	private void genTriangles(){
		/* +++ NOTES +++
		Top face = points * 3;
		Bottom Face   = Top Face * 2
		Walls = points *  6 (the amount of points also determines how many walls, a single wall is made of a 4 vertex square but a square is two triangles which will require 6 points to be defined)
		Total Triangles = (points * 6) + (points * 3 * 2)*/

		mTriangles = new int[(points * 6) + (points * 3 * 2)];
		int tPos = 0,						//Current Position in the triangle array
			aIndex,							//The first point of the triangle based on the vertice array
			bIndex,							//The last point of the traingle based on the vertice arrqy
			mid = points + 1,				//The mid point of the vertice array\
			yIndex,							//Bot Left Position of the side Wall
			zIndex,							//Bot Right " " " 
			wpos;							//Wall position counting backwards from points to 1

		for(int i=0; i < points; i++ ){
			tPos = i * 12;

			//Generate index for the top+bottom faces plus the Top 2 points of the wall
			aIndex = i+1;
			bIndex = (aIndex % points) + 1;

			//Generate the bottom index of the wall.
			wpos = points - i;
			yIndex = wpos + mid;
			zIndex = (wpos % points) + 1 + mid;

			//Top Face
			mTriangles[tPos]	= bIndex;
			mTriangles[tPos+1]	= 0;
			mTriangles[tPos+2]	= aIndex;

			//Bottom Face
			mTriangles[tPos+3]	= mid+bIndex;
			mTriangles[tPos+4]	= mid;
			mTriangles[tPos+5]	= mid+aIndex;

			//Wall - Tri 1
			mTriangles[tPos+6]	= yIndex;
			mTriangles[tPos+7]	= bIndex;
			mTriangles[tPos+8]	= aIndex;

			//Wall - Tri 2
			mTriangles[tPos+9]	= aIndex;
			mTriangles[tPos+10]	= zIndex;
			mTriangles[tPos+11]	= yIndex;
		}
	}
	#endregion

	#region Axis Rotation functions
	/// <summary>Takes a part of the vertice array and rotates it by the Y axis while saving the results to another part in the array.</summary>
	private void genAngleFaceY(int sPos,int dPos,int len,float angle){
		float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);

		//Bit of optimizing by putting the rotation math in this function
		for(int i = 0; i < len; i++){ 
			mVertices[i+dPos].x = mVertices[i+sPos].x * cos + mVertices[i+sPos].z * sin;
			mVertices[i+dPos].y = mVertices[i+sPos].y;
			mVertices[i+dPos].z = -mVertices[i+sPos].x * sin + mVertices[i+sPos].z * cos;

			mUV[i+dPos].x = mUV[i+sPos].x;
			mUV[i+dPos].y = mUV[i+sPos].y;
		}
	}

	/// <summary>Takes a part of the vertice array and rotates it by the Y axis while saving the results to another part in the array.</summary>
	private void genAngleFaceZ(int sPos,int dPos,int len,float angle,float yOveride){
		float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
		//Debug.Log(yOveride);
		//Bit of optimizing by putting the rotation math in this function
		for(int i = 0; i < len; i++){ 
			mVertices[i+dPos].x = mVertices[i+sPos].x * cos - mVertices[i+sPos].y * sin;
			mVertices[i+dPos].y = yOveride; //mVertices[i+sPos].x * sin + mVertices[i+sPos].y * cos;
			mVertices[i+dPos].z = mVertices[i+sPos].z;

			mUV[i+dPos].x = mUV[i+sPos].x;
			mUV[i+dPos].y = mUV[i+sPos].y;
		}
	}

	private Vector3 VectorRotateY(Vector3 v, float angle){
		float rad = angle * Mathf.PI / 180
			,cos = Mathf.Cos(rad)
			,sin = Mathf.Sin(rad);
		return new Vector3(v.x*cos + v.z*sin, v.y, -v.x*sin + v.z*cos);
	}

	private Vector3 VectorRotateX(Vector3 v, float angle){
		float rad = angle * Mathf.PI / 180
			,cos = Mathf.Cos(rad)
			,sin = Mathf.Sin(rad);
		return new Vector3(v.x, v.y*cos - v.z*sin, v.y*sin + v.z*cos);
	}

	private Vector3 VectorRotateZ(Vector3 v, float angle){
		float rad = angle * Mathf.PI / 180
			,cos = Mathf.Cos(rad)
			,sin = Mathf.Sin(rad);
		return new Vector3(v.x*cos - v.y*sin, v.x*sin + v.y*cos, v.z);
	}
	#endregion

	#region Debugging Stuff
	/// <summary>Generate Numbers of the points in the mesh. Makes it easier to hardcode the triangle values</summary>
	private void genNumberPoints(){
		GameObject go,prefab = Resources.Load<GameObject>("TxtMesh");
		for(int i=0; i < mVertices.Length; i++){
			go = GameObject.Instantiate<GameObject>(prefab);
			go.transform.parent = this.gameObject.transform;
			go.transform.localPosition = mVertices[i]; 

			//This creates position on the world, but to keep the numbers with the object, need to do above.
			//go = GameObject.Instantiate(prefab,mVertices[i],Quaternion.identity) as GameObject;
			//go.transform.parent = this.gameObject.transform;

			go.GetComponent<TextMesh>().text = i.ToString("00");
		}
	}
	#endregion
}
