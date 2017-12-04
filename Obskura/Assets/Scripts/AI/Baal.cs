/// <summary>
/// Enemy
/// Thanks to this https://docs.unity3d.com/Manual/NavMesh-BuildingComponents.html , we cand have navmesh in XY plane (2D)
/// </summary>

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Baal : Enemy {
	
	public float chaseTime = 10f;	//how long enemy chase player after detection

	public Vector3 centrePosition;
	private float nextTeleportTime=0;
	public float minTime = 1;
	public float maxTime =5;
	public float maxDistance = 3;
	public float chasingSpeed = 0.5F;
	public float sightDistance = 50F;
	public float attackRange = 2f;

	float endChaseTime = 0;

	// Use this for initialization
	void Start () {
		SetState(startState);	//set state to idle from none
		centrePosition = transform.position;
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

		base.Start ();
	}
		

	void StartIdle(){	
		
	}

	//Teleport code written by Elsa
	void TeleportIfTime(){

		if (Time.time > nextTeleportTime) 
		{	
			System.Random rnd = new System.Random();
			Vector3 newpos;
			const int maxTry = 100;
			int count = 0;

			do {
				if (count > maxTry)
					Die();
				//teleport code
				float dx = (float)rnd.NextDouble () * maxDistance; 
				float dy = (float)rnd.NextDouble () * maxDistance;
				newpos = centrePosition + new Vector3 (dx, dy, 0);
				count += 1;
			} while (Geometry.IsPointInAWall (newpos));
			//check for collisions
			//...
			transform.position = newpos;

			float interval = (float)(rnd.NextDouble () * (maxTime-minTime)) + minTime; 
			nextTeleportTime = Time.time + interval;

		}
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
		FaceTarget ();
	}

	void ContinueChase(){	

		if (Time.time<endChaseTime && target) {

			chaseIfInSight (sightDistance);

			Vector3 direction = (target.position - centrePosition).normalized;
			centrePosition = centrePosition + direction * Time.deltaTime * chasingSpeed;

			Debug.DrawLine (centrePosition, centrePosition + new Vector3 (0.2f, 0.2f, 0), Color.green, 100f);

			FaceTarget ();

			if (Vector3.Distance (target.position, transform.position) < sightDistance)
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
		target.GetComponent<PlayerMovement> ().DamagePlayer (damagePerSecond);
	}

	void ContinueAttack(){
		if (target && Vector3.Distance (target.position, transform.position) <= attackRange) {
			target.GetComponent<PlayerMovement> ().DamagePlayer (damagePerSecond * Time.deltaTime);
			FaceTarget ();
		}
		else {
			//EnemyAnimator.SetBool ("Attack", false);
			SetState (EnemyState.CHASE);
		}	
	}


	void EndAttack(){
	}
		



		
//	void Damage(){
//
//		RaycastHit[] hits=Physics.SphereCastAll (AttackPivot.position,Hades_weaponRange, AttackPivot.forward);
//		foreach(RaycastHit hit in hits)
//			if (hit.collider!=null && hit.collider.tag == "Player")
//				hit.collider.GetComponent<PlayerMovement>().DamagePlayer(20);
//	}


}
