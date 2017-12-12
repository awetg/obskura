using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Item : MonoBehaviour {

	public bool DestroyAfterTrigger = true;
	public bool TriggerOnlyOnUse = false;
	public float TriggerRechargeAfter = 1.0F;
	public float TriggerDistance = 2.0F;
	public Typer typer;
	public string Message;
	public Canvas dialogueBox;
	CanvasGroup canvasGroup;

	bool triggered = false;
	float rechargeAt = 0.0f;


	void Start(){
		dialogueBox.gameObject.SetActive (false);
		canvasGroup = dialogueBox.GetComponent<CanvasGroup> ();
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
			StartCoroutine (typer.TypeIn ());
			StartCoroutine (FadeIn ());
			StartCoroutine (lateDeactivate ());	// Fadeout and late deactivate
		}
	}

	public IEnumerator lateDeactivate()
	{
		yield return new WaitForSeconds (40.0f);	//wait for time until message read about 20 seconds

		float time = 1f;
		while(canvasGroup.alpha > 0)	//fade out
		{
			canvasGroup.alpha -= Time.deltaTime / time;
			yield return null;
		}

		dialogueBox.gameObject.SetActive (false);
	}

	public IEnumerator FadeIn(){
		float time = 1f;
		while(canvasGroup.alpha < 1)
		{
			canvasGroup.alpha += Time.deltaTime / time;
			yield return null;
		}
	}

	public void typerButtonclick()
	{
		dialogueBox.gameObject.SetActive (false);

	}
}
