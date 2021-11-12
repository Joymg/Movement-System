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

    [SerializeField]
    Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    [SerializeField, Range(0f, 1f)]
	float bounciness = 0.5f;

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

        Vector3 newPosition = transform.localPosition + displacement;
        //Constraining position 
        if (!allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
        {
            //looks laggy becuase inputs are being missed
            ///newPosition = transform.localPosition;

            //clamping solves that issue, but now balls sticks to the edge
            //because its keeping velocity in the edge dierection
            /**newPosition.x =
                Mathf.Clamp(newPosition.x, allowedArea.xMin, allowedArea.xMax);
            newPosition.z =
                Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);*/


            if (newPosition.x < allowedArea.xMin)
            {
                newPosition.x = allowedArea.xMin;
                //velocity.x = 0f;
                //Bouncing
                velocity.x = -velocity.x;
            }
            else if (newPosition.x > allowedArea.xMax)
            {
                newPosition.x = allowedArea.xMax;
                //velocity.x = 0f;
                //Bouncing
                velocity.x = -velocity.x;
            }
            if (newPosition.z < allowedArea.yMin)
            {
                newPosition.z = allowedArea.yMin;
                //velocity.z = 0f;
                //Bouncing
                velocity.z = -velocity.z;
            }
            else if (newPosition.z > allowedArea.yMax)
            {
                newPosition.z = allowedArea.yMax;
                //velocity.z = 0f;
                //Bouncing
                velocity.z = -velocity.z;
            }
        }
        transform.localPosition = newPosition;

    }

}

