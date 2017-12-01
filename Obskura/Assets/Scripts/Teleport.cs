using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour {

	private Vector3 centrePosition;
	private float nextTeleportTime=0;
	public float minTime = 3;
	public float maxTime =8;
	public float maxDistance = 3;

	void Update ()
	{	
		System.Random rnd = new System.Random();
		if (Time.time > nextTeleportTime) 
		{	
			Debug.Log("TELEPORT");
			//teleport code
			float dx=(float)rnd.NextDouble ()*maxDistance; 
			float dy=(float)rnd.NextDouble ()*maxDistance;
			//check for collisions
			//...
			transform.position = centrePosition + new Vector3 (dx, dy, 0);

			float interval = (float)(rnd.NextDouble () * (maxTime-minTime)) + minTime; 
			nextTeleportTime = Time.time + interval;

		}

			
	}	
	void Start () {
		centrePosition = transform.position;

	}
	

		

}
