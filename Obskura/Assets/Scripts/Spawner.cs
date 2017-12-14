using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a copy of a template object chosen at random from a list.
/// NOT USED IN THE FINAL VERSION OF THE GAME.
/// </summary>
public class Spawner : MonoBehaviour {

	public bool TriggerByProximity = true;
	public bool TriggerAtStart = true;
	public bool RandomAngle = true;
	public float TriggerDistance = 10.0F;
	public List<float> ProbabilityList;
	public List<GameObject> Templates;


	void Start(){
		if (TriggerAtStart)
			Trigger ();
	}

	// Update is called once per frame
	void Update () {

		if (!TriggerByProximity)
			return;

		//Check if the player is near enough to pick up
		var players = Tags.CachedGameObjectsWithTagInRange("Player", transform.position, TriggerDistance);

		foreach (GameObject playerObj in players) {
			Player player = playerObj.GetComponent<Player> ();

			if (player != null){
				Trigger ();
			}
		}
	}

	public void Trigger() {
		System.Random rnd = new System.Random ();
		List<float> probs = new List<float> ();
		float totalp = 0.0f;

		//Build the probability list
		for (int i = 0; i < ProbabilityList.Count; i++) {
			probs.Add (ProbabilityList [i]);
			totalp += ProbabilityList [i];
		}

		//The total probability can't be more than 1
		if (totalp > 1f)
			totalp = 1f;

		//Calculate and fill with equal probability the remaining places
		float remaining = 1.0F - totalp;
		int missing = Templates.Count - probs.Count;

		if (missing > 0) {
			float share = remaining / missing;
			while (probs.Count < Templates.Count) {
				probs.Add (share);
			}
		}

		//Check which probability basket has been selected
		float selected, pcount = 0f;
		int index = -1;

		selected = (float) rnd.NextDouble ();

		do {
			
			index += 1;
			pcount += probs[index];

		} while (selected > pcount && index < Templates.Count - 1);

		//Instantiate the selecte object...
		var obj = GameObject.Instantiate (Templates [index]);

		//...with a random angle
		if (RandomAngle)
			obj.transform.eulerAngles = new Vector3 (0, 0, (float) (rnd.NextDouble () * 2 * Mathf.PI * Mathf.Rad2Deg));

		obj.transform.position = transform.position;

		obj.SetActive (true);

		//Destroy the spawner
		Destroy (gameObject);
	}


	void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position,0.5f);
	}

	void OnDrawGizmosSelected(){
		if (TriggerByProximity)
			Gizmos.DrawWireSphere (transform.position, TriggerDistance);
	}
}
