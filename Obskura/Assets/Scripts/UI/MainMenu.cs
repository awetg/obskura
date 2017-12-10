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
	private List<string> resolutions = new List<string> { "Default", "1920 X 1080", "1280 X 720", "800 X 600" };
	// Default = 1366 X 768

	private bool exitClicked=false;
	private Color selected, notSelected;


	// Use this for initialization
	void Start () {
		logInPanel.gameObject.SetActive(true);
		tipText.gameObject.SetActive (false);
		mainPanel.gameObject.SetActive(false);
		gamePlusPanel.gameObject.SetActive(false);
		statisticsPanel.gameObject.SetActive(false);
		settingsPanel.gameObject.SetActive(false);
		exitPanel.gameObject.SetActive(false);
		selected = normalButton.GetComponent<Image> ().color;
		notSelected = hardButton.GetComponent<Image> ().color;
//		dropDown.GetComponent<Dropdown> ().captionText.text = "Resolutions";
		dropDown.GetComponent<Dropdown> ().AddOptions (resolutions);
		musicSlider.onValueChanged.AddListener (delegate {
			sliderValueChange ();
		});
	}
	
//	// Update is called once per frame
//	void Update () {
//		
//	}

	public void GetUserName(string newUser){

		userName = newUser;
	}


	public void EnterB(){
		
		if (userName == null) {
			tipText.gameObject.SetActive (true);
		}
		else {
			logInPanel.gameObject.SetActive (false);
			mainPanel.gameObject.SetActive (true);
		}
	}


	public void NewGameB(){
		SceneManager.LoadScene ("copyOfDemo");
	}

	public void NewGamePlusB(){
		mainPanel.gameObject.SetActive(false);
		gamePlusPanel.gameObject.SetActive(true);
	}

	public void StatisticsB(){
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

	public void ReturnNewGamePlus(){
		mainPanel.gameObject.SetActive(true);
		gamePlusPanel.gameObject.SetActive(false);
	}

	public void ReturnStatistics(){
		mainPanel.gameObject.SetActive(true);
		statisticsPanel.gameObject.SetActive(false);
	}

	public void ReturnSettings(){
		mainPanel.gameObject.SetActive(true);
		settingsPanel.gameObject.SetActive(false);
	}

	public void NormalB(){
		normalButton.GetComponent<Image> ().color = selected;
		hardButton.GetComponent<Image> ().color = notSelected;
		Debug.Log ("noraml mode");
	}

	public void HardB(){
		normalButton.GetComponent<Image> ().color = notSelected;
		hardButton.GetComponent<Image> ().color = selected;
		Debug.Log ("Hard mode");
	}

	public void ButtonHover(){
		hover.GetComponent<AudioSource> ().Play ();
	}

	public void ButtonClick(){
		click.GetComponent<AudioSource> ().Play ();
	}

	public void NoB(){
		exitClicked = false;
		exitPanel.gameObject.SetActive (false);
	}

	public void YesB(){
		//		Application.Quit();
		UnityEditor.EditorApplication.isPlaying = false;
	}

	public void dropDownOutput(int option){
		Debug.Log (resolutions [option]);
	}

	public void sliderValueChange (){
	
		sliderValue = musicSlider.value;
		music.GetComponent<AudioSource> ().volume = sliderValue;
	}
}
