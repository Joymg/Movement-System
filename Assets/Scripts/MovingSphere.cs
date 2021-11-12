using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovingSphere : MonoBehaviour
{ 

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
        Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y);
        Vector3 displacement = velocity * Time.fixedDeltaTime;
        transform.localPosition += displacement;

    }

}

