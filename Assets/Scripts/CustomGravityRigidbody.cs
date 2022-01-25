using UnityEngine;

/// <summary>
/// This component make other objetcs apart from the player and the Camera to be affected by the custom gravity.
/// This makes it nevces goes to sleep
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    /// <summary>
    /// Making configurable if a body is allowed to float so it can go to sleep
    /// </summary>
    [SerializeField] bool floatToSleep = false;

    [SerializeField] float submergenceOffset = 0.5f;

    [SerializeField, Min(0.1f)] float submergenceRange = 1f;

    [SerializeField, Min(0f)] float buoyancy = 1f;

    [SerializeField] Vector3 buoyancyOffset = Vector3.zero;

    [SerializeField, Range(0f, 10f)] float waterDrag = 1f;

    [SerializeField] LayerMask waterMask = 0;

    Rigidbody body;

    /// <summary>
    /// Delay where is assumed that the body is floating but might still fall
    /// </summary>
    float floatDelay;

    float submergence;

    Vector3 gravity;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    void FixedUpdate()
    {
        if (floatToSleep)
        {
            //if it is sleeping do not disturb it
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            //assuming it never goes to sleep if its velocity its really small, gravity is not applied
            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)
                {
                    return;
                }
            }
            else
            {
                floatDelay = 0f;
            }
        }

        gravity = CustomGravity.GetGravity(body.position);
        if (submergence > 0f)
        {
            float drag = Mathf.Max(0f, 1f - waterDrag * submergence * Time.deltaTime);
            body.velocity *= drag;
            body.angularVelocity *= drag;
            body.AddForceAtPosition(gravity * -(buoyancy * submergence), transform.TransformPoint(buoyancyOffset),
                ForceMode.Acceleration);
            submergence = 0f;
        }

        body.AddForce(gravity, ForceMode.Acceleration);
    }

    void OnTriggerEnter(Collider other)
    {
        if ((waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!body.IsSleeping() && (waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    void EvaluateSubmergence()
    {
        Vector3 upAxis = -gravity.normalized;
        if (Physics.Raycast(body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit,
                submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1f - hit.distance / submergenceRange;
        }
        else
        {
            submergence = 1f;
        }
    }
}