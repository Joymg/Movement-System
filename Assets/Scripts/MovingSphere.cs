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

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    Vector3 velocity, desiredVelocity;

    Rigidbody body;

    bool desiredJump;
    bool isGrounded;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
    
        desiredJump |= Input.GetButtonDown("Jump");

        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //Setting Desired Velocity
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
    }
    private void FixedUpdate()
    {


        velocity = body.velocity;
        //find maximum speed change this update
        float maxSpeedChange = maxAcceleration * Time.fixedDeltaTime;
        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;

        isGrounded = false;

    }

    void Jump()
    {
        if (isGrounded)
        {
            //Using gravity to calculate jump force
            velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            isGrounded |= normal.y >= 0.9f;
        }
    }
}

