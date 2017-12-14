/// <summary>
/// Enemy
/// Thanks to this https://docs.unity3d.com/Manual/NavMesh-BuildingComponents.html , we cand have navmesh in XY plane (2D)
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState{IDLE,CHASE,ATTACK,DEAD,NONE}	//describes what state the enemy is on
public enum EnemyAlert{GENERIC,LIGHT,DAMAGE}
public delegate void StateFunction();

public struct EnemyBehaviour {
	public StateFunction initialState;
	public StateFunction updateState;
	public StateFunction endState;

	public EnemyBehaviour(StateFunction init, StateFunction update, StateFunction end){
		initialState = init;
		updateState = update;
		endState = end;
	}
}

public abstract class Enemy : MonoBehaviour {
	
	public float enemyHp = 0;
	public float damagePerSecond = 0;
	public Animator EnemyAnimator;	//for enemy animation
	public AudioSource Sounds;
	private EnemyState currentState = EnemyState.NONE;	//needed variable for control of states
	public EnemyState startState = EnemyState.IDLE;	//can be made public to choose from INSPECTOR
	protected Transform target;
	private Player targetPlayer;

	protected Dictionary<EnemyState, EnemyBehaviour> states = new Dictionary<EnemyState, EnemyBehaviour> ();

	protected float destroyAfter = -1f;
	private float destroyAt = 0f;
	private bool destroy = false;

	protected string showEffectInLight = "SmokeEffect";
	bool isLightEffectPlaying = false;
	float stopLightEffectAt = 0F;

	protected void None() {
		//Function for state "do nothing"
	}

	private void initialState() {
		if (states.ContainsKey (currentState) && states[currentState].initialState != null) {
			states [currentState].initialState ();
		}
	}

	private void updateState() {
		if (states.ContainsKey (currentState) && states[currentState].updateState != null) {
			states [currentState].updateState ();
		}
	}

	private void endState() {
		if (states.ContainsKey (currentState) && states[currentState].endState != null) {
			states [currentState].endState ();
		}
	}

	// Use this for initialization
	protected virtual void Start () {
		var sounds = gameObject.GetComponent<AudioSource> ();

		if (sounds != null)
			Sounds = sounds;

		states.Add (EnemyState.NONE, new EnemyBehaviour (None, None, None));
		SetState(startState);	//set state to idle from none
		target = GameObject.FindGameObjectWithTag("Player").transform;
		targetPlayer = target.GetComponent<Player> ();
		Tags.CacheAdd (gameObject);
	}
	
	// Update is called once per frame
	protected virtual void Update () {

		if (isLightEffectPlaying && Time.time > stopLightEffectAt) {
			InLightEffect (false);
		}

		if (destroy && Time.time > destroyAt)
			Destroy (gameObject);

		if (!destroy && enemyHp <= 0.0f) {
			Die ();
		}

		updateState ();	//call the update method of every state, depending on the current state (Continue whatever your are doing all the time)

	}

	protected virtual void Die() {
		if (destroyAfter > 0) {
			destroyAt = Time.time + destroyAfter;
			destroy = true;
		}
		SetState (EnemyState.DEAD);
		Tags.CacheRemove (gameObject);
	}

	public virtual void Alert (EnemyAlert type){}

	/// <summary>
	/// Sets the state of the enemy, callled in start() method to set from NONE to IDLE at first
	/// </summary>
	/// <param name="newState">New State.</param>
	public void SetState(EnemyState newState){
		if (currentState != newState) {	//if state changes do all this, initial methods are called every time state changes
			endState ();
			currentState = newState;
			initialState();	//call the intial method of the state, only if the states chages, as the conditon is set in if()		
		}
	}


	public void SetOnFire() {
		//FIXME: Implement in the future
	}

	protected bool isPlayerInStrongLight(){
		if (target) {
			return targetPlayer.IsInStrongLight ();
		}
		return false;
	}

	protected bool isPlayerInSight(float sightDistance){
		if (target) {
			float minDistance = Vector3.Distance (target.position, transform.position);
			if (Geometry.IsInLineOfSight (target.position, transform.position) && minDistance < sightDistance) {
				if (!isPlayerInStrongLight())
					return true;
			}
		}
		return false;
	}

	protected void chaseIfInSight(float sightDistance){
		if (isPlayerInSight(sightDistance))
			SetState (EnemyState.CHASE);
	}

	public EnemyState GetCurrentState(){
		return currentState;
	}

	public virtual void GetDamaged(float damage){
		enemyHp -= damage * (1F /  GameData.GetDifficultyMultiplier());
		Alert (EnemyAlert.DAMAGE);
	}

	public virtual void GetDamagedByLight(float damage){
		if (!isLightEffectPlaying)
			InLightEffect (true);
		
		stopLightEffectAt = Time.time + 0.5f;

		enemyHp -= damage * (1F / GameData.GetDifficultyMultiplier());
		Alert (EnemyAlert.LIGHT);
	}

	protected void FaceTarget(float offsetAngle = 0.0F){
		Vector3 dir;
		float angle;
		if (target) {
			dir = target.position - transform.position;
			if (dir.magnitude > 0) {
				angle = (Mathf.Atan2 (dir.y, dir.x) + offsetAngle) * Mathf.Rad2Deg;
				transform.eulerAngles = new Vector3 (0, 0, angle);
			}
		}
	}

	protected void FaceRandom(){
		System.Random rnd = new System.Random ();
		float angle = (float)(rnd.NextDouble() * Mathf.PI * 2 -  Mathf.PI);
		angle = angle * Mathf.Rad2Deg;
		transform.eulerAngles = new Vector3 (0, 0, angle);	
	}

	private void InLightEffect(bool active){
		if (showEffectInLight != "") {
			foreach (Transform t in gameObject.transform) {
				if (t.gameObject.name == showEffectInLight) {
					isLightEffectPlaying = active;
					t.gameObject.SetActive (active);
				}
			}
		}
	}

}
