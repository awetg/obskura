using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Obskura enemy.
/// </summary>
public class Obskura : Enemy, ICollidableActor2D {

	//Parameters affecting the dimming caused by obskura
	public float AlertTime = 10f;
	public float MaxDimmingAfter = 5f;
	public float DimmingFactor = 0.1f;

	public AudioSource AudioDark; //Audio to play when obskura acts

	OLightManager lightManager; //Light manager to reduce the light

	//When the dimming stops and reaches the maximum
	float stopAlertAt = 0f;
	float maxDimmingAt = 0f;

	//Which proportion of the reference intensity the darkness will reach
	float darknessProportion = 1.0f;
	float referenceIntensity = 10f;

	// Use this for initialization
	protected override void Start () {
		SetState(startState);	//set state to idle from none

		//Retrive the camera to get the reference intensity
		var camera = GameObject.FindGameObjectWithTag ("MainCamera");

		if (camera != null) {
			var lm = camera.GetComponent<OLightManager> ();
			if (lm != null) {
				lightManager = lm;
				referenceIntensity = lightManager.Intensity;
			}
		}

		//Override here the default values from Enemy
		//Note: If they have been set in the inspector, their value will be != 0, so don't override
		if (enemyHp == 0)
			enemyHp = 50f;		
		if (damagePerSecond == 0)
			damagePerSecond = 50f;

		//Which effectt to show when in lightt
		showEffectInLight = "ObskuraEffect";

		//Destroy 0.1 seconds after death
		destroyAfter = 0.1f;

		//STATES TABLE:
		//To every state associate a behaviour, composed of an init, an update and an end delegate
		states.Add (EnemyState.IDLE, new EnemyBehaviour (StartIdle, None, None));
		states.Add (EnemyState.CHASE, new EnemyBehaviour (None, None, None));
		states.Add (EnemyState.ATTACK, new EnemyBehaviour (StartAttack, ContinueAttack, EndAttack));
		states.Add (EnemyState.DEAD, new EnemyBehaviour (None, None, None));

		base.Start ();
	}
		
	void StartIdle(){
		//Idle animation	
		EnemyAnimator.Play ("ObskuraAnim");
	}

	void StartAttack(){
		//Initialize the attack
		maxDimmingAt = Time.time + MaxDimmingAfter;
		darknessProportion = 1.0f;
		if (AudioDark != null && !AudioDark.isPlaying) {
			AudioDark.PlayOneShot (AudioDark.clip);
		}
	}

	void ContinueAttack(){
		//Alert time elapsed, back to idle
		if (Time.time > stopAlertAt) {
			SetState (EnemyState.IDLE);
			return;
		}

		//Calculate a time progressive dimming factor
		if (Time.time < maxDimmingAt) {
			darknessProportion = DimmingFactor + (1.0f - DimmingFactor) * (maxDimmingAt - Time.time) / MaxDimmingAfter;
		} else
			darknessProportion = DimmingFactor;

		//Reduces the intensity of the lights in the map
		lightManager.Intensity = referenceIntensity * darknessProportion;
	}

	//Restore the lights to reference intensity
	void EndAttack(){
		lightManager.Intensity = referenceIntensity;
		Debug.Log (referenceIntensity);
	}

	//Alert the obskura
	public override void Alert (EnemyAlert type)
	{
		base.Alert (type);
		stopAlertAt = Time.time + AlertTime;
		//If alerted dim the lights.
		if (GetCurrentState() != EnemyState.ATTACK)
			SetState (EnemyState.ATTACK);
	}

	protected override void Die() {
		base.Die ();
		lightManager.Intensity = referenceIntensity;
	}

	public override void GetDamagedByLight (float damage)
	{
		//Receive health when damaged by light
		//NOTE: Since the difficulty multiplier(dm) scales the damage to every enemy, we need to multiply
		//      by dm^2 to scale a negative damage (health increment) appropriately
		//      see (1/dm) * dm^2 = dm -> Increment of negative damage with difficulty
		base.GetDamagedByLight (-damage * 0.2f * GameData.GetDifficultyMultiplier() * GameData.GetDifficultyMultiplier());
	}

	//*** ICollidableActor2D Implementation ***

	public float GetSize() {
		return 2f;
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
