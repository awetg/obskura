using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState{IDLE,CHASE,ATTACK,DEAD,NONE}	//describes what state the enemy is on
public enum EnemyAlert{GENERIC,LIGHT,DAMAGE}
public delegate void StateFunction();

/// <summary>
/// Describes a behaviour by three different delegates, one to call when entering, one continously and the last at the exit of the behaviour
/// </summary>
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

/// <summary>
/// Generic  enemy.
/// </summary>
public abstract class Enemy : MonoBehaviour {
	
	public float enemyHp = 0;
	public float damagePerSecond = 0;
	public Animator EnemyAnimator;	//for enemy animation
	public AudioSource Sounds;
	private EnemyState currentState = EnemyState.NONE;	//Which behaviour is the enemy adopting right now
	public EnemyState startState = EnemyState.IDLE;	//Behaviour at the start
	protected Transform target; //What to target
	private Player targetPlayer; //Player to target

	//Map of states and respective behaviours defined for the enemy
	protected Dictionary<EnemyState, EnemyBehaviour> states = new Dictionary<EnemyState, EnemyBehaviour> ();

	//Fields to destroy the enemy after it died and a delay has passed
	protected float destroyAfter = -1f;
	private float destroyAt = 0f;
	private bool destroy = false;

	//Name of the effect to show when in light
	protected string showEffectInLight = "SmokeEffect";

	//To control the light effect
	bool isLightEffectPlaying = false;
	float stopLightEffectAt = 0F;

	protected void None() {
		//Function for state "do nothing"
	}

	//The next three functions are helpers to extract the needed delegate from the states table

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

		//Acquire components
		states.Add (EnemyState.NONE, new EnemyBehaviour (None, None, None));
		SetState(startState);	//set state to idle from none
		target = GameObject.FindGameObjectWithTag("Player").transform;
		targetPlayer = target.GetComponent<Player> ();
		//Add itself to the cache (used to speed up detection)
		Tags.CacheAdd (gameObject);
	}
	
	// Update is called once per frame
	protected virtual void Update () {

		//Manage the light effect and the disposal of the object in case of death

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

	/// <summary>
	/// Causes the enemy to die.
	/// </summary>
	protected virtual void Die() {
		if (destroyAfter > 0) {
			destroyAt = Time.time + destroyAfter;
			destroy = true;
		}
		SetState (EnemyState.DEAD);
		Tags.CacheRemove (gameObject);
	}

	/// <summary>
	/// Alert the enemy. Implement is daughter classes.
	/// </summary>
	/// <param name="type">Type.</param>
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

	/// <summary>
	/// Set the enemy on fire.
	/// </summary>
	public void SetOnFire() {
		//FIXME: Implement in the future
	}

	/// <summary>
	/// Ask the player if he is in strong light.
	/// </summary>
	/// <returns><c>true</c>, if player in strong light, <c>false</c> otherwise.</returns>
	protected bool isPlayerInStrongLight(){
		if (target) {
			return targetPlayer.IsInStrongLight ();
		}
		return false;
	}

	/// <summary>
	/// Is the player in sight? (The sight is not blocked by a wall)
	/// </summary>
	/// <returns><c>true</c>, if player is in sight, <c>false</c> otherwise.</returns>
	/// <param name="sightDistance">Sight distance.</param>
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

	/// <summary>
	/// Chases if in sight.
	/// </summary>
	/// <param name="sightDistance">Sight distance.</param>
	protected void chaseIfInSight(float sightDistance){
		if (isPlayerInSight(sightDistance))
			SetState (EnemyState.CHASE);
	}

	/// <summary>
	/// Gets the current state (behaviour) of tthe enemy.
	/// </summary>
	/// <returns>The current state.</returns>
	public EnemyState GetCurrentState(){
		return currentState;
	}

	/// <summary>
	/// Damage the enemy.
	/// </summary>
	/// <param name="damage">Damage.</param>
	public virtual void GetDamaged(float damage){
		//Apply the damage, scaling them by diffculty level
		enemyHp -= damage * (1F /  GameData.GetDifficultyMultiplier());
		Alert (EnemyAlert.DAMAGE);
	}

	/// <summary>
	/// The enemy is damaged by light.
	/// </summary>
	/// <param name="damage">Damage.</param>
	public virtual void GetDamagedByLight(float damage){
		if (!isLightEffectPlaying)
			InLightEffect (true);
		
		stopLightEffectAt = Time.time + 0.5f;

		enemyHp -= damage * (1F / GameData.GetDifficultyMultiplier());
		Alert (EnemyAlert.LIGHT); //If damaged by light, alert the enemy
	}

	/// <summary>
	/// Turn towards the target
	/// </summary>
	/// <param name="offsetAngle">Offset angle to add to the resultt.</param>
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

	/// <summary>
	/// Turn in a random direction
	/// </summary>
	protected void FaceRandom(){
		System.Random rnd = new System.Random ();
		//Create a random angle
		float angle = (float)(rnd.NextDouble() * Mathf.PI * 2 -  Mathf.PI);
		angle = angle * Mathf.Rad2Deg;
		transform.eulerAngles = new Vector3 (0, 0, angle);	
	}

	/// <summary>
	/// Activate or disactivate the effect that plays in the light.
	/// </summary>
	/// <param name="active">If set to <c>true</c> active.</param>
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
