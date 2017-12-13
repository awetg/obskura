using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : Item {

	public GameController Controller;

	// Use this for initialization
	void Start () {
		DestroyAfterTrigger = true;
		TriggerOnlyOnUse = false;
	}

	protected override void Action (Player player)
	{
		Controller.ConcludeGame ();
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position,0.5f);
	}

	void OnDrawGizmosSelected(){
		Gizmos.DrawWireSphere (transform.position, TriggerDistance);
	}
}

