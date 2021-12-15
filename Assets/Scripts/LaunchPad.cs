using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [SerializeField, Min(0f)]
    float speed = 10f;


    //When something enters the collider 
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rigidbody = other.attachedRigidbody;
        if (rigidbody)
        {
            //it will get launched
            Launch(rigidbody);
        }
    }

    //body will be launched with the determined speed
    private void Launch(Rigidbody rigidbody)
    {
        Vector3 velocity = rigidbody.velocity;
        //unless its velocity was already greater
        if (velocity.y >= speed)
        {
            return;
        }

        velocity.y = speed;
        rigidbody.velocity = velocity;
    }
}
