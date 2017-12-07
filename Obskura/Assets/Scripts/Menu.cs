using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {

	public Animator cameraAnimator;
	public GameObject mainPanel;
	public GameObject playPanel;
	public GameObject exitPanel;
	public GameObject hover;
	public GameObject click;
	bool menuView = false;
	bool buttonClicked = false;
	// Use this for initialization
	void Start () {
		playPanel.gameObject.SetActive(false);
		exitPanel.gameObject.SetActive (false);

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Escape)) {
			
			if (menuView)
				CloseMenu ();
			else
				ShowMenu ();
		}
		
	}

	public void PlayB(){
		if (buttonClicked) {
			buttonClicked = false;
			mainPanel.transform.Rotate (Vector3.up, 60);
			playPanel.gameObject.SetActive (false);
			exitPanel.gameObject.SetActive (false);
		}

		else {
			buttonClicked = true;
			mainPanel.transform.Rotate (Vector3.up, -60);
			playPanel.gameObject.SetActive (true);
			exitPanel.gameObject.SetActive (false);
		}

	}

	public void settingsB(){
		if (buttonClicked) {
			buttonClicked = false;
			mainPanel.transform.Rotate (Vector3.up, 60);
			playPanel.gameObject.SetActive (false);
			exitPanel.gameObject.SetActive (false);
		}

		else {
			buttonClicked = true;
			mainPanel.transform.Rotate (Vector3.up, -60);
			playPanel.gameObject.SetActive (false);
			exitPanel.gameObject.SetActive (false);
		}
	}

	public void ExitB(){
		if (buttonClicked) {
			buttonClicked = false;
			mainPanel.transform.Rotate (Vector3.up, 60);
			playPanel.gameObject.SetActive (false);
			exitPanel.gameObject.SetActive (false);
		}

		else {
			buttonClicked = true;
			mainPanel.transform.Rotate (Vector3.up, -60);
			playPanel.gameObject.SetActive (false);
			exitPanel.gameObject.SetActive (true);
		}
	}


	public void ShowMenu(){
		cameraAnimator.SetBool ("Menu", true);
	}

	public void CloseMenu(){
		buttonClicked = false;
		mainPanel.transform.Rotate (Vector3.up, 60);
		exitPanel.gameObject.SetActive(false);
		playPanel.gameObject.SetActive(false);
		cameraAnimator.SetBool ("Menu", false);
	}

	public void ButtonHover(){
		hover.GetComponent<AudioSource> ().Play ();
	}

	public void ButtonClick(){
		click.GetComponent<AudioSource> ().Play ();
	}

	public void CloseApplication(){
//		Application.Quit();
		UnityEditor.EditorApplication.isPlaying = false;
	}
}
