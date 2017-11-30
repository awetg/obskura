using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    //	public float Distance;
    //	public float speed;
    //	private Vector3 destination;
    //	private Vector3 projection;	

    public GameObject Player; //attach the object needed to be tracked to this variable in editor 
    private Vector3 offset; //get how long player traveled
    public Transform player;


    // Use this for initialization
    void Start()
    {
        offset = transform.position - Player.transform.position;
        //		destination = transform.position;
        //		projection = Player.position;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        //		transform.position = Player.transform.position + offset;
        transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z);
        //		if ((Player.position - projection).magnitude > Distance )
        //		{
        //			projection = Vector3.MoveTowards(projection, Player.position, Time.deltaTime * speed);
        //			destination = projection;
        //			destination.y = transform.position.y;
        //			transform.position = destination;
        //		}
    }
}
