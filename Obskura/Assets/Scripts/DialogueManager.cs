using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	public GameObject dialogueBox;
	public Text dialogueText;

	public bool dialogueActive;

	public string[] dialogueLines;
	public int currentLine;

	void Update ()

	{
		if (dialogueActive && Input.GetKeyDown(KeyCode.Space));
		{
			currentLine++;
		}

		//if the number of line exceeds dialogue lines, it means it is the end of the conversation.
		if(currentLine >=dialogueLines.Length)
		{
			dialogueBox.SetActive (false);
			dialogueActive = false;
			currentLine = 0; //set the currentline back to zero -> back to beginning

		}

		dialogueText.text = dialogueLines [currentLine];
	}

	public void ShowBox(string dialogue)
	{
		dialogueActive = true;
		dialogueBox.SetActive (true);
		dialogueText.text = dialogue;
	}

	public void ShowDialogue ()
	{
		dialogueActive = true;
		dialogueBox.SetActive (true);
	}
}
