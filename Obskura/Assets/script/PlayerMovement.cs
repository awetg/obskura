using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float speed;
    private new Rigidbody2D rigidbody;
    public float Seconds;
    public float StopDistance;
    private Vector3 target;
    private bool moving = false;
    private Animator myAnimator;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myAnimator.SetBool("move", false);
    }

    void FixedUpdate()
    {
        var mouthPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Quaternion rotate = Quaternion.LookRotation(transform.position - mouthPosition, Vector3.forward);
        transform.rotation = rotate;
        transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);


        target = transform.position;

        if (Input.GetMouseButtonDown(0))  //if mouse is clicked
        {
            moving = true;
            InvokeRepeating("Move", 0, Seconds); //how long to wait before updating
            myAnimator.SetBool("move", true);
        }

    }

    void Move()
    {

        if (moving)
        {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition); //target is position of mouse
                                                                          //			target.z = transform.position.z;
            Vector3 mousePosition = new Vector3(target.x, target.y, 0);

            Vector3 playerPosition = new Vector3(rigidbody.position.x, rigidbody.position.y, 0); //turn player position to vector 3
                                                                                                 //so that you can subtract target vector (vector3) with player vector(was vector2, now is vector3)

            Vector3 whereToMove = mousePosition - playerPosition; //create a vector between wheree mouse is and where player is


            if (whereToMove.magnitude < StopDistance)
            { //If condition to stop player from moving when too close to target position

                moving = false; //stops movement
                myAnimator.SetBool("move", false);
            }

            whereToMove.Normalize(); //normalise turns whereToMove vector into unit vector. 

            rigidbody.transform.position = playerPosition + whereToMove * speed; //add vector to player position

        }
    }

}
