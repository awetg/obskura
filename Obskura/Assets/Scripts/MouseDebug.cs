using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDebug : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10));
		//Debug.Log (Geometry.IsPointInAWall (pos));
		Debug.Log (Geometry.IsInLineOfSight (new Vector3(0,0,0), pos));
	}
}
