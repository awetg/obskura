using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public Text HpText;	//to show hp number int hp will be converted it will start 100 by default

	public Text scoreValue;
	public Text ammoValue;

	public GameObject restart;	//GameObjects or items to be manipulated while play
	private bool gameOver=false; //if true restart GameObject will be active
	public Player player;	// to access public methods of player
	public OLightManager lightManager;


	private string gameName;	//after game is won get game name for game plus
	public GameObject tipTextExit;	// if game name is nor properly entered activate tip text
	public GameObject menuCanvas;
	public GameObject MainPanel;
	public GameObject endGamePanle;
	public GameObject exitPanel;
	public GameObject hover;
	public GameObject click;
	bool menuView = false;
	bool exitClicked = false;

	public Canvas dialogueBox;




//	public OLightManager lightManager; // See OLightManager.cs

	void Awake()
	{
		this.tag = "GameController";
		//FIXME: Add code to check if the game controller is unique and otherwise throw an exception
	}

	// Use this for initialization
	void Start () {
		menuCanvas.gameObject.SetActive(false);
		exitPanel.gameObject.SetActive (false);
		endGamePanle.gameObject.SetActive (false);
	
	}
	
	// Update is called once per frame
	void Update () {

			ShowDamage(player.HP);

			if (player.HP <= 0) {
				restart.SetActive (true);
				gameOver = true;
				//UnityEditor.EditorApplication.isPaused = false;
			}

		ShowAmmo (player.Ammo);
		ShowScore (player.Score);

		if (gameOver && Input.GetKeyDown(KeyCode.R))	//if user press r, game is reloaded
		{
			Geometry.Clear ();
			Tags.ClearCache ();
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

	public void ConcludeGame(){
		lightManager.Overlay = new Color (1.0F, 1.0F, 1.0F);
		StartCoroutine (GameEnded ());
		SceneManager.LoadScene ("MainMenu");
	}

	public IEnumerator GameEnded(){
		yield return new WaitForSeconds (3.0F);
	}


	void ShowDamage(float hpamount){	//called from playerMovement() after damage registered
		HpText.text = hpamount.ToString (); //conver hp(int) to string
	}

	void ShowScore(float score){	//called from playerMovement() after damage registered
		scoreValue.text = score.ToString (); //conver hp(int) to string
	}

	void ShowAmmo(int ammo){	//called from playerMovement() after damage registered
		ammoValue.text = ammo.ToString (); //conver hp(int) to string
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
		menuCanvas.gameObject.SetActive(true);
		Time.timeScale = 0.0f;

	}

	public void CloseMenu(){	// if menu exited using escape key
		exitClicked = false;
		exitPanel.gameObject.SetActive(false);
		menuCanvas.gameObject.SetActive (false);
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
			tipTextExit.gameObject.SetActive (true);
		else {
			//write gameName to database
			SceneManager.LoadScene ("MainMenu");
		}
	}

	public void typerButtonclick()
	{
		dialogueBox.gameObject.SetActive (false);
		dialogueBox.GetComponentInChildren<Text> ().text = "";

	}
}
