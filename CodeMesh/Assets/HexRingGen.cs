using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexRingGen : MonoBehaviour{
	private struct HexData{
		public CylinderMeshGen Mesh;
		public float Height;
		public HexData(CylinderMeshGen m, float h){ Mesh = m; Height = h; }
	}

	public GameObject HexPrefab;
	private List<GameObject> HexList = new List<GameObject>();
	private List<HexData> HexDataList = new List<HexData>();
	private int ringLen = 3;
	private float totalHeight = 3f;


	#region Behavior Events
	void Start (){
		float hStep = totalHeight / (ringLen + 2);
		float h = totalHeight - hStep;

		genInitalRing(totalHeight,h);
		for(int i = 1; i <= ringLen; i++){
			h -= hStep;
			genNextRing(i,h);
		}
		//genNumberPoints();
	}

	float norm(float v, float min, float max){ return (v-min) / (max-min); }
	void Update (){
		float step = Mathf.Abs(Mathf.Sin(Time.time));
		float h;
		int len = HexDataList.Count;

		for(int i=0; i < HexDataList.Count; i++){
			h = norm(--len,1,HexDataList.Count) * 10f * step;
			HexDataList[i].Mesh.setHeight(h);
		}//
	}
	#endregion

	#region Helper functions
	//Some math thing I needed that created the values I saw when sketching out the positions on paper.
	//There may be a name for it, even a math function somewhere but this takes a number to iterate from
	//then sum up with  multiplier.
	// If num = 3 and multi = 6  ---> (1*6) + (2*6) + (3*6)
	private int inc(int num, int multi){
		int rtn = 0;
		for(int i = 1; i <= num; i++) rtn += i*multi;
		return rtn;
	}
	#endregion

	#region Generations
	/// <summary>Create the Hex Prefab at a specific position and a default height</summary>
	/// <returns>hex gameobject</returns>
	/// <param name="v">vector position</param>
	/// <param name="h">height</param>
	private GameObject genHex(Vector3 v,float h){
		GameObject gi = (GameObject)Instantiate(HexPrefab,v,Quaternion.identity);
		CylinderMeshGen cmg = gi.GetComponent<CylinderMeshGen>();
		cmg.height = h;

		gi.transform.parent = this.transform;

		HexList.Add(gi);
		HexDataList.Add(new HexData(gi.GetComponent<CylinderMeshGen>(),h));
		return gi;
	}

	/// <summary>Generates the center plus the initial ring</summary>
	/// <param name="centerHeight">Center height.</param>
	/// <param name="ringHeight">Ring height.</param>
	private void genInitalRing(float centerHeight,float ringHeight){
		float spacing = Mathf.PI * 2 / 6; //360 in radians divided by how many points to plot for the circle.
		float angle = 0f * Mathf.PI / 180; //Starting Angle
		float radius = 1.75f;

		genHex(Vector3.zero,centerHeight); //Center One

		Vector3 vpos;
		for(int i = 0; i < 6; i++){
			vpos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
			genHex(vpos,ringHeight);
			angle += spacing;
		}
	}

	/// <summary>Gens the next ring.</summary>
	/// <param name="ring">Ring Value to generate</param>
	private void genNextRing(int ring,float height){
		Vector3 vpos;										//Position to put the hex Prefab.
		float radius,angle;

		int ringStart = inc(ring-1,6) + 1,					//Starting position of the previous ring in the array
			ringEnd = ringStart - 1 + (ring*6),				//Ending position of the previous ring in the array
			ringLen = ringEnd - ringStart + 1,				//How many elements in the array make up the ring
			iniFocal = (ring > 1)? inc(ring-2,6) + 1 : 0, 	//Focal point is the starting index of 2 rings back.
			posFocal = 0,									//Modulas counter between 0 - X, allows to loop back on the focus
			indFocal = iniFocal,							//Index to the currently focused hex for positioning new hex
			npos;											//Next hex to be accessed in the loop

		bool isCurrentCorner = false,						//Keep track when the current selected hex is a corner, which it's angle is evently divided by 60
			isNextCorner = false;							//Keep trakc when the next hex is a corner, no shifting of focal hex when going around a corner.

		//Debug.Log("--------------------------------------------");
		//Debug.Log(ring);
		//Debug.Log("--------------------------------------------");
		//Debug.Log(" ringstart " + ringStart + " ringEnd " + ringEnd +  " ringlen " + ringLen + " iniFocal " + iniFocal);

		for(int i = ringStart; i <= ringEnd; i++){
			npos = ((i-ringStart+1) % (ringLen)) + ringStart;

			//-------------------------------------
			//Figure out if the current and next hex are corners
			angle = Mathf.Atan2(HexList[i].transform.position.z,HexList[i].transform.position.x) * (180 / Mathf.PI);
			isCurrentCorner = (Mathf.RoundToInt(angle) % 60 == 0)? true : false;

			angle = Mathf.Atan2(HexList[npos].transform.position.z,HexList[npos].transform.position.x) * (180 / Mathf.PI);
			isNextCorner = (Mathf.RoundToInt(angle) % 60 == 0)? true : false;

			//Debug.Log(i + " npos " + npos + " ringstart " + ringStart + " ringlen " + ringLen + " indFocal " + indFocal + " angle " + angle + " isCorner" + isCurrentCorner + " isNextCorner" + isNextCorner);

			//-------------------------------------
			//Create a hex that is in the same direction as the current hex only if its a corner item
			if(isCurrentCorner){	
				vpos = HexList[i].transform.position;
				radius = Vector3.Distance(Vector3.zero,vpos) / ring;
				angle = Mathf.Atan2(vpos.z,vpos.x);

				vpos.x += radius * Mathf.Cos(angle);
				vpos.z += radius * Mathf.Sin(angle);
				genHex(vpos,height);
			}

			//-------------------------------------
			//Create a hex between the current and next hex, then using a focal point to help determine the position.
			vpos = Vector3.Lerp(HexList[i].transform.position,HexList[npos].transform.position,0.5f);
			vpos.x += vpos.x - HexList[indFocal].transform.position.x;
			vpos.z += vpos.z - HexList[indFocal].transform.position.z;
			genHex(vpos,height);

			//radius = Vector3.Distance(HexList[indFocal].transform.position,vpos);
			//angle = Mathf.Atan2(vpos.z,vpos.x);
			//angle = Vector3.Angle(HexList[indFocal].transform.position,vpos) * (Mathf.PI/180);

			//-------------------------------------
			//Only shift focal point when the next item is not a corner, when its a corner we want to circle around using the same focal point
			if(!isNextCorner && ring > 1){
				posFocal = ++posFocal % (6 * (ring-1));
				indFocal = posFocal + iniFocal;
				//Debug.Log("[[[[ New focal Point " + indFocal + "  " + posFocal+ "  " + ring + " ]]] ");
			} 
		}
	}
	#endregion

	#region Debugging
	private void genNumberPoints(){
		GameObject go,prefab = Resources.Load<GameObject>("TxtMesh");
		for(int i=0; i < HexList.Count; i++){
			go = GameObject.Instantiate<GameObject>(prefab);
			go.transform.parent = this.gameObject.transform;
			go.transform.localPosition = HexList[i].transform.position + Vector3.up;
			go.GetComponent<TextMesh>().text = i.ToString("00");
		}
	}
	#endregion
}
