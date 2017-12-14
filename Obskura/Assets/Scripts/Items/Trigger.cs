using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Execute an action if tthe payer comes near or uses the use key.
/// NOT USED IN THE FINAL VERSION OF THE GAME CODE.
/// </summary>
public class Trigger : Item {

	public delegate void ExecActionDelegate();

	public ExecActionDelegate ExecAction;

	// Use this for initialization
	void Start () {
		DestroyAfterTrigger = true;
		TriggerOnlyOnUse = false;
	}

	protected override void Action (Player player)
	{
		ExecAction ();
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position,0.5f);
	}

	void OnDrawGizmosSelected(){
		Gizmos.DrawWireSphere (transform.position, TriggerDistance);
	}
}

