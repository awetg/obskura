using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hades enemy.
/// </summary>
public class Hades : Enemy, ICollidableActor2D
{
	public float chaseTime = 10f;	//how long enemy chase player after detection
	public float chaseRange = 15f;	//how far away can the enemy see the player	
	public float attackTime = 0.5f; //How long shouldd we attack for?
	public float attackRange = 1f; //How far away does the atttack reach

	public float idleSpeed = 1.0f; //Walking speed
	public float chaseSpeed = 10f;	//how long enemy chase player after detection
	public NavMeshAgent MynavMeshAgent; //Unity's nav mesh, to avoid the obstacles while walking
	public EnemyPath startNode;		//start node to move until enemy detection, set in INSPECTOR

	public AudioClip AudioWalk; //Step sound
	public AudioClip AudioChase; //Chase/Attack/Death sound

	//When to end chase and attack
	private float endChaseTime = 0;
	private float endAttackTime = 0;

	// Use this for initialization
	protected override void Start () {
		//Override here the default values from Enemy
		//Note: If they have been set in the inspector, their value will be != 0, so don't override
		if (enemyHp == 0)
			enemyHp = 100f;		
		if (damagePerSecond == 0)
			damagePerSecond = 33f;

		destroyAfter = -1f;

		//SetState(startState);	//set state to idle from none
		MynavMeshAgent = GetComponent<NavMeshAgent>();	//initialize navMeshAgnet
		//target = GameObject.FindGameObjectWithTag("Player").transform;

		//STATES TABLE:
		//To every state associate a behaviour, composed of an init, an update and an end delegate
		states.Add (EnemyState.IDLE, new EnemyBehaviour (StartPath, ContinuePath, EndPath));
		states.Add (EnemyState.CHASE, new EnemyBehaviour (StartChase, ContinueChase, EndChase));
		states.Add (EnemyState.ATTACK, new EnemyBehaviour (StartAttack, ContinueAttack, EndAttack));
		states.Add (EnemyState.DEAD, new EnemyBehaviour (DeadState, DeadStateUpdate, None));

		base.Start ();
	}

	// Update is called once per frame
	protected override void Update () {
		base.Update ();

		//Prevent the navmesh from messing up with the angle in the x axis
		transform.eulerAngles = new Vector3(0,0,transform.eulerAngles.z);
		MynavMeshAgent.updateRotation = false;
	}

	public override void Alert (EnemyAlert type)
	{
		base.Alert (type);
		//If the player alerted the enemy, extend the chase range to three times normal
		chaseIfInSight (chaseRange * 3);
	}

	protected override void Die() {
		base.Die ();
		EnemyAnimator.Play("HadesDeath");
	}
		
	void StartPath(){	
		//Initialize the walk
		MynavMeshAgent.speed = idleSpeed;
		EnemyAnimator.speed = 1.0F;
		MynavMeshAgent.acceleration = MynavMeshAgent.speed * 100;
		MynavMeshAgent.isStopped = false;
		MynavMeshAgent.SetDestination (startNode.GetPosition ());
		EnemyAnimator.SetBool ("Attack", false);
	}

	void ContinuePath(){

		//Consider the destination reached if is less than
		const float reachedIfLessThan = 0.1f;

		//Chase the player if is within chaseRange and there is no wall blocking the view
		chaseIfInSight (chaseRange);

		//Check if Hades reached a node
		if (Vector3.Distance (transform.position, MynavMeshAgent.destination) < reachedIfLessThan) {
			startNode = startNode.nextNode;
			MynavMeshAgent.SetDestination (startNode.GetPosition ());
		}

		//Play walking sound
		if (Sounds != null && AudioWalk != null && !Sounds.isPlaying)
			Sounds.PlayOneShot (AudioWalk);

		FaceForward ();
	}

	void EndPath(){	
		
	}

	void StartChase(){	
		//Initialize the chase
		MynavMeshAgent.speed = chaseSpeed;	//increase speed
		MynavMeshAgent.acceleration = MynavMeshAgent.speed / 2;
		MynavMeshAgent.isStopped=false;	//resume Movement with icreased speed
		EnemyAnimator.SetFloat("Run",0.0f);
		EnemyAnimator.SetBool ("Attack", false);
		endChaseTime = Time.time + chaseTime;

		//Set the step sound
		if (Sounds != null && AudioWalk != null && AudioChase != null) {
			Sounds.clip = AudioWalk;
		}
	}

	void ContinueChase(){	

		float speed = MynavMeshAgent.velocity.magnitude;
		bool playerUnsafe = !isPlayerInStrongLight();

		//Continue the chase if the player is not safe in the light and not enough time has passed
		if (isPlayerInSight (chaseRange) && playerUnsafe) {
			endChaseTime = Time.time + chaseTime;
		}

		FaceForward ();

		//Switch between walk and run animations at appropriate speeds
		if (speed < 4f && playerUnsafe) {
			EnemyAnimator.SetFloat ("Run", 0.0f); //walk animation
			EnemyAnimator.speed = speed / 2.0f;
		} else if (playerUnsafe) {
			EnemyAnimator.SetFloat ("Run", 1.2f); //run animation
			EnemyAnimator.speed = 2.0f + (speed - 4f)*0.1f;
		}

		//We reached the player, time to atttack
		if (Time.time < endChaseTime && target && playerUnsafe) {
			if (Vector3.Distance (target.position, transform.position) < attackRange) {
				EnemyAnimator.speed = 1.0F;
				SetState (EnemyState.ATTACK);
			}
			else {
				MynavMeshAgent.SetDestination (target.position);
			}
		} 
		else { //The player is not reachable, switch to IDLE
			Vector3 backToNode = startNode.transform.position;
			MynavMeshAgent.SetDestination (backToNode);
			EnemyAnimator.SetBool ("Attack", false);
			EnemyAnimator.SetFloat ("Run", 0.0f);
			SetState (EnemyState.IDLE);	
		}

		//Play the walking sound
		if (Sounds != null && AudioWalk != null && !Sounds.isPlaying) {
			Sounds.PlayOneShot (AudioWalk);
		}
			
	}

	void EndChase(){	
	}

	void StartAttack(){
		//Initialize the attack
		FaceTarget ();
		MynavMeshAgent.isStopped=true;	//stop enemy movement to do attack
		MynavMeshAgent.velocity = Vector3.zero;
		EnemyAnimator.SetBool ("Attack", true);
		target.GetComponent<Player> ().DamagePlayer (damagePerSecond * Time.deltaTime);
		endAttackTime = Time.time + attackTime;
	}

	void ContinueAttack(){
		//face the player
		FaceTarget ();
		if (target && Vector3.Distance (target.position, transform.position) <= attackRange) {
			//Damage the player and renew the attack ending time
			target.GetComponent<Player> ().DamagePlayer (damagePerSecond * Time.deltaTime);
			endAttackTime = Time.time + attackTime;
		} 
		else if (Time.time > endAttackTime) {
			//Attack ended, switch to chase
			EnemyAnimator.SetBool ("Attack", false);
			SetState (EnemyState.CHASE);
		}	
	}

	void EndAttack(){
	}
		
	void DeadState(){
		if (Sounds != null && AudioChase != null) {
			Sounds.PlayOneShot (AudioChase);
		}
		//Die in the correct orientation
		FaceForward ();
		MynavMeshAgent.enabled = false;
		MynavMeshAgent.velocity = Vector3.zero;
	}

	void DeadStateUpdate(){
	}

	void FaceForward(){
		Vector3 dir;
		float angle;
		dir = MynavMeshAgent.velocity;
		if (dir.magnitude != 0) {
			angle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3 (0, 0, angle);
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

