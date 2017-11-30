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
	Vector3 targetPosition;
	public EnemyPath startNode;		//start node to move until enemy detection, set in INSPECTOR
	EnemyState currentState = EnemyState.NONE;	//needed variable for control of states
	public EnemyState startState = EnemyState.IDLE;	//can be made public to choose from INSPECTOR
	public LayerMask hitTestLayer;
	float Hades_weaponRange = 1.0f;	// this enemy don't have weapon, need to be close to register damage
	public Transform AttackPivot;
	public Transform target;
	private float distance;

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

	/// <summary>
	/// Starts the path. Called only once when state changes to IDLE
	/// </summary>
	void StartPath(){	
		MynavMeshAgent.speed = 1.0f;
		MynavMeshAgent.SetDestination (startNode.GetPosition ());
	}

	void ContinuePath(){	
		if (ReachedMyDestination ()) {
			startNode = startNode.nextNode;
			MynavMeshAgent.SetDestination (startNode.GetPosition ());
		}
	}

	void EndPath(){		//NULL
	}

	/// IDLE : END ///	

	///// CHASE ////

	bool stopchase;

	/// <summary>
	/// Starts the chase. Called once only when state change to CHASE
	/// </summary>
	void StartChase(){	
		MynavMeshAgent.speed = 1.0f;	//increase speed
		MynavMeshAgent.isStopped=false;	//resume Movement with icreased speed
	}

	void ContinueChase(){	
		if (target) {
			MynavMeshAgent.SetDestination (target.position);
		} else {
			if (target == null) {
				target = this.gameObject.GetComponent<Transform> ();
			} else {
				target = GameObject.FindGameObjectWithTag ("Player").transform;
			}
		}
	}


	void EndChase(){	
	}

		/// CHASE : END ///

	/// ATTACK ///


	/// ATTACK : END ///


	void Damage(){
		
		RaycastHit[] hits=Physics.SphereCastAll (AttackPivot.position,Hades_weaponRange, AttackPivot.forward);
			foreach(RaycastHit hit in hits)
				if (hit.collider!=null && hit.collider.tag == "Player")
				hit.collider.GetComponent<PlayerMovement>().DamagePlayer(20);
	}

		
	/// <summary>
	/// Chech if target position reached.
	/// </summary>
	/// <returns><c>true</c>, if my destination was reacheded, <c>false</c> otherwise.</returns>
	public bool ReachedMyDestination(){
		float distance = Vector3.Distance (transform.position, MynavMeshAgent.destination);
		if ( distance<= 1.5f) {
			return 	true;
		}

		return false;
	}

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

//			case EnemyState.ATTACK:
//				intialState = StartAttack;
//				updateState = ContinueAttack;
//				endState = EndAttack;
//				break;
			}

			intialState();	//call the intial method of the state, only if the states chages, as the conditon is set in if()		
			currentState=newState;
		}
	}










	/// WE ARE NOT USING THOSE FUNCTIONS BELOW FOR THE TIME BEING ///


	/// <summary>
//	/ Sets the alert position. public called from player, when player is near enemy
//	/ </summary>
//	/ <param name="newPosition">New position.</param>
	public void SetAlertPosition(Vector3 newPosition){
		if (startState != EnemyState.NONE) {
			SetTargetPosition(newPosition);
		}
	}

	/// <summary>
	/// Sets the target position. Only when the enemy is in chase state
	/// </summary>
	/// <param name="newPosition">New position.</param>
	public void SetTargetPosition(Vector3 newPosition){
		targetPosition = newPosition;
		if (currentState != EnemyState.ATTACK ) {
			SetState (EnemyState.CHASE);
		}
	}

	/// <summary>
	/// Damage this enemy, destroy if hp is =0, damage are sent by player if he registered damage
	/// </summary>
	/// <param name="new damage value"> new damage. </param>
	public void DamageEnemy(int damage){
		
		enemyHp -= damage;	//register incoming damage

		if (enemyHp <= 0) {	//check if hp reached zero and destroy object, if not, just continue game
			MynavMeshAgent.isStopped=true;	//stop enemy movemnt
			EnemyAnimator.SetBool ("Dead", true);	//trigger dead animation
			EnemyAnimator.transform.parent = null;	//stop animation
			Destroy (gameObject);	//destroy object
		}
	}

	/// <summary>
	/// Randoms the rotate. Make enemy rotate in random direction in CHASE state
	/// </summary>
	void RandomRotate(){
		float randomAngle =Random.Range (45, 180);
		transform.Rotate (0, randomAngle, 0);
	}

}
