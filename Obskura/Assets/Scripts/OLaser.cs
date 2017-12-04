//FIXME: Refactor Olight and OLaser to share more code (after the demo)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OLaser : MonoBehaviour {

	const string damageTag = "Enemy";

	private const float shootingTime = 0.5f;
	private float stopShootingAt = 0;
	public float LaserPower = 200f;

	Mesh lightMesh;
	MeshFilter meshFilter;

	void Awake() 
	{
		lightMesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();
	}

	public void Fire(Vector2 target){
		if (Time.time < stopShootingAt)
			return;

		Vector2 pos = new Vector2 (transform.position.x, transform.position.y);

		// Set the current position in the shader
		Material mat = GetComponent<Renderer>().material;
		mat.SetVector ("_Origin", new Vector4(pos.x, pos.y, 0, 0));
		mat.SetVector ("_Destination", new Vector4(target.x, target.y, 0, 0));
		mat.SetFloat ("_Dist", 0.5f);
		mat.SetFloat ("_Intensity", 10f);
		mat.SetColor ("_Color", Color.red);

		Vector2 diff = new Vector2(transform.position.x, transform.position.y) - target;
		float length = diff.magnitude;

		Debug.DrawLine (transform.position, transform.position + new Vector3 (diff.x, diff.y, 0), Color.red, 100f);

		List<GameObject> gos = Geometry.GetActorsIntersectingRayWithTags (transform.position, target, new List<string>{ "Enemy" })
			.Select(g => g.GetGameObject()).ToList();
		
		List<Enemy> enemies = gos.Where (go => go.GetComponent<Enemy>() != null).Select(go => go.GetComponent<Enemy>()).Cast<Enemy>().ToList();
		enemies.ForEach (e => e.GetDamaged (LaserPower));

		Debug.Log (enemies.Count ());

		if (Time.time >= stopShootingAt) {
			stopShootingAt = Time.time + shootingTime;
		}

		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();

		verts.Add (new Vector3 (length, length, 0));
		verts.Add (new Vector3 (length, -length, 0));
		verts.Add (new Vector3 (-length, -length, 0));
		verts.Add (new Vector3 (-length, length, 0));


		// Triangles of the mesh
		for(var i=0;i<verts.Count+1;i++)
		{
			tris.Add((i+1) % verts.Count);
			tris.Add((i) % verts.Count);
			tris.Add(0);
		}

		// Build mesh
		lightMesh.Clear();
		lightMesh.vertices = verts.ToArray();
		lightMesh.triangles = tris.ToArray();

		//lightMesh.RecalculateNormals(); // FIXME: no need if no lights..or just assign fixed value..
		meshFilter.mesh = lightMesh;

		stopShootingAt = Time.time + shootingTime;

	}

	void Update () 
	{

		if (Time.time < stopShootingAt) {
			//Show the laser
		}



	}




}
