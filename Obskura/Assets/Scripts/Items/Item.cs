using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

	public bool DestroyAfterTrigger = true;
	public bool TriggerOnlyOnUse = true;
	public float TriggerRechargeAfter = 1.0F;
	public float TriggerDistance = 2.0F;

	bool triggered = false;
	float rechargeAt = 0.0f;

	// Update is called once per frame
	protected void Update () {

		if (Time.time < rechargeAt)
			return;

		//Check if the player is near enough to pick up
		var players = Tags.CachedGameObjectsWithTagInRange("Player", transform.position, TriggerDistance);

		foreach (GameObject playerObj in players) {
			Player player = playerObj.GetComponent<Player> ();

			if (player != null){
				if (!TriggerOnlyOnUse || (TriggerOnlyOnUse && player.IsPressingUseKey())) {
					Action (player);
					triggered = true;
					rechargeAt = Time.time + TriggerRechargeAfter;
				}
			}
		}

		if (triggered && DestroyAfterTrigger)
			Destroy (gameObject);

	}

	protected abstract void Action (Player player);
}
