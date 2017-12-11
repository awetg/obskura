using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	public Animator cameraAnimator;
	public GameObject exitPanel;
	public GameObject hover;
	public GameObject click;
	public Canvas d;
	bool menuView = false;
	bool buttonClicked = false;
	public Typer typer;
	// Use this for initialization
	void Start () {
		exitPanel.gameObject.SetActive (false);
//		typer = GameObject.FindGameObjectWithTag ("Paper").GetComponent<Typer>();


	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			
			if (menuView) {
				CloseMenu ();
				menuView = false;
			}
			else {
				ShowMenu ();
				menuView = true;
			}
		}
		
	}
		
	public void ExitB(){
		if (buttonClicked) {
			buttonClicked = false;
			exitPanel.gameObject.SetActive (false);
		}

		else {
			buttonClicked = true;
			exitPanel.gameObject.SetActive (true);
		}
	}


	public void ShowMenu(){
		cameraAnimator.SetBool ("Menu", true);

	}

	public void CloseMenu(){
		buttonClicked = false;
		exitPanel.gameObject.SetActive(false);
		cameraAnimator.SetBool ("Menu", false);
	}

	public void ButtonHover(){
		hover.GetComponent<AudioSource> ().Play ();
	}

	public void ButtonClick(){
		click.GetComponent<AudioSource> ().Play ();
	}

	public void EndGame(){
//		Application.Quit();
//		UnityEditor.EditorApplication.isPlaying = false;
		SceneManager.LoadScene("MainMenu");
	}

	public void typerButtonclick()
	{
		d.gameObject.SetActive (false);

	}
}
