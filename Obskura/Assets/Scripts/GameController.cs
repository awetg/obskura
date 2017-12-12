using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public Text HpText;	//to show hp number int hp will be converted it will start 100 by default
	float hp=100;	//hp number //Probably better to move it in Player
	float hpCompare;
	public GameObject restart,endOfGame;	//GameObjects or items to be manipulated while play
	private bool gameOver=false; //if true restart GameObject will be active
	private bool labClose=false;	//if true acitvate endOfGame GameObject
	public Player player;	// to access public methods of player



	private string gameName;	//after game is won get game name for game plus
	public GameObject tipText;	// if game name is nor properly entered activate tip text
	public GameObject canvas;
	public GameObject MainPanel;
	public GameObject endGamePanle;
	public GameObject exitPanel;
	public GameObject hover;
	public GameObject click;
	bool menuView = false;
	bool exitClicked = false;




//	public OLightManager lightManager; // See OLightManager.cs

	void Awake()
	{
		this.tag = "GameController";
		//FIXME: Add code to check if the game controller is unique and otherwise throw an exception
	}

	// Use this for initialization
	void Start () {
		canvas.gameObject.SetActive(false);
		exitPanel.gameObject.SetActive (false);
		endGamePanle.gameObject.SetActive (false);
	
	}
	
	// Update is called once per frame
	void Update () {

		hpCompare = player.GetHP ();

		if (hp != hpCompare) {
			Debug.Log ("hp changed");
			hp = hpCompare;
			ShowDamage(hp);
			if (hp <= 0) {
				restart.SetActive (true);
				gameOver = true;
				UnityEditor.EditorApplication.isPaused = false;
			}
		}
		
		if (gameOver && Input.GetKeyDown(KeyCode.R))	//if user press r, game is reloaded
		{
			Geometry.Clear ();
			Tags.ClearCache ();
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if (labClose) {
			endOfGame.SetActive(true);	//if player achieve main objective activate object used to exit Lab
		}



		if (Input.GetKeyDown (KeyCode.Escape)) {

			if (menuView) {	// using escape for both going to menu and exiting menu
				CloseMenu ();
				menuView = false;
			}
			else {
				ShowMenu ();
				menuView = true;
			}
		}

	}







	void ShowDamage(float hpamount){	//called from playerMovement() after damage registered
		HpText.text = hp.ToString (); //conver hp(int) to string
		Debug.Log ("shown");
	}






	/////////////////////////    MENU CONTROL FUNCTIONS


	public void ExitB(){	// ON Exit button click do this 
		if (exitClicked) {
			exitClicked = false;
			exitPanel.gameObject.SetActive (false);
		}

		else {
			exitClicked = true;
			exitPanel.gameObject.SetActive (true);
		}
	}


	public void ShowMenu(){	
		canvas.gameObject.SetActive(true);
		Time.timeScale = 0.0f;

	}

	public void CloseMenu(){	// if menu exited using escape key
		exitClicked = false;
		exitPanel.gameObject.SetActive(false);
		canvas.gameObject.SetActive (false);
		Time.timeScale = 1.0f;
	}

	public void ButtonHover(){	// On hover of any button play sound
		hover.GetComponent<AudioSource> ().Play ();
	}

	public void ButtonClick(){	// On click of any button play sound
		click.GetComponent<AudioSource> ().Play ();
	}
	public void NoB(){
		exitClicked = false;
		exitPanel.gameObject.SetActive (false);
	}
	public void EndGame(){	//if yes button for exit is click go to main menu
		SceneManager.LoadScene("MainMenu");
	}
		
	public void GetGameName(string newGameName){	//getting input from input field in end game menu

		gameName = newGameName;
	}

	public void EnterGameNameB(){
	
		if (gameName == null)
			tipText.gameObject.SetActive (true);
		else {
			//write gameName to database
			SceneManager.LoadScene ("MainMenu");
		}
	}
}
