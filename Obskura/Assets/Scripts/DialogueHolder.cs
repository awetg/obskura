using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueHolder : MonoBehaviour {

	public string dialogue;
	private DialogueManager dialogueManager;

	public string[] dialogueLines;
	void Start()
	{
		dialogueManager = FindObjectOfType<DialogueManager> ();
	}
	

	void Update () {
		
	}

	void onTriggerStay2D(Collider2D other)
	{
		if(other.gameObject.name == "Player")
		{
			if(Input.GetKeyUp(KeyCode.Space))
			{   
				//check if dialogue is not active, we activate it
				if(!dialogueManager.dialogueActive)
				{
					dialogueManager.dialogueLines=dialogueLines;
					dialogueManager.currentLine=0;
					dialogueManager.ShowDialogue ();
			}
		}

	}
	}
}
