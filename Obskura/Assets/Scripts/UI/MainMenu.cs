using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {


	public GameObject logInPanel;
	public GameObject mainPanel;
	public GameObject gamePlusPanel;
	public GameObject statisticsPanel;
	public GameObject settingsPanel;
	public GameObject exitPanel;
	public GameObject tipText;
	public GameObject normalButton;
	public GameObject hardButton;
	public Dropdown dropDown;
	public Slider musicSlider;

	public GameObject hover;
	public GameObject click;
	public GameObject music;


	private float sliderValue;
	private string userName;

	private bool exitClicked=false;
	private Color selected, notSelected;


	// Use this for initialization
	void Start () {
		
		logInPanel.gameObject.SetActive (false);
		tipText.gameObject.SetActive (false);
		mainPanel.gameObject.SetActive(false);
		gamePlusPanel.gameObject.SetActive(false);
		statisticsPanel.gameObject.SetActive(false);
		settingsPanel.gameObject.SetActive(false);
		exitPanel.gameObject.SetActive(false);
//		dropDown.GetComponent<Dropdown> ().captionText.text = "Resolutions";
		selected = normalButton.GetComponent<Image> ().color;
		notSelected = hardButton.GetComponent<Image> ().color;
		musicSlider.onValueChanged.AddListener (delegate {
			sliderValueChange ();
		});

		StartMainMenu ();

	}
	
//	// Update is called once per frame
//	void Update () {
//		
//	}

	private void StartMainMenu(){
		if (GameData.GetPlayerName () == null) {
			logInPanel.gameObject.SetActive (true);
		} else
			mainPanel.gameObject.SetActive (true);
	
	}

	public void GetUserName(string newUser){	//getting input from input field in main menu

		userName = newUser;
	}


	public void EnterB(){	// On click of Enter button
		
		if (userName == null) {
			tipText.gameObject.SetActive (true);
		}
		else {
			//write useName to database
			logInPanel.gameObject.SetActive (false);
			mainPanel.gameObject.SetActive (true);
		}
	}


	public void NewGameB(){	// on click of New game button
		SceneManager.LoadScene ("copyOfDemo");
	}

	public void NewGamePlusB(){	// on click of new game plus button
		mainPanel.gameObject.SetActive(false);
		gamePlusPanel.gameObject.SetActive(true);
	}

	public void StatisticsB(){	// on click of statistics button
		mainPanel.gameObject.SetActive(false);
		statisticsPanel.gameObject.SetActive(true);
	}

	public void SettingsB(){
		mainPanel.gameObject.SetActive(false);
		settingsPanel.gameObject.SetActive(true);
	}

	public void ExitB(){
		if (exitClicked) {
			exitClicked = false;
			exitPanel.gameObject.SetActive (false);
		}
		else{
			exitClicked = true;
			exitPanel.gameObject.SetActive (true);
		}
	}

	public void ReturnNewGamePlus(){	// on click of return button in new game plus panel
		mainPanel.gameObject.SetActive(true);
		gamePlusPanel.gameObject.SetActive(false);
	}

	public void ReturnStatistics(){	// on click of return button in statistics panel
		mainPanel.gameObject.SetActive(true);
		statisticsPanel.gameObject.SetActive(false);
	}

	public void ReturnSettings(){
		mainPanel.gameObject.SetActive(true);
		settingsPanel.gameObject.SetActive(false);
	}

	public void NormalB(){	// on click of normal button in settings panel (setting game difficulty to normal)
		normalButton.GetComponent<Image> ().color = selected;
		hardButton.GetComponent<Image> ().color = notSelected;
	}

	public void HardB(){	//on click of hard button in settings panel ( setting game difficulty to hard)
		normalButton.GetComponent<Image> ().color = notSelected;
		hardButton.GetComponent<Image> ().color = selected;
	}

	public void ButtonHover(){	// on hover all buttons play sound
		hover.GetComponent<AudioSource> ().Play ();
	}

	public void ButtonClick(){	// on click all buttons play sound
		click.GetComponent<AudioSource> ().Play ();
	}

	public void NoB(){	// on click of NO button in exit panel
		exitClicked = false;
		exitPanel.gameObject.SetActive (false);
	}

	public void YesB(){	// on click of YES button in exit panel ( exits the application totally)
		//		Application.Quit();
		UnityEditor.EditorApplication.isPlaying = false;
	}
		

	public void sliderValueChange (){	// change in value from slider element to control volume of game music
	
		sliderValue = musicSlider.value;
		music.GetComponent<AudioSource> ().volume = sliderValue;
	}
}
