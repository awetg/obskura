/// <summary>
/// Baal.
/// Written by Manuel, Awet and Elsa.
/// </summary>

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Teleporting enemy.
/// </summary>
public class Baal : Enemy, ICollidableActor2D {
	
	public float chaseTime = 30f;	//how long enemy chase player after detection

	public Vector3 centerPosition; //Position of the point the Baal is going to teleport around
	public float minTime = 1; //Minimum interval between two teleports
	public float maxTime =5; //Maximum interval between two teleports
	public float maxDistance = 3; //Max teleport distance from centerPosition
	public float chasingSpeed = 0.5F; //Speed of movement of the centerPosition toward the player during chase
	public float sightDistance = 50F; //Distance at which the player can be seen
	public float attackRange = 3f; //Range at which the Baal will infert damage

	public AudioSource AudioTeleport; //Sound the player hear when next to the Baal

	private Vector3 originalPosition; // Original position of the Baal, before chase (to restore if the Baal is blocked in a wall)

	float nextTeleportTime=0; //When to teleport
	float disableTeleportEffectAt=0; //When to disable the teleport graphical effect
	float endChaseTime = 0; //When to end the chase
	bool isTeleportEffectOn = false;

	// Use this for initialization
	protected override void Start () {
		SetState(startState);	//set state to idle from none

		centerPosition = transform.position; //The spawning center is the initial position of the object

		//Override here the default values from Enemy
		//Note: If they have been set in the inspector, their value will be != 0, so don't override
		if (enemyHp == 0)
			enemyHp = 100f;		
		if (damagePerSecond == 0)
			damagePerSecond = 50f;

		//How long will the gameObject survive after the enemy is dead
		destroyAfter = 0.1f;

		//STATES TABLE:
		//To every state associate a behaviour, composed of an init, an update and an end delegate
		states.Add (EnemyState.IDLE, new EnemyBehaviour (StartIdle, ContinueIdle, EndIdle));
		states.Add (EnemyState.CHASE, new EnemyBehaviour (StartChase, ContinueChase, EndChase));
		states.Add (EnemyState.ATTACK, new EnemyBehaviour (StartAttack, ContinueAttack, EndAttack));
		states.Add (EnemyState.DEAD, new EnemyBehaviour (None, None, None));

		originalPosition = centerPosition;

		base.Start ();
	}
		
	/// <summary>
	/// If the time delay for the next teleport has elapsed, try to teleport.
	/// </summary>
	void TeleportIfTime(){

		//Play the effect if less than 0.3 seconds are left
		if (!isTeleportEffectOn && nextTeleportTime - Time.time < 0.3f) {
			TeleportEffect (true);
			disableTeleportEffectAt = Time.time + 0.6f;
		}

		//Disable the effect if enough time has elapsed
		if (isTeleportEffectOn && Time.time > disableTeleportEffectAt)
			TeleportEffect (false);

		//If it's time to teleport...
		if (Time.time > nextTeleportTime) 
		{	
			System.Random rnd = new System.Random();
			Vector3 newpos;
			const int maxTry = 3;
			int count = 0;

			//... try to teleport to maximum maxTry random positions
			do {
				if (count > maxTry){
					//If we ended up in a wall for more than maxTry times, go back to the starting point
					newpos = originalPosition;
					break;
				}
				//generate the random position
				float dx = (float)rnd.NextDouble () * maxDistance; 
				float dy = (float)rnd.NextDouble () * maxDistance;
				newpos = centerPosition + new Vector3 (dx, dy, 0);
				count += 1;
				//Check if Baal teleported inside a wall
			} while (Geometry.IsPointInAWall(newpos) || Geometry.IsSquareInAWall (newpos, this.GetSize()));

			//Teleport
			transform.position = newpos;

			//Set a random next teleport time, between minTime and maxTime
			float interval = (float)(rnd.NextDouble () * (maxTime-minTime)) + minTime; 
			nextTeleportTime = Time.time + interval;

		}
	}

	/// <summary>
	/// Called when the Idle state starts
	/// </summary>
	void StartIdle(){	
		//Idle animation
		EnemyAnimator.Play ("BaalIdle");
	}

	/// <summary>
	/// Called at every update in the idle state
	/// </summary>
	void ContinueIdle(){
		
		//Check if Baal will start to chase or teleport
		chaseIfInSight (sightDistance);

		TeleportIfTime ();
		

	}

	/// <summary>
	/// Called at the end of idle state
	/// </summary>
	void EndIdle(){	}


	/// <summary>
	/// Called at the start of Chase state.
	/// </summary>
	void StartChase(){	
		endChaseTime = Time.time + chaseTime;
		FaceTarget (-Mathf.PI/2);
	}

	/// <summary>
	/// Called at every update of the chase state.
	/// </summary>
	void ContinueChase(){	

		//Don't keep on chasing if too much time passed or the player is in strong light
		if (Time.time<endChaseTime && target && !isPlayerInStrongLight()) {

			chaseIfInSight (sightDistance);

			TeleportIfTime ();

			Vector3 direction = (target.position - centerPosition).normalized;

			//Move the center towards the player
			centerPosition = centerPosition + direction * Time.deltaTime * chasingSpeed;

			FaceTarget (-Mathf.PI/2);

			//Check if Baal is near enough to attack
			if (Vector3.Distance (target.position, transform.position) < attackRange)
				SetState (EnemyState.ATTACK);

		} 
		else {
			//End the chase
			SetState (EnemyState.IDLE);			
		}
	}

	/// <summary>
	/// Called when the chase ends.
	/// </summary>
	void EndChase(){	
		FaceRandom ();
	}

	/// <summary>
	/// Called at the start of the attack state
	/// </summary>
	void StartAttack(){
		//Play an attack (proximity) sound
		if (AudioTeleport != null && !AudioTeleport.isPlaying) {
			AudioTeleport.PlayOneShot (AudioTeleport.clip);
		}
	}

	/// <summary>
	/// Called at every update of the attack state.
	/// </summary>
	void ContinueAttack(){
		//Check if we are still near enough to attack
		if (target && Vector3.Distance (target.position, transform.position) <= attackRange) {
			//Damage the player and face him/her
			target.GetComponent<Player> ().DamagePlayer (damagePerSecond * Time.deltaTime);
			FaceTarget (-Mathf.PI/2);
		}
		else {
			SetState (EnemyState.CHASE);
		}	
	}

	/// <summary>
	/// Called at the end of the attack state
	/// </summary>
	void EndAttack(){
	}
		
	/// <summary>
	/// Alerts the enemy.
	/// </summary>
	/// <param name="type">Type.</param>
	public override void Alert (EnemyAlert type)
	{
		base.Alert (type);

		//If the player alerted the enemy, extend the chase range to three times normal
		chaseIfInSight (sightDistance * 3);
	}

	/// <summary>
	/// Graphical effect of the teleport.
	/// </summary>
	/// <param name="active">If set to <c>true</c> active.</param>
	private void TeleportEffect(bool active){
		foreach (Transform t in gameObject.transform) {
			if (t.gameObject.name == "TeleportEffect")
			{
				isTeleportEffectOn = active;
				t.gameObject.SetActive(active);
			}
		}
	}

	//*** ICollidableActor2D Implementation ***

	public float GetSize() {
		return 1.5f;
	}

	public Vector2 GetPosition() {
		return new Vector2(transform.position.x, transform.position.y);
	}

	public GameObject GetGameObject() {
		return gameObject;
	}

	public void CollidedBy(CollisionType type, float damage, Vector2 force, bool setOnFire = false){
		if (type == CollisionType.PLASMA) {
			GetDamaged (damage);
			if (setOnFire)
				SetOnFire ();
		}
	}
		
	public bool IsColliderActive(){
		return enemyHp > 0.0f;
	}

}
