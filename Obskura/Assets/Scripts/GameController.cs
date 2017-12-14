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

	void Awake()
	{
		this.tag = "GameController";
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

		//If tthe player dies, it's game over
		if (player == null || player.HP <= 0) {
			restart.SetActive (true);
			gameOver = true;
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

	/// <summary>
	/// Concludes the game.
	/// </summary>
	public void ConcludeGame(){
		//Fill the screen with white light
		lightManager.Overlay = new Color (1.0F, 1.0F, 1.0F);
		StartCoroutine (GameEnded ());
		SceneManager.LoadScene ("MainMenu");
	}

	/// <summary>
	/// Called when the game ends, to wait for the player to see the white light.
	/// </summary>
	/// <returns>The ended.</returns>
	public IEnumerator GameEnded(){
		yield return new WaitForSeconds (3.0F);
	}

	/// <summary>
	/// Shows the HP of the player on sceen.
	/// </summary>
	/// <param name="hpamount">Hpamount.</param>
	void ShowDamage(float hpamount){	
		HpText.text = hpamount.ToString (); 
	}

	/// <summary>
	/// Shows the score on screen.
	/// </summary>
	/// <param name="score">Score.</param>
	void ShowScore(float score){	
		scoreValue.text = score.ToString ();
	}

	/// <summary>
	/// Shows the ammo on screen.
	/// </summary>
	/// <param name="ammo">Ammo.</param>
	void ShowAmmo(int ammo){	
		ammoValue.text = ammo.ToString (); 
	}

	/// <summary>
	/// Called when the exit button gets pressed.
	/// </summary>
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

	/// <summary>
	/// Shows the in game menu.
	/// </summary>
	public void ShowMenu(){	
		menuCanvas.gameObject.SetActive(true);
		Time.timeScale = 0.0f;

	}

	/// <summary>
	/// Closes the is game menu.
	/// </summary>
	public void CloseMenu(){	// if menu exited using escape key
		exitClicked = false;
		exitPanel.gameObject.SetActive(false);
		menuCanvas.gameObject.SetActive (false);
		Time.timeScale = 1.0f;
	}

	/// <summary>
	/// Called when the mouse hovers a button, plays a sound.
	/// </summary>
	public void ButtonHover(){	// On hover of any button play sound
		hover.GetComponent<AudioSource> ().Play ();
	}

	/// <summary>
	/// Called when the mouse clicks a button, plays a sound.
	/// </summary>
	public void ButtonClick(){	// On click of any button play sound
		click.GetComponent<AudioSource> ().Play ();
	}
	/// <summary>
	/// The exit "no" button has been clicked, hide the "yes" "no".
	/// </summary>
	public void NoB(){
		exitClicked = false;
		exitPanel.gameObject.SetActive (false);
	}

	/// <summary>
	/// The "exit" button has been pressed and confirmed.
	/// </summary>
	public void EndGame(){	//if yes button for exit is click go to main menu
		//SceneManager.LoadScene("MainMenu");
		//Going to main menu is buggy, causing the player to be unable to move in a new game
		//So I close the application on exit.
		Application.Quit();
	}
		
	/// <summary>
	/// Click on the button to close tthe DialogueBox
	/// </summary>
	public void typerButtonclick()
	{
		dialogueBox.gameObject.SetActive (false);
		dialogueBox.GetComponentInChildren<Text> ().text = "";

	}
}
