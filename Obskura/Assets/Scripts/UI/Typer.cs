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

	void showText(){
		textHolder.text = message;
	}

}
