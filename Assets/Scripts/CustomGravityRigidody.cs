using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component make other objetcs apart from the player and the Camera to be affected by the custom gravity.
/// This makes it nevces goes to sleep
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidody : MonoBehaviour
{
    Rigidbody body;

    /// <summary>
    /// Delay where is assumed that the body is floating but might still fall
    /// </summary>
    float floatDelay;

    /// <summary>
    /// Making configurable if a body is allowed to float so it can go to sleep
    /// </summary>
    [SerializeField]
    bool floatToSleep = false;


    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (floatToSleep)
        {
            //if it is sleeping do not disturb it
            if (body.IsSleeping())
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
                floatDelay = 0f;
                return;
            }

            //assuming it never goes to sleep if its velocity its really small, gravity is not applied
            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)
                {
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    return;
                }
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
                floatDelay = 0f;
            }
        }

        body.AddForce(CustomGravity.GetGravity(body.position),ForceMode.Acceleration);
    }
}
