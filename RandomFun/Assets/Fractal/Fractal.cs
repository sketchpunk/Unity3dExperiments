using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour {
	public Mesh mesh;
	public Material material;
	public int maxDepth = 4;
	public float childScale = 0.5f;
	public float spawnProbability = 1f;
	public float maxRotationSpeed = 50f;

	private int depth = 0;
	private float rotationSpeed;


	private Material[] materials;

	private static Vector3[] cDirection = {
		Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
	};

	private static Quaternion[] cOrient = {
		Quaternion.identity,
		Quaternion.Euler(0f,0f,-90f),
		Quaternion.Euler(0f,0f,90f),
		Quaternion.Euler(90f,0f,0f),
		Quaternion.Euler(-90f,0f,0f)
	};

	void Start () {
		if(materials == null) initMaterials();

		rotationSpeed = Random.Range(-maxRotationSpeed,maxRotationSpeed);

		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		gameObject.AddComponent<MeshRenderer>().material = materials[depth];
		//MeshRenderer mr = gameObject.AddComponent<MeshRenderer>()
		//mr.material = material;
		//mr.material.color = Color.Lerp(Color.white,Color.red, (float)depth / maxDepth * 0.7f);

		if(depth < maxDepth){
			StartCoroutine(MakeChildren());
		}
	}

	private void initMaterials(){
		materials = new Material[maxDepth+1];
		for(int i=0; i <= maxDepth; i++){
			float t = i / (maxDepth - 1f); //(float)depth / maxDepth * 0.7f
			t *= t;

			//Debug.Log(t);
			materials[i] = new Material(this.material);
			materials[i].color = Color.Lerp(Color.white,Color.red, t);
		}
		materials[maxDepth].color = Color.magenta;
	}

	public void Init(Fractal parent,int cIndex){
		this.mesh = parent.mesh;
		this.material = parent.material;
		this.maxDepth = parent.maxDepth;
		this.childScale = parent.childScale;
		this.depth = parent.depth+1;
		this.materials = parent.materials;
		this.spawnProbability = parent.spawnProbability;
		this.maxRotationSpeed = parent.maxRotationSpeed;

		this.transform.parent = parent.transform;
		this.transform.localRotation = cOrient[cIndex];
		this.transform.localScale = Vector3.one * childScale;
		this.transform.localPosition = cDirection[cIndex] * (0.5f + 0.5f * childScale); // 1 * HALF PLUS The other Half but at scale value
	}

	private IEnumerator MakeChildren(){
		for(int i=0; i < cDirection.Length; i++){
			if(Random.value < spawnProbability){
				yield return new WaitForSeconds(Random.Range(0.1f,0.5f));
				new GameObject("Fractal Child").AddComponent<Fractal>().Init(this,i);
			}
		}

		/*
		yield return new WaitForSeconds(0.5f);
		new GameObject("Fractal Child").AddComponent<Fractal>().Init(this,Vector3.up,Quaternion.identity);
		yield return new WaitForSeconds(0.5f);
		new GameObject("Fractal Child").AddComponent<Fractal>().Init(this,Vector3.right,Quaternion.Euler(0f,0f,-90f));
		yield return new WaitForSeconds(0.5f);
		new GameObject("Fractal Child").AddComponent<Fractal>().Init(this,Vector3.left,Quaternion.Euler(0f,0f,90f));
		*/
		//yield return new WaitForSeconds(0.5f);
		//new GameObject("Fractal Child").AddComponent<Fractal>().Init(this,Vector3.forward);
		//yield return new WaitForSeconds(0.5f);
		//new GameObject("Fractal Child").AddComponent<Fractal>().Init(this,-Vector3.forward);
	}

	// Update is called once per frame
	void Update () {
		transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
	}
}
