using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RightHandTool{PLASMAGUN,NULL}	//items, will also be accessed from Gamecontroller
public enum LeftHandTool{TORCH, UVTORCH, NIGHTVISION, NULL}

public class Player : MonoBehaviour {


	public float Score = 0.0F;
	public int Ammo = 0;
	public int AmmoClip = 8;
	public float HP = 100;	//player hp, updated from enemies and sent to gamecontroller to screen
	public float MaxHP = 200;
	public float speed;
	private Quaternion rotate;
	private Rigidbody2D MyRigidbody;
	private Animator PlayerAnimator;

	//public float Seconds;		//CAN BE DELETED EVERYWHERE IF NOT WANTED How many seconds delay before it calls/invoke Move() method
	private Vector3 target;		//used to get mouse position as vector3 in Move() method, vector2 can't take 3 argument so can't be used
	public float TargetStopDistance = 0.5F;		//How far player have to stop before reaching mouse position, small value is needed here
	private bool moving = false;	//used to stop player/or stop calling Move() method, when stopDistance is reached
	private bool usePressed = false;
	private Vector3 destination;

	public LeftHandTool leftHand = LeftHandTool.NULL;
	public RightHandTool rightHand = RightHandTool.NULL;
	private Dictionary<LeftHandTool, Tool> torches = new Dictionary<LeftHandTool, Tool> ();
	private Dictionary<RightHandTool, Tool> guns = new Dictionary<RightHandTool, Tool> ();

	private float resetCameraAt = 0;
	private float resetCameraAfter = 3;
	private float cameraDamage = 0;
	private bool focused = false;
	//private bool inStrongLight = false;
	private const float outOfLightAfter = 0.5f;
	private float outOfLightAt = 0.0f;

	public OGun PlayerLaser;

	//Set the implementations of the tools in the inspectors
	public OLight SurroundLight;
	public OLight TorchLight;
	public OLight UVLight;
	public OLight NightVision;
	public OLight CurrentTorchLight;
	public OGun PlasmaGun;

	//Main camera instancce
	private OLightManager lightManager;

	//	public GameObject bulletPrefab;	//Bullets to be fired from gunPivot or is laser just drag laser prefab in INSPERCTOR-unity
	//	public Transform LightPivot,gunPivot;	//gunPivot to fire light, bullets or laser and hitPivot to check if damage can be done




	void Start() //Initializing variables
	{
		var camera = GameObject.FindGameObjectWithTag ("MainCamera");
		lightManager = camera.GetComponent<OLightManager> ();

		Tags.CacheAdd (gameObject);

		MyRigidbody = GetComponent<Rigidbody2D>();
		PlayerAnimator = GetComponent<Animator> ();
		PlayerAnimator.SetBool("move", false);
		//		Screen.lockCursor = true;

		torches.Add(LeftHandTool.TORCH, new Tool("Torch", light : TorchLight));
		torches.Add(LeftHandTool.UVTORCH, new Tool("UVTorch", light : UVLight));
		torches.Add(LeftHandTool.NIGHTVISION, new Tool("NightVision", light : NightVision));
		guns.Add(RightHandTool.PLASMAGUN, new Tool("PlasmaGun", gun : PlasmaGun));

		//Give the torch and gun to the player (for testing)
		//SetLeftHand (LeftHandTool.TORCH);
		//SetRightHand (RightHandTool.PLASMAGUN);
	}

	void Update()
	{			
		///////// PLAYER LOOK AT MOUSE ///////////
		var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);	//used to get mouse position to rotate player towards mouse
		rotate = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);	//used to represent rotation
		transform.rotation = rotate;	// player rotate to "rotate" value

		if(SurroundLight!=null)
			SurroundLight.Position = new Vector2 (transform.position.x, transform.position.y);

		transform.eulerAngles = new Vector3(0,0,transform.eulerAngles.z);	//used to prevent rotation on z-axis when mouse out side of play screen

		//////// PLAYER WALK ///////
		if (Input.GetMouseButtonDown (0) && !moving) { 	//if mouse is clicked Move player
			PlayerAnimator.SetBool ("move", true);	//start walk animation,, bcs player is moving
			moving = true;
			//InvokeRepeating("Move", 0, Seconds);	//how long to wait before updating
		} else if (Input.GetMouseButtonUp (0)) {
			moving = false;
			PlayerAnimator.SetBool ("move", false);
		}

		//Shoot at right click if the player has a gun and the gun exists and is selected
		if (Input.GetKeyDown(KeyCode.Space) && guns.ContainsKey(rightHand)
			&& guns[rightHand].gun != null && guns[rightHand].inInventory) 	
		{
			if (Ammo != 0) {
				guns [rightHand].gun.Fire (transform.position, mousePosition);
				Ammo -= 1;
			}
		}

		if (Input.GetKeyDown (KeyCode.E))
			usePressed = true;

		if (Input.GetKeyUp (KeyCode.E))
			usePressed = false;

		//Rotate the current torch to face in tthe same direction as the player
		if (torches.ContainsKey (leftHand) && torches [leftHand].light != null) {
			torches [leftHand].light.Position = new Vector2 (transform.position.x, transform.position.y + 0.3f);
			torches [leftHand].light.Direction = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;

			if (Input.GetKeyDown (KeyCode.T))
				torches [leftHand].light.IsOn = !torches [leftHand].light.IsOn;

			//Ctrl to focus the light
			if (!focused && Input.GetKeyDown (KeyCode.LeftControl)) {
				torches [leftHand].light.ConeAngle /= 3f;
				torches [leftHand].light.Intensity *= 1.3f;
				torches [leftHand].light.DimmingDistance *= 2f;
				torches [leftHand].light.DamagePerSecond *= 1.5f;
				focused = true;
			} else if (focused && Input.GetKeyUp (KeyCode.LeftControl)) {
				torches [leftHand].light.ConeAngle *= 3f;
				torches [leftHand].light.Intensity /= 1.3f;
				torches [leftHand].light.DimmingDistance /= 2f;
				torches [leftHand].light.DamagePerSecond /= 1.5f;
				focused = false;
			}
		}
		//Red overlay when the player gets hurt
		if (resetCameraAt > Time.time) {
			float proportion = (resetCameraAt - Time.time) / resetCameraAfter;

			if (lightManager != null) {
				lightManager.Overlay = new Color (Mathf.Min((proportion * cameraDamage), 1.0F), 0, 0);
			}
		} else if (resetCameraAt < Time.time) {
			cameraDamage = 0;

			if (lightManager != null) {
				lightManager.Overlay = new Color (0, 0, 0);
			}
		}

		//Move if moving
		if (moving)
			Move();
	}


	/// <summary>
	/// Move this instance. The player.
	/// </summary>
	void Move()
	{
		target = Camera.main.ScreenToWorldPoint(Input.mousePosition); //target is position of mouse target.z = transform.position.z;
		Vector3 mousePosition = new Vector3(target.x, target.y, 0); //get new vector3 from target without z intercept,
		// bcs player act weird without this

		Vector3 playerPosition = new Vector3(transform.position.x, transform.position.y, 0); //turn player position to vector 3
		//so that you can subtract target vector (vector3) with player vector(was vector2, now is vector3)

		Vector3 whereToMove = mousePosition - playerPosition; //create a vector between where mouse is and where player is

		if (whereToMove.magnitude < TargetStopDistance) { //If condition to stop player from moving when too close to target position
			moving = false; //stops movement
			PlayerAnimator.SetBool ("move", false); //stop walk animation
			//CancelInvoke ("Move");
		}

		whereToMove.Normalize(); //normalise turns whereToMove vector into unit vector.
		transform.position = playerPosition + whereToMove * speed * Time.deltaTime; //move player to new positon using unit vector WhereToMove

		var camera = GameObject.FindGameObjectWithTag ("MainCamera");

		if (camera != null) {
			camera.transform.position = new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
		}
	}

	public void CollectTool(LeftHandTool torch = LeftHandTool.NULL, RightHandTool gun = RightHandTool.NULL){
		if (gun != RightHandTool.NULL && guns.ContainsKey(gun)) {
			Ammo += AmmoClip;
			guns [gun] = guns[gun].SetInInventory();
			SetRightHand (gun);
		}
		if (torch != LeftHandTool.NULL && torches.ContainsKey(torch)) {
			torches[torch] = torches [torch].SetInInventory();
			SetLeftHand (torch);
		}
	}


	public void SetLeftHand(LeftHandTool item){
		if (item != leftHand) {	//do only if new_weapon is different from currentWeapon
			ChangeTorch(item);
		}
	}

	public void SetRightHand(RightHandTool item){
		if (item != rightHand)
			rightHand = item;
	}

	public void DamagePlayer(float damage){

		return;

		HP = HP - damage;
		//NOTE: The controller should command to write the player's hp to screen
		//GameController.ShowDamage (hp);	//send hp to screen, only player hp get displayed

		if (lightManager != null) {
			cameraDamage += damage / 20.0f;
			lightManager.Overlay =  new Color(Mathf.Min(cameraDamage, 1.0F), 0, 0);
			resetCameraAt = Time.time + resetCameraAfter;
		}

		if (HP <= 0) {
			//			PlayerAnimator.SetBool ("Dead", true);	//start dead animation
			//			PlayerAnimator.transform.parent = null;
			this.enabled = false;
			gameObject.GetComponent<BoxCollider2D> ().enabled = false;
			CancelInvoke ();
			Tags.CacheRemove (gameObject);
			lightManager.Overlay = new Color (0, 0, 0);
			Destroy (gameObject);

		}	
	}

	public void SetInStrongLight(){
		outOfLightAt = Time.time + outOfLightAfter;
	}

	public bool IsAlive(){
		return (HP > 0);
	}

	public bool IsPressingUseKey(){
		return usePressed;
	}

	public bool IsInStrongLight(){
		if (Time.time > outOfLightAt)
			return false;
		return true;
	}

	public float GetHP(){
		return HP;
	}

	public string GetLeftHandItemName(){	
		if (leftHand != LeftHandTool.NULL && torches.ContainsKey (leftHand))
			return torches [leftHand].name;
		return "";
	}

	public string GetRightHandItemName(){	
		if (rightHand != RightHandTool.NULL && torches.ContainsKey (leftHand))
			return guns [rightHand].name;
		return "";
	}

	public void ChangeTorch(LeftHandTool item){
		if (leftHand != LeftHandTool.NULL && torches.ContainsKey (leftHand) && torches [leftHand].inInventory) {
			torches [leftHand].light.IsOn = false;
		}

		leftHand = item;

		if (item != LeftHandTool.NULL && torches.ContainsKey (item) && torches [item].inInventory) {
			torches [item].light.IsOn = true;
		}
		else
			leftHand = LeftHandTool.NULL;
	}

	public void OnTriggerEnter(Collider other){
		Debug.Log ("Entered");
		var gc = GameObject.FindGameObjectWithTag ("GameController");
		var controller = gc.GetComponent<GameController> ();

		controller.ConcludeGame ();
	}

	private struct Tool
	{
		public OLight light;
		public OGun gun;
		public string name;
		public bool inInventory;

		public Tool(string name, OLight light = null, OGun gun = null, bool inInventory = false){
			this.name = name;
			this.light = light;
			this.gun = gun;
			this.inInventory = inInventory;
		}

		public Tool SetInInventory(bool value = true){
			return new Tool (name, light, gun, value);
		}

	}

}
