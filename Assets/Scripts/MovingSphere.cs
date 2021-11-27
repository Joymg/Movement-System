using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MovingSphere : MonoBehaviour
{

    /// <summary>
    /// Orbital camera component
    /// </summary>
    [SerializeField]
    Transform playerInputSpace = default;


    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f, maxClimbSpeed = 2f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f, maxClimbAcceleration = 20f;

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
    /// Max angle of stais that can be walked on
    /// </summary>
    [SerializeField, Range(0f, 90f)]
    float maxStairsAngle = 50f;

    /// <summary>
    /// Max wall's angle that can be climbed
    /// </summary>
    [SerializeField, Range(90, 180)]
    float maxClimbAngle = 140f;

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
    /// Layer used to detect collisions with stairs
    /// </summary>
    [SerializeField]
    LayerMask stairsMask = -1;

    /// <summary>
    /// Layer used to detect collinsions with climbable elements
    /// </summary>
    [SerializeField]
    LayerMask climbMask = -1;

    /// <summary>
    /// Saving different materiasl to distinguissh from normal movement or climbing
    /// </summary>
    [SerializeField]
    Material normalMaterial = default, climbingMaterial = default;

    Vector2 playerInput;

    Vector3 velocity, connectionVelocity;

    /// <summary>
    /// Saves the surface's normal that is in contact with
    /// </summary>
    Vector3 contactNormal;

    /// <summary>
    /// Saves the Steeps's normal that is in contact with
    /// </summary>
    Vector3 steepNormal;

    /// <summary>
    /// Saves the surface's normal is in contact with when climbing
    /// </summary>
    Vector3 climbNormal;

    /// <summary>
    /// Y axis won't be up and down strictly, now can be modified
    /// </summary>
    Vector3 upAxis;

    /// <summary>
    /// Vectors used for relative movement when gravity direction is not in the Y axis
    /// </summary>
    Vector3 rightAxis, forwardAxis;

    /// <summary>
    /// Current jumps executed
    /// </summary>
    int jumpPhase;

    float minGroundDotProduct;
    float minStairsDotProduct;
    float minClimbDotProduct;

    /// <summary>
    /// Keeps track of how many physic steps since the sphere was in ground
    /// </summary>
    int stepsSinceLastGrounded;

    /// <summary>
    /// Keeps track of how many physics steps since last jump
    /// </summary>
    int stepsSinceLastJump;

    Rigidbody body;

    //to move along with the body is connected with,
    //is needed to know if the sphere remained in contact with the same body the prevous step 
    Rigidbody connectedBody, previousConnectedBody;

    //platforms are kinematic animated, so their velocity is always zero,
    //so velocityCOnnection has to be calculated
    //also tracking connection position in local space for rotating connections
    Vector3 connectionWorldPosition, connectionLocalPosition;

    bool desiredJump;

    int groundContactCount;
    bool IsGrounded => groundContactCount > 0;

    int steepContactCount;
    bool OnSteep => steepContactCount > 0;

    int climbContactCount;

    bool IsClimbing => climbContactCount > 0 && stepsSinceLastJump > 2;

    MeshRenderer meshRenderer;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        //cause we are use custom gravity, RB gravity is desactivated
        body.useGravity = false;
        meshRenderer = GetComponent<MeshRenderer>();
        OnValidate();
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }


    private void Update()
    {

        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        //if playerInputSpace is set
        if (playerInputSpace)
        {

            rightAxis = ProjectOnContactPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectOnContactPlane(playerInputSpace.forward,upAxis);
        }
        //else keep world space
        else
        {
            rightAxis = ProjectOnContactPlane(Vector3.right, upAxis);
            forwardAxis = ProjectOnContactPlane(Vector3.forward, upAxis);
        }

        desiredJump |= Input.GetButtonDown("Jump");

        meshRenderer.material = IsClimbing ? climbingMaterial : normalMaterial;

    }
    private void FixedUpdate()
    {
        //adding suport for chaging gravity direction, it points in the opposite direction that gravity pulls
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);
        UpdateState();

        AdjustVelocity();
        

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

        if (IsClimbing)
        {
            velocity -= contactNormal * (maxClimbAcceleration*0.9f * Time.deltaTime);
        }
        else
        {
            velocity += gravity * Time.deltaTime;
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
        if (CheckCkimbing()|| IsGrounded || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            //checking if jumpPhase is less than maxAirJumps only works beacuse pahse is set back to zero directly after the jump
            //beacuse in the next step the sphere is still considered as grounded.
            //We should only reset the jump one step after the jump was initiated
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }

            if (groundContactCount >1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }

        //if there is a connection with another object update the connection
        if (connectedBody)
        {
            //only id the body is kinematic or at least as massive as the sphere
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    private void ClearState()
    {
        groundContactCount = steepContactCount= climbContactCount =0;
        contactNormal = steepNormal = climbNormal = Vector3.zero;
        connectionVelocity = Vector3.zero;

        //savinf connectBody before resetting it
        previousConnectedBody = connectedBody;

        //reset connected body
        connectedBody = null;
    }

    private void UpdateConnectionState()
    {
        //if the body is still in contact with the same body
        if (connectedBody==previousConnectedBody)
        {
            //calculate the movement direction by converting local postion back to world space,
            //using the current transform of the connected body
            //if thre isnt a rotation the result is the same, if thre is orbit is now take into account
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            //and the velocity
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        //Using player connection position in world space, instead of connections' position
        connectionWorldPosition = body.position;
        //connection local position is the same point but ins connection body's local space
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    void Jump(Vector3 gravity)
    {
        //Jumps was only allowed on ground and on air, but now we have the tools to allow it from walls too
        Vector3 jumpDirection;
        if (IsGrounded)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            // reset jump phase when wall jumping to be able to air jump after wall jump
            jumpPhase = 0;
        }
        //if air jumping is allowed allow jumping is if jump phase is equal to maxAirJumps
        else if (maxAirJumps > 0 &&jumpPhase <= maxAirJumps)
        {
            //However this makes it possible to do one more extra jump than intended
            //so the first jump phase is skipped
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        //Pressing jump quicky stacks too much upwards velocity
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        //adding upward force to jump direction and normialize, getting the average of both,
        //making it not affecting ground jumps but lifting up when jumping from a wall
        jumpDirection = (jumpDirection + upAxis).normalized;

        //Get upward component of saved normal
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            //if there is an upward force, substract it from jump speed
            //before adding it to velocity, so it wont exceed the limit
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            //with max( ) we prevent a jump from slow it down so it wont be negative
        }

        //Using gravity to calculate jump force
        velocity += jumpDirection * jumpSpeed;

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
        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        //adding the possibility to change the gravty direction makes that hits normal is not a viable option
        //dot product has to be calculated
        float upDot = Vector3.Dot(upAxis,hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
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

        //if we are snapping to ground we save the connected body
        connectedBody = hit.rigidbody;
        return true;
    }

    /// <summary>
    /// Switches to climb mode, sticking to the surface and moving relative to it
    /// </summary>
    /// <returns>If climbing is posible</returns>
    bool CheckCkimbing()
    {
        if (IsClimbing)
        {
            groundContactCount = climbContactCount;
            contactNormal = climbNormal;
            return true;
        }
        return false;
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
        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                //save surfaces's normal
                //and acummulate them if ther is more than one in contact
                contactNormal += normal;

                //if the the ground the sphere is contact with has a rigidbody is assigned directly, otherwise is set tu null
                connectedBody = collision.rigidbody;
            }

            //If the contact is not with ground check if it is with a wall,
            //0.01 just in case wall is not perfectly vertical
            else
            {
                if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    //save steep's normal
                    //and acummulate them if ther is more than one in contact
                    steepNormal += normal;

                    //if sphre ends up in a slope, but ground shold be preferred over slopes,
                    //so only assing slope body if there is not a ground contact
                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
                //if contact does not count as ground nor a wall, check for a climb
                //only including the climb if it isnt masked
                if (upDot >= minClimbDotProduct && (climbMask & (1 << layer))!= 0)
                {
                    climbContactCount += 1;
                    climbNormal += normal;
                    //looking for rigidbody to be able to climb to moving platforms
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }

    /// <summary>
    /// in case of been stuck in a crevasse use its steeps normals by pushing against those contact points 
    /// </summary>

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    void AdjustVelocity()
    {
        float acceleration, speed;
        Vector3 xAxis, zAxis;

        //if climbing
        if (IsClimbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            //make movement relative to wall and gravity, ignoring camera orientation
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        }
        else
        {
            acceleration = IsGrounded ? maxAcceleration : maxAirAcceleration;
            speed = maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }

        //Determine projected axes by projecting vectors on contact plane
        xAxis = ProjectOnContactPlane(rightAxis,contactNormal);
        zAxis = ProjectOnContactPlane(forwardAxis,contactNormal);

        //At this point, isalready known the velocity of what is under the body
        Vector3 relativeVelocity = velocity - connectionVelocity;

        //project currentvelocity on both vectors to get relatives speeds
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        //find maximum speed change this update
        float maxSpeedChange = acceleration * Time.deltaTime;

        //calculate new speeds relatives to ground
        float newX =
            Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

        //adjust velocity bya dding differences between new and old speeds
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    /// <summary>
    /// Projects a direction in a plane, it works with an arbirary normal and normalize at the end
    /// </summary>
    /// <param name="vector">Vector projected on a plane</param>
    /// <param name="normal">Plane's normal</param>
    /// <returns>Normalized projection of the vector in a plane</returns>
    private Vector3 ProjectOnContactPlane(Vector3 vector,Vector3 normal)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

}

