using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDebug : MonoBehaviour {

	public OLight torch;

	// Use this for initialization
	void Start () {
		/* //DEBUG FOR TAGS
		var gos = Tags.GetAllGameObjects ();
		gos.ForEach (g => Debug.Log (g.name));
		Debug.Log ("    ");
		var walls = Tags.GameObjectsWithTag ("Wall");
		walls.ForEach (g => Debug.Log (g.name));
		Debug.Log ("    ");
		var enemies = Tags.GameObjectsWithTag ("Enemy");
		enemies.ForEach (g => Debug.Log (g.name));
		Debug.Log ("    ");
		var player = Tags.GameObjectsWithTag ("Player");
		player.ForEach (g => Debug.Log (g.name));
		Debug.Log ("    ");
		var enemyorplayer = Tags.GameObjectsWithOneOf (new string[] {"Enemy", "Player"});
		enemyorplayer.ForEach (g => Debug.Log (g.name));
		Debug.Log ("    ");
		var enemyplayer = Tags.GameObjectsWithTags (new List<string> {"Enemy", "Player"});
		enemyplayer.ForEach (g => Debug.Log (g.name));
		Debug.Log ("    ");
		*/
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10));
		//Debug.Log (Geometry.IsPointInAWall (pos));
		Debug.Log (torch.PointIsInLight(pos));
		//Debug.Log (Geometry.IsInLineOfSight (new Vector3(0,0,0), pos));
	
	}
}
