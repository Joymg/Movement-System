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

    Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");

        playerInput = Vector2.ClampMagnitude(playerInput, 1f);


        //Setting Desired Velocity
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        //find maximum speed change this update
        float maxSpeedChange = maxAcceleration * Time.fixedDeltaTime;
        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        body.velocity = velocity;


    }

}

