using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSample : MonoBehaviour {

	public Button button;
	public Text gameName;
	public Text difficulty;
	public Text level;

	private GamePlusData gameData;
	private GameLister l;

	// Use this for initialization
	void Start () {
		
	}
	
	public void SetValues(GamePlusData currentdata, GameLister currentSample ){

		gameData = currentdata;
		gameName.text = gameData.gameName;
		difficulty.text = gameData.difficulty;
		level.text = gameData.level;
	}
}
