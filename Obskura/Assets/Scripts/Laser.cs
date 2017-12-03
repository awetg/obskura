using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour {

	LineRenderer line;
//	Light light;
	Ray ray;
	RaycastHit hit;
	float damage = 20.0f;

	// Use this for initialization
	void Start () {
		line = gameObject.GetComponent<LineRenderer> ();
//		light = gameObject.GetComponent<Light> ();
		line.enabled = false;
		line.useWorldSpace = true;
	}
	
	// Update is called once per frame
	void Update () {
		


		if (Input.GetMouseButtonDown (1)) {	//if right mouse click clicked 
			
			line.enabled = true;
//			Fire ();

		ray = new Ray (transform.position, transform.forward);
		line.SetPosition (0, ray.origin);

		if (Physics.Raycast (ray, out hit, 100)) {
			line.SetPosition (1, hit.point);

			if (hit.rigidbody) {
				//					hit.rigidbody.AddForceAtPosition (transform.forward * 5, hit.point);
				hit.transform.GetComponent<Enemy>().DamageEnemy(damage);
			}
		}
		else
			line.SetPosition(1,ray.GetPoint(100));
			line.enabled = false;
		}
	}
		

//	void Fire(){
		

//			ray = new Ray (transform.position, transform.forward);
//			line.SetPosition (0, ray.origin);
//
//			if (Physics.Raycast (ray, out hit, 100)) {
//				line.SetPosition (1, hit.point);
//
//				if (hit.rigidbody) {
////					hit.rigidbody.AddForceAtPosition (transform.forward * 5, hit.point);
//					hit.transform.GetComponent<Enemy>().DamageEnemy(damage);
//				}
//			}
//			else
//				line.SetPosition(1,ray.GetPoint(100));

//	}
}
