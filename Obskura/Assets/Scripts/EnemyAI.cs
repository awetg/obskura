//Created by Elsa﻿

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour 
{	public float speed;
	public Transform target;
	public float chaseRange;


	void Update ()
	{
		float distanceToTarget =Vector3.Distance(transform.position, target.position);
		if(distanceToTarget <chaseRange)
		{
			Vector3 targetDir = target.position - transform.position;
			//find out where the player is
			float angle=Mathf.Atan2 (targetDir.y,targetDir.x)*Mathf.Rad2Deg-90f;
			Quaternion q=Quaternion.AngleAxis (angle,Vector3.forward);
			//turns towards the player
			transform.rotation = Quaternion.RotateTowards (transform.rotation, q, 180);
			transform.Translate (Vector3.up*Time.deltaTime*speed);
		}
	}
}
