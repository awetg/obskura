using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Text))]

public class Typer : MonoBehaviour {

	public string message = "write here";
	public float startDelay = 1f;
	public float typeDelay = 0.02f;
	public AudioClip typerClip;
	private Text textHolder;

	// Use this for initialization
	void Start () {
//		StartCoroutine (TypeIn());

	}

	void Awake(){
		textHolder = GetComponent<Text> ();
	}
	
	// Update is called once per frame
//	void Update () {
//		
//	}

	public IEnumerator TypeIn(){

		yield return new WaitForSeconds (startDelay);

		for (int i = 0; i < message.Length + 1; i++) {
			Debug.Log ("what happend");
			textHolder.text = message.Substring (0, i);
			GetComponent<AudioSource> ().PlayOneShot (typerClip);
			yield return new WaitForSeconds (typeDelay);
		}
		yield return new WaitForSeconds (10.0f);
		StartCoroutine (TypeOut ());
	}

	public IEnumerator TypeOut(){
	
		for(int i = message.Length; i>=0; i--){
		
			textHolder.text = message.Substring(0,i);
			GetComponent<AudioSource> ().PlayOneShot (typerClip);
			yield return new WaitForSeconds (typeDelay);
		}
	}
}
