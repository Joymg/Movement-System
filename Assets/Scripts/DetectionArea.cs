
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DetectionArea : MonoBehaviour
{
    [SerializeField]
    UnityEvent onFirstEnter = default, onLastExit = default;

    //List of colliders used to check if there is any colliders in the area
    List<Collider> colliders = new List<Collider>();

    private void Awake()
    {
        //disabling component until something enters the area
        enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //only invoke the event if there is no item in the area
        if (colliders.Count == 0)
        {
            onFirstEnter.Invoke();
            //enabling component 
            enabled = true;
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
            //disabling component until something enters the area
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        //checks if the colliders in the area are still valid (they might have been disabled, deactivades or destroyed)
        for (int i = 0; i < colliders.Count; i++)
        {
            Collider collider = colliders[i];
            if (!collider || !collider.gameObject.activeInHierarchy)
            {
                //if its not valid remove it form the list
                colliders.RemoveAt(i--);
                if (colliders.Count == 0)
                {
                    onLastExit.Invoke();
                    //disabling component until something enters the area
                    enabled = false;
                }
            }
        }
    }

    void OnDisable()
    {
        if (colliders.Count > 0)
        {
            //preventing hot reloads from invoking exit event
#if UNITY_EDITOR
            if (enabled && gameObject.activeInHierarchy)
            {
                return;
            }
#endif
            //clearing the collider list and calling exit event
            colliders.Clear();
            onLastExit.Invoke();
        }
    }

}
