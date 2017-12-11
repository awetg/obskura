﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnerTrigger : Item {

	public string SpawnerTag = "Spawner";

	// Use this for initialization
	void Start () {
		DestroyAfterTrigger = true;
		TriggerOnlyOnUse = false;
	}
	
	protected override void Action (Player player)
	{
		var objs = GameObject.FindGameObjectsWithTag (SpawnerTag);

		objs.Where (obj => obj.tag == SpawnerTag && obj.GetComponent<Spawner> () != null)
			.Select (obj => obj.GetComponent<Spawner> ()).Cast<Spawner> ().ToList ()
			.ForEach (spawner => spawner.Trigger ());
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position,0.5f);
	}

	void OnDrawGizmosSelected(){
			Gizmos.DrawWireSphere (transform.position, TriggerDistance);
	}
}