using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

	public bool DestroyAfterTrigger = true;
	public bool TriggerOnlyOnUse = false;
	public float TriggerRechargeAfter = 1.0F;
	public float TriggerDistance = 2.0F;
	public Typer typer;
	public string Message;
	public Canvas dialogueBox;

	bool triggered = false;
	float rechargeAt = 0.0f;


	void Start(){
		dialogueBox.gameObject.SetActive (false);
//		typer = GameObject.FindGameObjectWithTag ("Paper").GetComponent<Typer>();
//		Debug.Log (typer.ToString ());
	}

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

	protected virtual void Action (Player player){
		if (Message != ""){
			dialogueBox.gameObject.SetActive (true);
			typer.message = Message;
			lateTypeOut ();
		}
	}

	public IEnumerator lateTypeOut()
	{
		yield return new WaitForSeconds (10.0f);
		dialogueBox.gameObject.SetActive (false);
	}
}
