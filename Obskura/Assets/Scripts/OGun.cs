//FIXME: Refactor Olight and OLaser to share more code (after the demo)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OGun : MonoBehaviour {

	const string damageTag = "Enemy";

	private const float shootingTime = 0.5f;
	private float stopShootingAt = 0;
	public float LaserPower = 200f;

	public OBullet PlasmaBullet;

	//Mesh lightMesh;
	//MeshFilter meshFilter;


	public void Fire(Vector2 from, Vector2 target){

		// Set the current position in the shader
		/*Material mat = PlasmaBullet.GetComponent<Renderer>().material;
		mat.SetVector ("_Origin", new Vector4(pos.x, pos.y, 0, 0));
		mat.SetVector ("_Destination", new Vector4(target.x, target.y, 0, 0));
		mat.SetFloat ("_Dist", 2f);
		mat.SetFloat ("_Intensity", 10f);
		mat.SetColor ("_Color", Color.red);*/

		//Vector2 diff = new Vector2(transform.position.x, transform.position.y) - target;
		//float length = diff.magnitude;

		GameObject bullet = GameObject.Instantiate (PlasmaBullet.gameObject);
		bullet.GetComponent<OBullet> ().Fire (from, target);

		//Debug.DrawLine (transform.position, transform.position + new Vector3 (diff.x, diff.y, 0), Color.red, 100f);

		/*List<GameObject> gos = Geometry.GetActorsIntersectingRayWithTags (transform.position, target, new List<string>{ "Enemy" })
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
		}*/

		//stopShootingAt = Time.time + shootingTime;

	}

	void Update () 
	{

		if (Time.time < stopShootingAt) {
			//Show the laser
		}



	}




}
