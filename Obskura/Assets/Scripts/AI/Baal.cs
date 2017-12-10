/// <summary>
/// Enemy
/// Thanks to this https://docs.unity3d.com/Manual/NavMesh-BuildingComponents.html , we cand have navmesh in XY plane (2D)
/// </summary>

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Baal : Enemy, ICollidableActor2D {
	
	public float chaseTime = 10f;	//how long enemy chase player after detection

	public Vector3 centerPosition;
	public float minTime = 1;
	public float maxTime =5;
	public float maxDistance = 3;
	public float chasingSpeed = 0.5F;
	public float sightDistance = 50F;
	public float attackRange = 3f;

	private Vector3 originalPosition;

	float nextTeleportTime=0;
	float disableTeleportEffectAt=0;
	float endChaseTime = 0;
	bool isTeleportEffectOn = false;

	// Use this for initialization
	protected override void Start () {
		SetState(startState);	//set state to idle from none
		centerPosition = transform.position;
		//Override here the default values from Enemy
		//Note: If they have been set in the inspector, their value will be != 0, so don't override
		if (enemyHp == 0)
			enemyHp = 100f;		
		if (damagePerSecond == 0)
			damagePerSecond = 50f;
		
		destroyAfter = 0.1f;

		//STATES:
		states.Add (EnemyState.IDLE, new EnemyBehaviour (StartIdle, ContinueIdle, EndIdle));
		states.Add (EnemyState.CHASE, new EnemyBehaviour (StartChase, ContinueChase, EndChase));
		states.Add (EnemyState.ATTACK, new EnemyBehaviour (StartAttack, ContinueAttack, EndAttack));
		states.Add (EnemyState.DEAD, new EnemyBehaviour (None, None, None));

		originalPosition = centerPosition;

		base.Start ();
	}
		
	//Teleport code written by Elsa
	//If the time delay for the next teleport has elapsed, try to teleport
	void TeleportIfTime(){

		//Play the effect if less than 0.3 seconds are left
		if (!isTeleportEffectOn && nextTeleportTime - Time.time < 0.3f) {
			TeleportEffect (true);
			disableTeleportEffectAt = Time.time + 0.6f;
		}

		if (isTeleportEffectOn && Time.time > disableTeleportEffectAt)
			TeleportEffect (false);

		if (Time.time > nextTeleportTime) 
		{	
			System.Random rnd = new System.Random();
			Vector3 newpos;
			const int maxTry = 3;
			int count = 0;

			do {
				if (count > maxTry){
					newpos = originalPosition;
					break;
				}
				//teleport code
				float dx = (float)rnd.NextDouble () * maxDistance; 
				float dy = (float)rnd.NextDouble () * maxDistance;
				newpos = centerPosition + new Vector3 (dx, dy, 0);
				count += 1;
			} while (Geometry.IsPointInAWall(newpos) || Geometry.IsSquareInAWall (newpos, this.GetSize()));
			//check for collisions
			//...
			transform.position = newpos;

			float interval = (float)(rnd.NextDouble () * (maxTime-minTime)) + minTime; 
			nextTeleportTime = Time.time + interval;

		}
	}

	void StartIdle(){	
		EnemyAnimator.Play ("BaalIdle");
	}


	void ContinueIdle(){
		
	
		chaseIfInSight (sightDistance);

		TeleportIfTime ();
		

	}

	void EndIdle(){		//NULL
	}

	/// IDLE : END ///	

	///// CHASE ////



	void StartChase(){	
		//MynavMeshAgent.speed = 1.5f;	//increase speed
		//MynavMeshAgent.isStopped=false;	//resume Movement with icreased speed
		//EnemyAnimator.SetFloat("Run",1.2f);
		endChaseTime = Time.time + chaseTime;
		FaceTarget (-Mathf.PI/2);
	}

	void ContinueChase(){	

		if (Time.time<endChaseTime && target && !isPlayerInStrongLight()) {

			chaseIfInSight (sightDistance);

			TeleportIfTime ();

			Vector3 direction = (target.position - centerPosition).normalized;
			centerPosition = centerPosition + direction * Time.deltaTime * chasingSpeed;

			//Debug.DrawLine (centrePosition, centrePosition + new Vector3 (0.2f, 0.2f, 0), Color.green, 100f);

			FaceTarget (-Mathf.PI/2);

			if (Vector3.Distance (target.position, transform.position) < attackRange)
				SetState (EnemyState.ATTACK);
			else {
				//MynavMeshAgent.SetDestination (target.position);
			}

		} 
		else {
			//Vector3 backToNode = startNode.transform.position;
			//MynavMeshAgent.SetDestination (backToNode);
			//EnemyAnimator.SetFloat ("Run", 0.0f);
			SetState (EnemyState.IDLE);			
			//FaceForward ();
		}
	}


	void EndChase(){	
		FaceRandom ();
	}


	void StartAttack(){
		//MynavMeshAgent.isStopped=true;	//stop enemy movement to do attack
		//MynavMeshAgent.velocity = Vector3.zero;
		//EnemyAnimator.SetBool ("Attack", true);
		//target.GetComponent<Player> ().DamagePlayer (damagePerSecond);
	}

	void ContinueAttack(){
		if (target && Vector3.Distance (target.position, transform.position) <= attackRange) {
			//Debug.Log (Vector3.Distance (target.position, transform.position));
			target.GetComponent<Player> ().DamagePlayer (damagePerSecond * Time.deltaTime);
			FaceTarget (-Mathf.PI/2);
		}
		else {
			//EnemyAnimator.SetBool ("Attack", false);
			SetState (EnemyState.CHASE);
		}	
	}


	void EndAttack(){
	}
		

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

		
//	void Damage(){
//
//		RaycastHit[] hits=Physics.SphereCastAll (AttackPivot.position,Hades_weaponRange, AttackPivot.forward);
//		foreach(RaycastHit hit in hits)
//			if (hit.collider!=null && hit.collider.tag == "Player")
//				hit.collider.GetComponent<PlayerMovement>().DamagePlayer(20);
//	}


}
