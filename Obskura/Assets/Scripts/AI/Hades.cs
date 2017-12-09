using UnityEngine;
using UnityEngine.AI;

public class Hades : Enemy, ICollidableActor2D
{
	public float chaseTime = 10f;	//how long enemy chase player after detection
	public float chaseRange = 15f;	//how far away can the enemy see the player	
	public float attackTime = 0.5f;
	public float attackRange = 1f;

	public float idleSpeed = 1.0f;
	public float chaseSpeed = 10f;	//how long enemy chase player after detection
	//public Animator EnemyAnimator;	//for enemy animation
	public NavMeshAgent MynavMeshAgent;
	public EnemyPath startNode;		//start node to move until enemy detection, set in INSPECTOR

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

		//STATES:
		states.Add (EnemyState.IDLE, new EnemyBehaviour (StartPath, ContinuePath, EndPath));
		states.Add (EnemyState.CHASE, new EnemyBehaviour (StartChase, ContinueChase, EndChase));
		states.Add (EnemyState.ATTACK, new EnemyBehaviour (StartAttack, ContinueAttack, EndAttack));
		states.Add (EnemyState.DEAD, new EnemyBehaviour (DeadState, DeadStateUpdate, None));

		base.Start ();
	}

	// Update is called once per frame
	protected override void Update () {
		base.Update ();

		//updateState ();	//call the update method of every state, depending on the current state (Continue whatever your are doing all the time)

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
		MynavMeshAgent.speed = idleSpeed;
		EnemyAnimator.speed = 1.0F;
		MynavMeshAgent.acceleration = MynavMeshAgent.speed * 100;
		MynavMeshAgent.isStopped = false;
		MynavMeshAgent.SetDestination (startNode.GetPosition ());
	}

	void ContinuePath(){

		const float reachedIfLessThan = 0.1f;

		chaseIfInSight (chaseRange);

		if (Vector3.Distance (transform.position, MynavMeshAgent.destination) < reachedIfLessThan) {
			startNode = startNode.nextNode;
			MynavMeshAgent.SetDestination (startNode.GetPosition ());
		}

		FaceForward ();
	}

	void EndPath(){	
		
	}

	void StartChase(){	
		MynavMeshAgent.speed = chaseSpeed;	//increase speed
		MynavMeshAgent.acceleration = MynavMeshAgent.speed / 4;
		MynavMeshAgent.isStopped=false;	//resume Movement with icreased speed
		//EnemyAnimator.SetFloat("Run",1.2f);
		EnemyAnimator.SetFloat("Run",0.0f);
		endChaseTime = Time.time + chaseTime;
	}

	void ContinueChase(){	

		float speed = MynavMeshAgent.velocity.magnitude;
		//float speedProportion = speed / chaseSpeed;

		if (isPlayerInSight (chaseRange)) {
			endChaseTime = Time.time + chaseTime;
		}

		FaceForward ();

		if (speed < 4f) {
			EnemyAnimator.SetFloat ("Run", 0.0f);
			EnemyAnimator.speed = speed / 2.0f;
		} else {
			Debug.Log ("Running");
			EnemyAnimator.SetFloat ("Run", 1.2f);
			EnemyAnimator.speed = 2.0f + (speed - 4f)*0.1f;
		}


		/*if (speedProportion < 0.5F) {
			Debug.Log (speed);
			EnemyAnimator.SetFloat ("Run", 0.0f);
			EnemyAnimator.speed = speed;
		} else if (speedProportion < 0.8F) {
			EnemyAnimator.SetFloat ("Run", 1.2f);
			EnemyAnimator.speed = speedProportion + 0.2F;
		} else {
			EnemyAnimator.SetFloat ("Run", 1.2f);
			EnemyAnimator.speed = 1.0F;
		}*/

		if (Time.time < endChaseTime && target) {
			if (Vector3.Distance (target.position, transform.position) < attackRange) {
				EnemyAnimator.speed = 1.0F;
				SetState (EnemyState.ATTACK);
			}
			else {
				MynavMeshAgent.SetDestination (target.position);
			}
		} 
		else {
			Vector3 backToNode = startNode.transform.position;
			MynavMeshAgent.SetDestination (backToNode);
			EnemyAnimator.SetFloat ("Run", 0.0f);
			SetState (EnemyState.IDLE);			
		}
	}

	void EndChase(){	
	}

	void StartAttack(){
		FaceTarget ();
		MynavMeshAgent.isStopped=true;	//stop enemy movement to do attack
		MynavMeshAgent.velocity = Vector3.zero;
		EnemyAnimator.SetBool ("Attack", true);
		target.GetComponent<Player> ().DamagePlayer (damagePerSecond * Time.deltaTime);
		endAttackTime = Time.time + attackTime;
	}

	void ContinueAttack(){
		if (target && Vector3.Distance (target.position, transform.position) <= attackRange) {
			target.GetComponent<Player> ().DamagePlayer (damagePerSecond * Time.deltaTime);
			endAttackTime = Time.time + attackTime;
			FaceTarget ();
		} 
		else if (Time.time > endAttackTime) {
			EnemyAnimator.SetBool ("Attack", false);
			SetState (EnemyState.CHASE);
		}	
	}

	void EndAttack(){
	}

	void DeadState(){
		FaceForward ();
		MynavMeshAgent.enabled = false;
		MynavMeshAgent.isStopped = true;	//stop enemy movement to do attack
		MynavMeshAgent.velocity = Vector3.zero;
	}

	void DeadStateUpdate(){
		//FaceTarget ();
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

