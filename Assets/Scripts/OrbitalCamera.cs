using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitalCamera : MonoBehaviour
{
    /// <summary>
    /// Target followed by the camera
    /// </summary>
    [SerializeField]
    Transform focus = default;

    /// <summary>
    /// Distance the camera will follow the target
    /// </summary>
    [SerializeField, Range(1f, 20f)]
    float distance = 5f;

    /// <summary>
    /// Radius away from center at which the camera will start moving
    /// </summary>
    [SerializeField, Min(0f)]
    float focusRadius = 1f;

    Vector3 focusPoint;

    private void Awake()
    {
        focusPoint = focus.position;
    }

    //using late updat in case anything moves the target in update
    private void LateUpdate()
    {
        UpdateFocusPoint();
        Vector3 lookDirection = transform.forward;
        transform.localPosition = focusPoint - lookDirection * distance;
    }

    void UpdateFocusPoint()
    {
        //if focus Radius is positive
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            //if the dist between target and current focus is greater than the radius
            float distance = Vector3.Distance(targetPoint, focusPoint);
            if (distance > focusRadius)
            {
                //pull focus towards target
                focusPoint = Vector3.Lerp(targetPoint,focusPoint,focusRadius/distance);
            }
        }
        //otherwise set focus point to targer
        else
            focusPoint = targetPoint;
    }
}
