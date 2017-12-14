using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Triggers ttthe end of the game.
/// </summary>
public class EndTrigger : Item {

	public GameController Controller;

	// Use this for initialization
	void Start () {
		DestroyAfterTrigger = true;
		TriggerOnlyOnUse = false;
	}
		
	protected override void Action (Player player)
	{
		//Call the controller to end the game
		Controller.ConcludeGame ();
	}

	//Functions to draw the object in the unity editor
	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position,0.5f);
	}

	void OnDrawGizmosSelected(){
		Gizmos.DrawWireSphere (transform.position, TriggerDistance);
	}
}

