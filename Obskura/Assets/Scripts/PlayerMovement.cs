using System.Collections;
using UnityEngine;

public enum PlayerWeaponType{TORCH,GUN,NULL}	//items, will also be accessed from Gamecontroller

public class PlayerMovement : MonoBehaviour {
	
    public float speed;
	private Quaternion rotate;
	private Rigidbody2D MyRigidbody;
	private Animator PlayerAnimator;

	public float Seconds;		//CAN BE DELETED EVERYWHERE IF NOT WANTED How many seconds delay before it calls/invoke Move() method
	private float hp = 100;	//player hp, updated from enemies and sent to gamecontroller to screen
	private Vector3 target;		//used to get mouse position as vector3 in Move() method, vector2 can't take 3 argument so can't be used
	public float StopDistance;		//How far player have to stop before reaching mouse position, small value is needed here
	private bool moving = false;	//used to stop player/or stop calling Move() method, when stopDistance is reached
	PlayerWeaponType currentWeapon=PlayerWeaponType.TORCH;	//player will not have weapon until he get the torch
	private float resetCameraAt = 0;
	private float resetCameraAfter = 3;
	private float cameraDamage = 0;
	private bool focused = false;

	public OGun PlayerLaser;

	public OLight TorchLight;

//	public GameObject bulletPrefab;	//Bullets to be fired from gunPivot or is laser just drag laser prefab in INSPERCTOR-unity
//	public Transform LightPivot,gunPivot;	//gunPivot to fire light, bullets or laser and hitPivot to check if damage can be done




	void Start() //Initializing variables
    {
		MyRigidbody = GetComponent<Rigidbody2D>();
		PlayerAnimator = GetComponent<Animator> ();
		PlayerAnimator.SetBool("move", false);
//		Screen.lockCursor = true;

		if (TorchLight != null)
			TorchLight.ConeAngle = 10f;
    }

    void FixedUpdate()
    {	///////// PLAYER LOOK AT MOUSE ///////////
		var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);	//used to get mouse position to rotate player towards mouse
		rotate = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);	//used to represent rotation
        transform.rotation = rotate;	// player rotate to "rotate" value
		if (TorchLight != null) {
			TorchLight.Position = new Vector2 (transform.position.x, transform.position.y + 0.3f);
			TorchLight.Direction = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
		}

		transform.eulerAngles = new Vector3(0,0,transform.eulerAngles.z);	//used to prevent rotation on z-axis when mouse out side of play screen

		//////// PLAYER WALK ///////
		if (Input.GetMouseButtonDown(1) && !moving) 	//if mouse is clicked Move player
        {
            moving = true;
            InvokeRepeating("Move", 0, Seconds);	//how long to wait before updating
            PlayerAnimator.SetBool("move", true);	//start walk animation,, bcs player is moving
        }

		if (Input.GetMouseButtonDown(0)) 	//if mouse is clicked Move player
		{
			//PlayerLaser.Fire (mousePosition);
		}
			
		if (!focused && Input.GetKeyDown (KeyCode.LeftControl)) {
			TorchLight.ConeAngle /= 3f;
			TorchLight.Intensity *= 1.3f;
			TorchLight.DimmingDistance *= 2f;
			focused = true;
		} else if (focused && Input.GetKeyUp(KeyCode.LeftControl)) {
			TorchLight.ConeAngle *= 3f;
			TorchLight.Intensity /= 1.3f;
			TorchLight.DimmingDistance /= 2f;
			focused = false;
		}
		
		if (resetCameraAt > Time.time) {
			float proportion = (resetCameraAt - Time.time) / resetCameraAfter;

			var camera = GameObject.FindGameObjectWithTag ("MainCamera");

			if (camera != null) {
				var lightManager = camera.GetComponent<OLightManager> ();
				lightManager.Overlay = new Color (proportion * cameraDamage, 0, 0);
			}
		} else if (resetCameraAt < Time.time) {
			cameraDamage = 0;
		}
			
		////// TRY ATTACK IF THER IS WEAPON ///////
//		if (PlayerWeaponType.GUN) {
//			
//			if (Input.GetKeyDown (KeyCode.Space))	//if spacebar clicked 
//			Fier();
//		}
    }


	/// <summary>
	/// Move this instance. The player.
	/// </summary>
    void Move()
    {

        if (moving)
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition); //target is position of mouse target.z = transform.position.z;
            Vector3 mousePosition = new Vector3(target.x, target.y, 0); //get new vector3 from target without z intercept,
																		// bcs player act weird without this

            Vector3 playerPosition = new Vector3(MyRigidbody.position.x, MyRigidbody.position.y, 0); //turn player position to vector 3
                                                                                                 //so that you can subtract target vector (vector3) with player vector(was vector2, now is vector3)

            Vector3 whereToMove = mousePosition - playerPosition; //create a vector between where mouse is and where player is


            if (whereToMove.magnitude < StopDistance)
            { //If condition to stop player from moving when too close to target position

                moving = false; //stops movement
                PlayerAnimator.SetBool("move", false); //stop walk animation
				CancelInvoke("Move");
            }

            whereToMove.Normalize(); //normalise turns wher  eToMove vector into unit vector.

            transform.position = playerPosition + whereToMove * speed; //move player to new positon using unit vector WhereToMove

			var camera = GameObject.FindGameObjectWithTag ("MainCamera");

			if (camera != null) {
				camera.transform.position = new Vector3(transform.position.x, transform.position.y,camera.transform.position.z);
			}
        }
    }


	public void SetWeapon(PlayerWeaponType New_Weapon){
		if (New_Weapon != currentWeapon) {	//do only if new_weapon is different from currentWeapon
			currentWeapon = New_Weapon;
//			PlayerAnimator.SetTrigger ("WeaponChange"); 	//change to new animation state
			switch (New_Weapon) {
			case PlayerWeaponType.TORCH:
				//destroy gun object and attach torch object
				break;
			case PlayerWeaponType.GUN:
				//destroy torch object and attach gun object
				break;
			}
		}
		//NOTE: The controller should check for a weapon change by using Player.GetCurrentWeapon()
		//GameController.SelectWeapon (New_Weapon);	//select the weapon on screen
	}

	public void DamagePlayer(float coming_hp){
		
		hp =hp - coming_hp;
		//NOTE: The controller should command to write the player's hp to screen
		//GameController.ShowDamage (hp);	//send hp to screen, only player hp get displayed

		var camera = GameObject.FindGameObjectWithTag ("MainCamera");

		if (camera != null) {
			var lightManager = camera.GetComponent<OLightManager> ();
			lightManager.Overlay =  new Color(coming_hp, 0, 0);
			cameraDamage = coming_hp;
			resetCameraAt = Time.time + resetCameraAfter;
		}

		if (hp <= 0) {
//			PlayerAnimator.SetBool ("Dead", true);	//start dead animation
//			PlayerAnimator.transform.parent = null;
			this.enabled = false;
			gameObject.GetComponent<BoxCollider2D> ().enabled = false;
			CancelInvoke ();
			Destroy (gameObject);
		}	
	}

	public bool IsAlive(){
		return (hp > 0);
	}

	public float GetHP(){
		return hp;
	}


//	public PlayerWeaponType GetCurrentWeapon(){	// NOTE: no need to get weapon type now,bcs gamecontroller set weaapon type
//		return currentWeapon;
//	}

}
