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

    /// <summary>
    /// Every angle below this threshold is considered as grounds
    /// </summary>
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;

    /// <summary>
    /// Angle 
    /// </summary>
    [SerializeField, Range(0f, 90f)]
    float maxStairsAngle = 50f;

    /// <summary>
    /// Max speed at which the sphere will snap to ground (shold be a bit higher than maxSpeed)
    /// </summary>
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    /// <summary>
    /// Max distance to the ground below for snap checking
    /// </summary>
    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    /// <summary>
    /// Layer used to ignore collisions with some object at snapping
    /// </summary>
    [SerializeField]
    LayerMask probeMask = -1;

    /// <summary>
    /// Layer used to detct collisions with stairs
    /// </summary>
    [SerializeField]
    LayerMask stairsMask = -1;

    Vector3 velocity, desiredVelocity;
    /// <summary>
    /// Saves the surface's normal that is in contact with
    /// </summary>
    Vector3 contactNormal;

    /// <summary>
    /// Current jumps executed
    /// </summary>
    int jumpPhase;

    float minGroundDotProduct;
    float minStairsDotProduct;

    /// <summary>
    /// Keeps track of how many physic steps since the sphere was in ground
    /// </summary>
    int stepsSinceLastGrounded;

    /// <summary>
    /// Keeps track of how many physics steps since last jump
    /// </summary>
    int stepsSinceLastJump;

    Rigidbody body;

    bool desiredJump;


    int groundContactCount;
    bool IsGrounded => groundContactCount > 0;

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

        //making the sphere white when its not in the ground
        GetComponent<Renderer>().material.SetColor(
            "_Color", IsGrounded ? Color.black : Color.white
        );
    }
    private void FixedUpdate()
    {

        UpdateState();

        AdjustVelocity();
        

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;

        ClearState();

    }

    private void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        //Snapping broke jump so anpping needs to be aborted right after jumping
        stepsSinceLastJump += 1;
        velocity = body.velocity;

        //if not on the ground call SnapToground
        if (IsGrounded || SnapToGround())
        {
            stepsSinceLastGrounded = 0;
            jumpPhase = 0;
            if (groundContactCount >1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    private void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

   

    void Jump()
    {
        if (IsGrounded || jumpPhase < maxAirJumps)
        {
            stepsSinceLastJump = 0;
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

    //keeps the sphere stuck to the ground,only invoked if in the air

    bool SnapToGround()
    {
        //We need to stick to the ground ritght when we lose connection with ground
        //so if the sphere its been in the air for more than 1 step: abort.

        //If a jump is executed snapping is aborted, afeter jumping the sphere
        //is considered grounded for a few seconds, so a small delay is added just in case
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        //snapping will only be produced if there's ground below the sphere.
        //hit allows to check if the thing below the sphere counts as ground
        if (!Physics.Raycast(body.position,Vector3.down, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        //use collision's normal to check if its ground
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        //if snap hasn't been aborted then we just lost contact with the ground so we snap to it
        groundContactCount = 1;
        //saving contacted ground's normal
        contactNormal = hit.normal;

        //then the sphere will be considered grounded, although it's still in the air.
        //Next step is adjust the speed to the ground
        
        float dot = Vector3.Dot(velocity, hit.normal);

        //if the velocity was already pointing down realign wil slow down snapping to the ground
        //so velocity its only adjusted if dot product is positive
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    /// <summary>
    /// Determines the appropiete minimun given a layer
    /// </summary>
    /// <param name="layer">A</param>
    /// <returns></returns>
    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
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
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDot)
            {
                groundContactCount += 1;
                //save surfaces's normal
                //and acummulatie them if ther is more than one in contact
                contactNormal += normal;
            }
        }
    }

    void AdjustVelocity()
    {
        //Determine projected axes by projecting vectors on contact plane
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        //project currentvelocity on both vectors to get relatives speeds
        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        //make air movement different from ground movement
        float acceleration = IsGrounded ? maxAcceleration : maxAirAcceleration;
        //find maximum speed change this update
        float maxSpeedChange = acceleration * Time.deltaTime;

        //calculate new speeds relatives to ground
        float newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        //adjust velocity bya dding differences between new and old speeds
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

}

