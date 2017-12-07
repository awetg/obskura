using System.Collections.Generic;
using UnityEngine;

public enum RightHandItem{PLASMAGUN,NULL}	//items, will also be accessed from Gamecontroller
public enum LeftHandItem{TORCH, UVTORCH, NIGHTVISION, NULL}

public class Player : MonoBehaviour {



	public float speed;
	private Quaternion rotate;
	private Rigidbody2D MyRigidbody;
	private Animator PlayerAnimator;

	//public float Seconds;		//CAN BE DELETED EVERYWHERE IF NOT WANTED How many seconds delay before it calls/invoke Move() method
	private float hp = 100;	//player hp, updated from enemies and sent to gamecontroller to screen
	private Vector3 target;		//used to get mouse position as vector3 in Move() method, vector2 can't take 3 argument so can't be used
	public float TargetStopDistance = 0.5F;		//How far player have to stop before reaching mouse position, small value is needed here
	private bool moving = false;	//used to stop player/or stop calling Move() method, when stopDistance is reached
	private Vector3 destination;

	public LeftHandItem leftHand = LeftHandItem.TORCH;
	public RightHandItem rightHand = RightHandItem.PLASMAGUN;
	private Dictionary<LeftHandItem, Tool> torches = new Dictionary<LeftHandItem, Tool> ();
	private Dictionary<RightHandItem, Tool> guns = new Dictionary<RightHandItem, Tool> ();

	private float resetCameraAt = 0;
	private float resetCameraAfter = 3;
	private float cameraDamage = 0;
	private bool focused = false;

	public OGun PlayerLaser;

	//Set the implementations of the tools in the inspectors
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

		MyRigidbody = GetComponent<Rigidbody2D>();
		PlayerAnimator = GetComponent<Animator> ();
		PlayerAnimator.SetBool("move", false);
		//		Screen.lockCursor = true;

		torches.Add(LeftHandItem.TORCH, new Tool("Torch", light : TorchLight));
		torches.Add(LeftHandItem.UVTORCH, new Tool("UVTorch", light : UVLight));
		torches.Add(LeftHandItem.NIGHTVISION, new Tool("NightVision", light : NightVision));
		guns.Add(RightHandItem.PLASMAGUN, new Tool("PlasmaGun", gun : PlasmaGun));

		//Give the torch and gun to the player (for testing)
		SetLeftHand (LeftHandItem.TORCH);
		SetRightHand (RightHandItem.PLASMAGUN);
	}

	void Update()
	{	///////// PLAYER LOOK AT MOUSE ///////////
		var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);	//used to get mouse position to rotate player towards mouse
		rotate = Quaternion.LookRotation(transform.position - mousePosition, Vector3.forward);	//used to represent rotation
		transform.rotation = rotate;	// player rotate to "rotate" value

		//Rotate the current torch to face in tthe same direction as the player
		if (torches.ContainsKey(leftHand) && torches[leftHand].light != null) {
			torches[leftHand].light.Position = new Vector2 (transform.position.x, transform.position.y + 0.3f);
			torches[leftHand].light.Direction = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
		}

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
		if (Input.GetKeyDown(KeyCode.Space) && guns.ContainsKey(rightHand) && guns[rightHand].gun != null) 	
		{
			guns[rightHand].gun.Fire (transform.position, mousePosition);
		}

		//Ctrl to focus the light
		if (!focused && Input.GetKeyDown (KeyCode.LeftControl)) {
			CurrentTorchLight.ConeAngle /= 3f;
			CurrentTorchLight.Intensity *= 1.3f;
			CurrentTorchLight.DimmingDistance *= 2f;
			focused = true;
		} else if (focused && Input.GetKeyUp(KeyCode.LeftControl)) {
			CurrentTorchLight.ConeAngle *= 3f;
			CurrentTorchLight.Intensity /= 1.3f;
			CurrentTorchLight.DimmingDistance /= 2f;
			focused = false;
		}

		//Red overlay when the player gets hurt
		if (resetCameraAt > Time.time) {
			float proportion = (resetCameraAt - Time.time) / resetCameraAfter;

			if (lightManager != null) {
				lightManager.Overlay = new Color ((proportion * cameraDamage) % 1.0F, 0, 0);
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

	public void CollectTool(LeftHandItem torch = LeftHandItem.NULL, RightHandItem gun = RightHandItem.NULL){
		if (gun != RightHandItem.NULL && guns.ContainsKey(gun)) {
			guns [gun].SetInInventory ();
		}
		if (torch != LeftHandItem.NULL && torches.ContainsKey(torch)) {
			torches [torch].SetInInventory();
		}
	}


	public void SetLeftHand(LeftHandItem item){
		if (item != leftHand) {	//do only if new_weapon is different from currentWeapon
			leftHand = item;
		}
	}

	public void SetRightHand(RightHandItem item){
		if (item != rightHand)
			rightHand = item;
	}

	public void DamagePlayer(float coming_hp){

		hp = hp - coming_hp;
		//NOTE: The controller should command to write the player's hp to screen
		//GameController.ShowDamage (hp);	//send hp to screen, only player hp get displayed

		if (lightManager != null) {
			cameraDamage += coming_hp / 30.0f;
			lightManager.Overlay =  new Color(cameraDamage % 1.0F, 0, 0);
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

	public string GetLeftHandItemName(){	
		if (leftHand != LeftHandItem.NULL && torches.ContainsKey (leftHand))
			return torches [leftHand].name;
		return "";
	}

	public string GetRightHandItemName(){	
		if (rightHand != RightHandItem.NULL && torches.ContainsKey (leftHand))
			return guns [rightHand].name;
		return "";
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

		public bool IsInInventory(){
			return inInventory;
		}

		public void SetInInventory(bool inInventory = true){
			this.inInventory = inInventory;
		}
	}

}
