using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Item that can be triggere by proximity or use key.
/// </summary>
public abstract class Item : MonoBehaviour {

	public bool DestroyAfterTrigger = true;
	public bool TriggerOnlyOnUse = false;
	public float TriggerRechargeAfter = 1.0F;
	public float TriggerDistance = 2.0F;

	bool triggered = false;
	//At which point the trigger will be active again
	float rechargeAt = 0.0f;


	// Update is called once per frame
	protected void Update () {

		//Already triggered, need to destroy
		if (triggered && DestroyAfterTrigger) {
			return;
		}

		//Wait for the trigger to recharge
		if (Time.time < rechargeAt)
			return;

		//Check if the player is near enough to pick up
		var players = Tags.CachedGameObjectsWithTagInRange("Player", transform.position, TriggerDistance);

		//All the player objects...
		foreach (GameObject playerObj in players) {
			Player player = playerObj.GetComponent<Player> ();

			if (player != null){
				//...check if one of them satisfies the conditions for the trigger
				if (!TriggerOnlyOnUse || (TriggerOnlyOnUse && player.IsPressingUseKey())) {
					Action (player);
					triggered = true;
					rechargeAt = Time.time + TriggerRechargeAfter;
				}
			}
		}

		//Remove the trigger if set to do so
		if (triggered && DestroyAfterTrigger) {
			Destroy (gameObject);
		}

	}

	//Action to be implemented by derived classes
	protected abstract void Action (Player player);

}
