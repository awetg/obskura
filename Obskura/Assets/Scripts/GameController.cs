using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public Text HpText;	//to show hp number int hp will be converted it will start 100 by default
	int hp=100;	//hp number //Probably better to move it in Player
	public GameObject restart,torch,gun,endOfGame;	//GameObjects or items to be manipulated while play
	public bool gameOver=false; //if true restart GameObject will be active
	public bool labClose=false;	//if true acitvate endOfGame GameObject

	Geometry geometry = new Geometry(); //Geometry data about the walls
	public OLightManager lightManager; // See OLightManager.cs

	void Awake()
	{
		this.tag = "Controller";
		//FIXME: Add code to check if the game controller is unique and otherwise throw an exception
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (gameOver && Input.GetKeyDown(KeyCode.R))	//if user press r, game is reloaded
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		if (labClose) {
			endOfGame.SetActive(true);	//if player achieve main objective activate object used to exit Lab
		}
	}

	void ShowDamage(int hpamount){	//called from playerMovement() after damage registered
		hp -= hpamount;
		HpText.text = hp.ToString (); //conver hp(int) to string
	}

	void RestartText(){	//RestartWindow() method will be called from PlayerMovemnt() if registered damage is fatal
		restart.SetActive (true);
		gameOver = true;
	}

	void SelectWeapon(PlayerWeaponType weaponType){
		switch (weaponType) {
		case PlayerWeaponType.TORCH:
			torch.SetActive (true);//make an object look active on window
			gun.SetActive (false);
			break;
		case PlayerWeaponType.GUN:
			torch.SetActive (false);
			gun.SetActive (true);
			break;
		}
	}
}
