using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)) ]
public class CubeMeshGen : MonoBehaviour{
	#region Public Vars
	[Tooltip("Width of the Cube Mesh (x)")]
	public float width = 2f;			
	[Tooltip("Height of the Cube Mesh (y)")]
	public float height = 2f;
	[Tooltip("Depth of the Cube Mesh (z)")]
	public float depth = 2f;
	[Tooltip("Set origin at the center of the mesh, else at the bottom.")]
	public bool IsOriginCenter = false;
	#endregion

	#region Private Vars
	private const int VERT_LEN = 8;							
	private Mesh mMesh;
	private Vector3[] mVertices = new Vector3[VERT_LEN];	//All the vertices needed to build the cube.
	private int[] mTriangles;								//Index if triangle to build the mesh
	private Vector2[] mUV = new Vector2[VERT_LEN];
	private int[] mFaces;									//An array that keep Tracks of points per cube side face, Can gen triangle array from it, also use it for animations/deformations of the cube based on faces
	#endregion

	#region GameObject Events
	void Start(){
		mMesh = GetComponent<MeshFilter>().mesh;
		genVertices();
		applyToMesh();
		genNumberPoints(); //TODO, can remove only for debugging
	}
	//void Update (){}
	#endregion

	#region Methods
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
	/*=======================================================
	**NOTES**
	Documents say the front of the triangle face is clockwise, but when using the scene editor 
	set to the front, the X is backwards, it goes negative to the right. Because of that, to get
	a single triangle to display in the front view, it has to be in a counter clockwise order,
	which goes against documentation. When adding a TextMesh, it shows that the back is really the front
	even though Scene editor says I'm viewing the back. So starting with the back, I can order things
	in a clockwise direction.

	Order of verts to create first face.
	0 _ 1
	|   |
	3 _ 2
	=======================================================*/

	/// <summary>Generate all the vertices, triangle, uv values needed to create a shape.</summary>
	private void genVertices(){
		float	hposTop = (IsOriginCenter)? height * 0.5f	: height,
				hposBot = (IsOriginCenter)? height * -0.5f	: 0f,
				wpos = width * .5f,
				dpos = depth * .5f;
		
		//Set Inital Back face
		mVertices[0].Set(wpos*-1,hposTop,-dpos);
		mVertices[1].Set(wpos,hposTop,-dpos);
		mVertices[2].Set(wpos,hposBot,-dpos);
		mVertices[3].Set(wpos*-1,hposBot,-dpos);

		//Set Inital UV
		mUV[0].Set(0,1);
		mUV[1].Set(1,1);
		mUV[2].Set(1,0);
		mUV[3].Set(0,0);

		//Save Index of each face
		mFaces = new int[]{
			0,1,2,3,	//Back
			4,5,6,7,	//Front
			5,4,1,0,	//Top
			3,2,7,6,	//Bottom
			1,4,7,2,	//Left
			5,0,3,6		//Right
		};

		//Rotate Back face to the Front to create the remaining verts, can keep the same triangle pattern when rotated.
		genAngleFaceY(0, 4, 4, 180f * Mathf.PI / 180);
		genTriFromFaces();
	}

	public void genTriFromFaces(){
		int pos = 0;
		mTriangles = new int[mFaces.Length / 4 * 6]; // 6 points per face
		for(int i=0; i < mFaces.Length; i+=4){
			mTriangles[pos] = mFaces[i];
			mTriangles[pos+1] = mFaces[i+1];
			mTriangles[pos+2] = mTriangles[pos+3] = mFaces[i+2];
			mTriangles[pos+4] = mFaces[i+3];
			mTriangles[pos+5] = mFaces[i];
			pos+=6;
		}

		/* Originally hardcoded
		mTriangles = new int[6*6]{ //6 Points Per Square Face 
			0,1,2, 2,3,0,	//Back
			4,5,6 ,6,7,4,	//Front
			5,4,1 ,1,0,5,	//Top
			3,2,7 ,7,6,3,	//Bottom
			1,4,7 ,7,2,1,	//Left
			5,0,3 ,3,6,5	//Right
		};
		*/
	}//func
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
