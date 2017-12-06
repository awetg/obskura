//FIXME: Refactor Olight and OLaser to share more code (after the demo)

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OBullet : MonoBehaviour {

	const string damageTag = "Enemy";

	public float PrimaryDamage = 80f;
	public float SecondaryDamage = 30f;
	public float Speed = 1.0f;
	public float MaxDistance = 40.0f;

	private Vector2 origin;
	private Vector2 diff;
	private float progress;
	private bool isEnabled = false;

	public OLight LaserLight;


	public void Fire(Vector2 from, Vector2 target){

		if (isEnabled) //Already fired this bullet
			return;

		Vector2 pos = new Vector2 (from.x, from.y);
		//LaserLight = GameObject.Instantiate (LaserLight);
		LaserLight.IsOn = true;


		// Set the current position in the shader
		/*Material mat = LaserLight.GetComponent<Renderer>().material;
		mat.SetVector ("_Origin", new Vector4(pos.x, pos.y, 0, 0));
		mat.SetVector ("_Destination", new Vector4(target.x, target.y, 0, 0));
		mat.SetFloat ("_Dist", 2f);
		mat.SetFloat ("_Intensity", 10f);
		mat.SetColor ("_Color", Color.red);*/

		//Debug.DrawLine (transform.position, transform.position + new Vector3 (direction.x, direction.y, 0), Color.red, 100f);
		origin = pos;

		Debug.Log (pos.x + " " + pos.y);

		Intersection firstWall = Geometry.GetFirstIntersection (pos, target, MaxDistance);

		if (firstWall.v == null)
			return;
		
		diff = new Vector2(firstWall.v.Value.x, firstWall.v.Value.y) - pos;

		Debug.Log (firstWall.v.Value.x + " " + firstWall.v.Value.y);


		isEnabled = true;
		progress = 0.0f;

	}

	void Update () 
	{
		if (!isEnabled)
			return;
			
		if (progress >= 1.0f) {
			Destroy (gameObject);
		}

		progress += Speed * Time.deltaTime / diff.magnitude;

		Debug.Log (progress);
		Debug.Log (origin + diff * progress);

		transform.position = origin + diff * progress;
		LaserLight.transform.position = transform.position;
		LaserLight.Position = transform.position;
	}




}
