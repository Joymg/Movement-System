
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DetectionArea : MonoBehaviour
{
    [SerializeField]
    UnityEvent onFirstEnter = default, onLastExit = default;

    //List of colliders used to check if there is any colliders in the area
    List<Collider> colliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        //only invoke the event if there is no item in the area
        if (colliders.Count == 0)
        {
            onFirstEnter.Invoke();
        }
        //adds the collider to the list
        colliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        //removes collider from the list and if it is empty invoke exit event
        if (colliders.Remove(other) && colliders.Count==0)
        {
            onLastExit.Invoke();
        }
    }
}
