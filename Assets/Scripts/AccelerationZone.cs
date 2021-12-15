using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
    [SerializeField, Min(0f)]
    float acceleration = 10f, speed = 10f;


    //When something enters the collider 
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rigidbody = other.attachedRigidbody;
        if (rigidbody)
        {
            //it will get launched
            Accelerate(rigidbody);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rigidbody = other.attachedRigidbody;
        if (rigidbody)
        {
            Accelerate(rigidbody);
        }
    }

    //body will be launched with the determined speed
    private void Accelerate(Rigidbody rigidbody)
    {
        //Converting rb velocity intos local coordinates of the launchpad
        Vector3 velocity = transform.InverseTransformDirection(rigidbody.velocity);
        //unless its velocity was already greater
        if (velocity.y >= speed)
        {
            return;
        }

        if (acceleration > 0f)
        {
            velocity.y = Mathf.MoveTowards(velocity.y, speed, acceleration * Time.deltaTime);
        }
        else
        {
            velocity.y = speed;
        }

        //and converting it back to world coordinates when applying it
        rigidbody.velocity = transform.TransformDirection(velocity);

        if (rigidbody.TryGetComponent(out MovingSphere player))
        {
            player.PreventSnapToGround();
        }
    }
}
