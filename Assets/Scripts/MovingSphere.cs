using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    //Setting custom groundAngle
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;

    Vector3 velocity, desiredVelocity;
    //Saves thesurface's normal that is in contact with
    Vector3 contactNormal;
    int jumpPhase;
    float minGroundDotProduct;

    Rigidbody body;

    bool desiredJump;
    bool isGrounded;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
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

        UpdateState();
        //make air movement different from ground movement
        float acceleration = isGrounded ? maxAcceleration : maxAirAcceleration;
        //find maximum speed change this update
        float maxSpeedChange = acceleration * Time.fixedDeltaTime;
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

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void UpdateState()
    {

        velocity = body.velocity;
        if (isGrounded)
        {
            jumpPhase = 0;
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    void Jump()
    {
        if (isGrounded || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;
            //Pressing jump quicky stacks too much upwards velocity
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

            //Get upward component of saved normal
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                //if there is an upward force, substract it from jump speed
                //before adding it to velocity, so it wont exceed the limit
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
                //with max( ) we prevent a jump from slow it down so it wont be negative
            }

            //Using gravity to calculate jump force
            velocity += contactNormal * jumpSpeed;
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
            if (normal.y >= minGroundDotProduct)
            {
                isGrounded = true;
                //save surfaces's normal
                contactNormal = normal;
            }
        }
    }
}

