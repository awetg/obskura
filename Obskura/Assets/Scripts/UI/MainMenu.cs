using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {


	public GameObject mainPanel;
	public GameObject settingsPanel;
	public GameObject exitPanel;
	public GameObject normalButton;
	public GameObject hardButton;
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
		
		mainPanel.gameObject.SetActive(true);
		settingsPanel.gameObject.SetActive(false);
		exitPanel.gameObject.SetActive(false);
		selected = normalButton.GetComponent<Image> ().color;
		notSelected = hardButton.GetComponent<Image> ().color;
		musicSlider.onValueChanged.AddListener (delegate {
			sliderValueChange ();
		});

	}
	






	public void NewGameB(){	// on click of New game button
		SceneManager.LoadScene ("GameWorld");
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

	public void ReturnSettings(){
		mainPanel.gameObject.SetActive(true);
		settingsPanel.gameObject.SetActive(false);
	}

	public void NormalB(){	// on click of normal button in settings panel (setting game difficulty to normal)
		normalButton.GetComponent<Image> ().color = selected;
		hardButton.GetComponent<Image> ().color = notSelected;
		GameData.SetDifficulty (2);
	}

	public void HardB(){	//on click of hard button in settings panel ( setting game difficulty to hard)
		normalButton.GetComponent<Image> ().color = notSelected;
		hardButton.GetComponent<Image> ().color = selected;
		GameData.SetDifficulty (3);
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
		Application.Quit();
		//UnityEditor.EditorApplication.isPlaying = false;
	}
		

	public void sliderValueChange (){	// change in value from slider element to control volume of game music
	
		sliderValue = musicSlider.value;
		music.GetComponent<AudioSource> ().volume = sliderValue;
	}
}
