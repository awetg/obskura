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
	public float DimmingTime = 0.3f;
	public float MaxDistance = 100.0f;
	public float FireProbability = 0.2f;
	public float ImpactForce = 1f;
	public float SecondaryDamageRadius = 8f;

	private Vector2 origin;
	private Vector2 destination;
	private Vector2 diff;
	private float progress;
	private float dimming = 3.0f;
	private bool isEnabled = false;
	private bool arrived = false;
	private float baseIntensity = 0f;
	private float baseRange = 0f;
	private System.Random rnd = new System.Random();
	private List<ICollidableActor2D> collidables; //Objects that can be hit by a bullet


	public OLight LaserLight;


	public void Fire(Vector2 from, Vector2 target){

		if (isEnabled) //Already fired this bullet
			return;

		var enemies = GameObject.FindGameObjectsWithTag ("Enemy").Select(enemy => enemy.GetComponent<Enemy>());
		collidables = enemies.Where (enemy => enemy is ICollidableActor2D).Cast<ICollidableActor2D>().ToList();

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

		destination = firstWall.v.Value;
		
		diff = new Vector2(firstWall.v.Value.x, firstWall.v.Value.y) - pos;

		Debug.Log (firstWall.v.Value.x + " " + firstWall.v.Value.y);


		isEnabled = true;
		progress = 0.0f;

	}

	void CheckEnemyCollision(){
		var collided = false;
		Vector2 pos = Vector2.zero;

		//First check for collision
		foreach(ICollidableActor2D c in collidables){
			var dstv = new Vector2(transform.position.x, transform.position.y) - c.GetPosition();
			var dist = dstv.magnitude;
			if (c.IsColliderActive() && dist < c.GetSize()){
				pos = c.GetPosition ();
				collided = true;
				break;
			}
		}
			
		if (collided) {
			
			foreach(ICollidableActor2D c in collidables){

				if (!c.IsColliderActive ())
					continue;

				var dstv = new Vector2(transform.position.x, transform.position.y) - c.GetPosition();
				var dist = dstv.magnitude;
				if (dist < c.GetSize ()) {
					var setOnFire = rnd.NextDouble () < FireProbability;
					c.CollidedBy (CollisionType.PLASMA, PrimaryDamage, ImpactForce * dstv.normalized / dist, setOnFire);
				} else if (dist < SecondaryDamageRadius) {
					var setOnFire = rnd.NextDouble () < FireProbability / 2;
					c.CollidedBy (CollisionType.PLASMA, SecondaryDamage, ImpactForce * dstv.normalized / dist, setOnFire);
				}

			}

			//Switch to collided state
			Collided (pos - 0.8f * diff.normalized);
		}
	}

	void Collided(Vector2 point) {
		//Damage player if too near (but with lower damage, and only in two thirds of the range)
		var player = GameObject.FindGameObjectWithTag("Player");
		if (player != null) {
			var dstv = new Vector2(transform.position.x, transform.position.y) - new Vector2(player.transform.position.x, player.transform.position.y);
			var dist = dstv.magnitude;
			if (dist < SecondaryDamageRadius / 1.5f)
				player.GetComponent<Player> ().DamagePlayer (SecondaryDamage);
		}

		arrived = true;
		dimming = 3.0f;
		baseIntensity = LaserLight.Intensity;
		baseRange = LaserLight.DimmingDistance;

		//Set the arriving position to be slightly before the destination
		transform.position = point - 0.5f * diff.normalized;
		LaserLight.transform.position = transform.position;
		LaserLight.Position = transform.position;
	}

	void Update () 
	{
		if (!isEnabled)
			return;

		if (dimming <= 0.01f) {
			Destroy (gameObject);
		}

		if (!arrived) {
			CheckEnemyCollision ();

			if (progress >= 0.99f) {
				Collided (destination - 0.5f * diff.normalized);
			} else {

				progress += Speed * Time.deltaTime / diff.magnitude;

				transform.position = origin + diff * progress;
				LaserLight.transform.position = transform.position;
				LaserLight.Position = transform.position;
			}
		} else {
			LaserLight.Intensity = baseIntensity * dimming;
			LaserLight.DimmingDistance = baseRange * dimming / 1.5F;
			dimming -= (Time.deltaTime / (DimmingTime / 3));
		}
	}




}
