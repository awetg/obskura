/// <summary>
/// Enemy
/// Thanks to this https://docs.unity3d.com/Manual/NavMesh-BuildingComponents.html , we cand have navmesh in XY plane (2D)
/// </summary>

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState{IDLE,CHASE,ATTACK,NONE}	//describes what state the enemy is on

public class Enemy : MonoBehaviour {
	
	int enemyHp;
	public float chaseTime;	//how long enemy chase player after detection
	public Animator EnemyAnimator;	//for enemy animation
	public NavMeshAgent MynavMeshAgent;
	public EnemyPath startNode;		//start node to move until enemy detection, set in INSPECTOR
	private EnemyState currentState = EnemyState.NONE;	//needed variable for control of states
	public EnemyState startState = EnemyState.IDLE;	//can be made public to choose from INSPECTOR
	float Hades_weaponRange = 1.0f;	// this enemy don't have weapon, need to be close to register damage
	private float distance;
	private Vector3 targetPosition;
	private Transform target;

	delegate void State();	//you gotta read about this, very intersting stuff
	State intialState;
	State updateState;
	State endState;

	// Use this for initialization
	void Start () {
		SetState(startState);	//set state to idle from none
		MynavMeshAgent = GetComponent<NavMeshAgent>();	//initialize navMeshAgnet
		target= GameObject.FindGameObjectWithTag("Player").transform;

	}
	
	// Update is called once per frame
	void Update () {
		updateState ();	//call the update method of every state, depending on the current state (Continue whatever your are doing all the time)
		transform.rotation = Quaternion.Euler(0,180 , 0);	//lock rotation in x and z , so that sprite can be seen, sprite is inside a capsule
//		MynavMeshAgent.updateRotation=false;
		
	}

	/// IDLE:  follow designeted path until player detection ///


	void StartPath(){	
		MynavMeshAgent.speed = 1.0f;
		MynavMeshAgent.SetDestination (startNode.GetPosition ());
	}

	void ContinuePath(){	
		if (Vector3.Distance (transform.position, MynavMeshAgent.destination)<0.2) {
			startNode = startNode.nextNode;
			MynavMeshAgent.SetDestination (startNode.GetPosition ());
		}
	}

	void EndPath(){		//NULL
	}

	/// IDLE : END ///	

	///// CHASE ////

	public float timer;	//public for debug

	void StartChase(){	
		MynavMeshAgent.speed = 1.2f;	//increase speed
		MynavMeshAgent.isStopped=false;	//resume Movement with icreased speed
		timer = 0.0f;
	}

	void ContinueChase(){	
		timer += Time.deltaTime;
		if (target && timer<chaseTime) {

			if (Vector3.Distance (target.position, transform.position) < Hades_weaponRange)
				SetState (EnemyState.ATTACK);
			else
				MynavMeshAgent.SetDestination (target.position);

		} else {
			Vector3 backToNode = startNode.transform.position;
			MynavMeshAgent.SetDestination (backToNode);
			SetState (EnemyState.IDLE);
		}
	}


	void EndChase(){	
	}

		/// CHASE : END ///

	/// ATTACK ///

	float attackTime;

	void StartAttack(){
		MynavMeshAgent.isStopped=true;	//stop enemy movement to do attack
		MynavMeshAgent.velocity = Vector3.zero;
		//		EnemyAnimator.SetBool ("Attack", true);
		target.GetComponent<PlayerMovement> ().DamagePlayer (2);
		attackTime = 2.0f;
	}

	void ContinueAttack(){
		attackTime -= Time.deltaTime;
		if(attackTime > 0){

			if (Vector3.Distance (target.position, transform.position) < Hades_weaponRange)
				target.GetComponent<PlayerMovement> ().DamagePlayer (2);
			else
				EndAttack ();
			SetState (EnemyState.CHASE);
		}
	}


	void EndAttack(){
		//		EnemyAnimator.SetBool ("Attack", false);
	}

	/// ATTACK : END ///

	/// <summary>
	/// Sets the state of the enemy, callled in start() method to set from NONE to IDLE at first
	/// </summary>
	/// <param name="newState">New State.</param>
	public void SetState(EnemyState newState){
		if (currentState != newState) {	//if state changes do all this, initial methods are called every time state changes
			if (endState != null)
				endState ();
			switch (newState) {

			case EnemyState.IDLE:
				intialState = StartPath;
				updateState = ContinuePath;
				endState = EndPath;
				break;

			case EnemyState.CHASE:
				intialState = StartChase;
				updateState = ContinueChase;
				endState = EndChase;
				break;

			case EnemyState.ATTACK:
				intialState = StartAttack;
				updateState = ContinueAttack;
				endState = EndAttack;
				break;
			}

			intialState();	//call the intial method of the state, only if the states chages, as the conditon is set in if()		
			currentState=newState;
		}
	}
		

	/// <summary>
	/// Damage this enemy, destroy if hp is =0, damage are sent by player if he registered damage
	/// </summary>
	/// <param name="new damage value"> new damage. </param>
	public void DamageEnemy(int damage){

		enemyHp -= damage;	//register incoming damage

		if (enemyHp <= 0) {	
			MynavMeshAgent.isStopped=true;	//stop enemy movemnt
			MynavMeshAgent.velocity = Vector3.zero;
			//			EnemyAnimator.SetBool ("Dead", true);	//trigger dead animation
			//			EnemyAnimator.transform.parent = null;	//stop animation
			Destroy (gameObject);	//destroy object
		}
	}
		
//	void Damage(){
//
//		RaycastHit[] hits=Physics.SphereCastAll (AttackPivot.position,Hades_weaponRange, AttackPivot.forward);
//		foreach(RaycastHit hit in hits)
//			if (hit.collider!=null && hit.collider.tag == "Player")
//				hit.collider.GetComponent<PlayerMovement>().DamagePlayer(20);
//	}


}
