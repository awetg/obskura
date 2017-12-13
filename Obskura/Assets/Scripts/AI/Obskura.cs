using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obskura : Enemy, ICollidableActor2D {


	public float AlertTime = 10f;
	public float MaxDimmingAfter = 5f;
	public float DimmingFactor = 0.1f;

	OLightManager lightManager;

	float stopAlertAt = 0f;
	float maxDimmingAt = 0f;


	float darknessProportion = 1.0f;
	float referenceIntensity = 10f;

	// Use this for initialization
	protected override void Start () {
		SetState(startState);	//set state to idle from none

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

		showEffectInLight = "ObskuraEffect";

		destroyAfter = 0.1f;

		//STATES:
		states.Add (EnemyState.IDLE, new EnemyBehaviour (StartIdle, None, None));
		states.Add (EnemyState.CHASE, new EnemyBehaviour (None, None, None));
		states.Add (EnemyState.ATTACK, new EnemyBehaviour (StartAttack, ContinueAttack, EndAttack));
		states.Add (EnemyState.DEAD, new EnemyBehaviour (None, None, None));

		base.Start ();
	}

	//Teleport code written by Elsa
	//If the time delay for the next teleport has elapsed, try to teleport


	void StartIdle(){	
		EnemyAnimator.Play ("ObskuraAnim");
	}

	void StartAttack(){
		maxDimmingAt = Time.time + MaxDimmingAfter;
		darknessProportion = 1.0f;
		//referenceIntensity = lightManager.Intensity;
	}

	void ContinueAttack(){
		if (Time.time > stopAlertAt) {
			SetState (EnemyState.IDLE);
			return;
		}

		if (Time.time < maxDimmingAt) {
			darknessProportion = DimmingFactor + (1.0f - DimmingFactor) * (maxDimmingAt - Time.time) / MaxDimmingAfter;
		} else
			darknessProportion = DimmingFactor;

		lightManager.Intensity = referenceIntensity * darknessProportion;
	}

	void EndAttack(){
		lightManager.Intensity = referenceIntensity;
		Debug.Log (referenceIntensity);
	}

	public override void Alert (EnemyAlert type)
	{
		base.Alert (type);
		stopAlertAt = Time.time + AlertTime;
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
