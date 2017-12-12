using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGamePlus : MonoBehaviour {


	public GameObject gamePlusPrefab;
	string gameName;
	string difficulty;
	string level;


	// Use this for initialization
	void Start () {
		
	}
	
	public void GamePlusB(){
		gameName = gamePlusPrefab.GetComponent<GameSample> ().gameName.text;
		difficulty = gamePlusPrefab.GetComponent<GameSample> ().difficulty.text;
		level = gamePlusPrefab.GetComponent<GameSample> ().level.text;
		Debug.Log (gameName + " " + difficulty + " " + level);
		SceneManager.LoadScene ("copyOfDemo");

	}
}
