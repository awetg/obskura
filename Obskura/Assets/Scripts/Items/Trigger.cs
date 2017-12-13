using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

