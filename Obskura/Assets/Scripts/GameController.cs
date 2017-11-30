using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

	public Text HpText;	//to show hp number int hp will be converted it will start 100 by default
	int hp=100;	//hp number
	public GameObject restart,torch,gun,endOfGame;	//GameObjects or items to be manipulated while play
	public bool gameOver=false; //if true restart GameObject will be active
	public bool labClose=false;	//if true acitvate endOfGame GameObject

	static GameController gameController;	//bcs we will use Awake() , only one object of this will be created
											//and manipulate the other variables in this class in different play

	void Awake()
	{
		gameController = this;

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
			gameController.endOfGame.SetActive(true);	//if player achieve main objective activate object used to exit Lab
		}
	}

	public static void ShowDamage(int hpamount){	//called from playerMovement() after damage registered
		gameController.hp -= hpamount;
		gameController.HpText.text = gameController.hp.ToString (); //conver hp(int) to string
	}

	public static void RestartText(){	//RestartWindow() method will be called from PlayerMovemnt() if registered damage is fatal
		gameController.restart.SetActive (true);
		gameController.gameOver = true;
	}

	public static void SelectWeapon(PlayerWeaponType weaponType){
		switch (weaponType) {
		case PlayerWeaponType.TORCH:
			gameController.torch.SetActive (true);//make an object look active on window
			gameController.gun.SetActive (false);
			break;
		case PlayerWeaponType.GUN:
			gameController.torch.SetActive (false);
			gameController.gun.SetActive (true);
			break;
		}
	}
}
