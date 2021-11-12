using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    { 
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");

        //Normalizing to behave the same with keys and joystick
        ///playerInput.Normalize();

        //Normalizing limits the position 
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //Displace instead of teleporting
        ///Vector3 displacement = new Vector3(playerInput.x, 0f, playerInput.y);

        //Add Velocity for precise control
        /**Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        Vector3 displacement = velocity * Time.fixedDeltaTime;*/

        //Adding Accelaration
        /**Vector3 accelaration = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        velocity += accelaration * Time.fixedDeltaTime;
        Vector3 displacement = velocity * Time.fixedDeltaTime;*/

        //Setting Desired Velocity
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
            //find maximum speed change this update
        float maxSpeedChange = maxAcceleration * Time.fixedDeltaTime;
        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        Vector3 displacement = velocity * Time.fixedDeltaTime; 
         transform.localPosition += displacement;

    }

}

