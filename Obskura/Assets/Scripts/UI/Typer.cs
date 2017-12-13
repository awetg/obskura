using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Typer : MonoBehaviour {

	public string message = "write here";
	private Text textHolder;


	void Start(){
		textHolder =GetComponent<Text> ();
		showText ();

	}

	public void showText(){
		if (textHolder != null)
			textHolder.text = message;
	}

}
