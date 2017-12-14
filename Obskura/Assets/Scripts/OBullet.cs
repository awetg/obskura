//Written by Manuel
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Bullet fired by plasma gun.
/// </summary>
public class OBullet : MonoBehaviour {

	//Who to damage (apart from the player)
	const string damageTag = "Enemy";

	//Parameters of the buller
	public float PrimaryDamage = 80f;
	public float SecondaryDamage = 30f; //Lower damage in a bigger radius
	public float Speed = 1.0f;
	public float DimmingTime = 0.3f;
	public float MaxDistance = 100.0f;
	public float FireProbability = 0.2f;
	public float ImpactForce = 1f;
	public float SecondaryDamageRadius = 8f; //Lower damage in a bigger radius

	public AudioSource ImpactSound; //Sound on impact

	//Vectors to compute the motion of the bullet
	private Vector2 origin;
	private Vector2 destination;
	private Vector2 diff;

	//Progress of the bullet in its motion (from 0 to 1)
	private float progress;
	private float dimming = 3.0f; //Dimming factor after explosion
	private bool isEnabled = false;
	private bool arrived = false;
	private float baseIntensity = 0f; //Light intensity of the plasma bullet
	private float baseRange = 0f; //Light distance of the plasma buller
	private System.Random rnd = new System.Random();
	private List<ICollidableActor2D> collidables; //Cache of objects that can be hit by a bullet


	public OLight LaserLight;

	/// <summary>
	/// Fire the buller from "from" to "target".
	/// </summary>
	/// <param name="from">From.</param>
	/// <param name="target">Target.</param>
	public void Fire(Vector2 from, Vector2 target){

		if (isEnabled) //Already fired this bullet
			return;

		//Find the enemies thhat are collidable (ICollidableActor.cs)
		var enemies = GameObject.FindGameObjectsWithTag ("Enemy").Select(enemy => enemy.GetComponent<Enemy>());
		collidables = enemies.Where (enemy => enemy is ICollidableActor2D).Cast<ICollidableActor2D>().ToList();

		//Position of the starting pointt
		Vector2 pos = new Vector2 (from.x, from.y);

		//Turn on the plasma light
		LaserLight.IsOn = true;

		//set the origin
		origin = pos;

		//Find the first interception with a wall...
		Intersection firstWall = Geometry.GetFirstIntersection (pos, target, MaxDistance);

		//...if it exists...
		if (firstWall.v == null)
			return;

		//...set it as destination for the bullet
		destination = firstWall.v.Value;

		//Displacement vector of origin -> destination
		diff = new Vector2(firstWall.v.Value.x, firstWall.v.Value.y) - pos;

		//Initialize the travel
		isEnabled = true;
		progress = 0.0f;

	}

	/// <summary>
	/// Checks if it collided an enemy.
	/// </summary>
	void CheckEnemyCollision(){
		var collided = false;
		Vector2 pos = Vector2.zero;

		//Check for collision to get the explosion position
		foreach(ICollidableActor2D c in collidables){
			if (c != null && !c.IsColliderActive ())
				continue;
			
			var dstv = new Vector2(transform.position.x, transform.position.y) - c.GetPosition();
			var dist = dstv.magnitude;

			//If the distance of the bullet is less of the size of the object
			if (c.IsColliderActive() && dist < c.GetSize()){
				pos = c.GetPosition ();
				collided = true; //There has been a collision
				break;
			}
		}
			
		if (collided) {
			//Check now for all tthe enemies/players in damage distance
			foreach(ICollidableActor2D c in collidables){

				if (!c.IsColliderActive ())
					continue;

				var dstv = new Vector2(transform.position.x, transform.position.y) - c.GetPosition();
				var dist = dstv.magnitude;

				//Primary collision
				if (dist < c.GetSize ()) {
					var setOnFire = rnd.NextDouble () < FireProbability;
					c.CollidedBy (CollisionType.PLASMA, PrimaryDamage, ImpactForce * dstv.normalized / dist, setOnFire);
				} else if (dist < SecondaryDamageRadius) { //Secondary(distance) damage
					var setOnFire = rnd.NextDouble () < FireProbability / 2;
					c.CollidedBy (CollisionType.PLASMA, SecondaryDamage, ImpactForce * dstv.normalized / dist, setOnFire);
				}

			}

			//Switch to collided state
			Collided (pos - 0.8f * diff.normalized);
		}
	}

	/// <summary>
	/// Collided at the specified point, set the explosion in there.
	/// </summary>
	/// <param name="point">Point.</param>
	void Collided(Vector2 point) {
		//Damage player if too near (but with lower damage, and only in two thirds of the range)
		var player = GameObject.FindGameObjectWithTag("Player");
		if (player != null) {
			var dstv = new Vector2(transform.position.x, transform.position.y) - new Vector2(player.transform.position.x, player.transform.position.y);
			var dist = dstv.magnitude;
			if (dist < SecondaryDamageRadius / 1.5f)
				player.GetComponent<Player> ().DamagePlayer (SecondaryDamage);
		}

		//Prepare the explosion intensity animation
		arrived = true;
		dimming = 3.0f;
		baseIntensity = LaserLight.Intensity;
		baseRange = LaserLight.DimmingDistance;

		//Set the arriving position to be slightly before the destination
		transform.position = point - 0.5f * diff.normalized;
		LaserLight.transform.position = transform.position;
		LaserLight.Position = transform.position;

		//Play impact sound
		if (ImpactSound != null)
			ImpactSound.PlayOneShot (ImpactSound.clip);
	}

	void Update () 
	{
		if (!isEnabled)
			return;

		//The explosion faded away, destroy the bullet
		if (dimming <= 0.01f) {
			Destroy (gameObject);
		}

		if (!arrived) { //If is not arrived at the nearest wall
			CheckEnemyCollision (); //Check for enemy collision in between

			if (progress >= 0.99f) { //Finally collided the wall
				Collided (destination - 0.5f * diff.normalized);
			} else {
				//No collision, move the bullet forward
				progress += Speed * Time.deltaTime / diff.magnitude;

				transform.position = origin + diff * progress;
				LaserLight.transform.position = transform.position;
				LaserLight.Position = transform.position;
			}
		} else {
			//Already exploded... fade away in time
			LaserLight.Intensity = baseIntensity * dimming;
			LaserLight.DimmingDistance = baseRange * dimming / 1.5F;
			dimming -= (Time.deltaTime / (DimmingTime / 3));
		}
	}




}
