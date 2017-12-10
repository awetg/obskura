using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class GamePlusData {
	public string gameName;
	public string difficulty;
	public string level;
}

public class GameLister : MonoBehaviour {

	public List<GamePlusData> gameList;
	public GameObject gamePlusPrefab;

	// Use this for initialization
	void Start () {

		AddGame ();
	}
	
//	// Update is called once per frame
//	void Update () {
//		
//	}

	private void AddGame(){
		for (int i = 0; i < gameList.Count; i++) {
			GamePlusData gameData = gameList [i];
			GameObject newGame = (GameObject)GameObject.Instantiate (gamePlusPrefab);
			newGame.transform.SetParent (transform,false);
			GameSample newSample = newGame.GetComponent<GameSample> ();
			newSample.SetValues (gameData, this);
		}
	}
}
