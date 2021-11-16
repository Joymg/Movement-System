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

    /// <summary>
    /// Factor in charge of centering the focus 
    /// </summary>
    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;

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
            float t = 1f;
            //if the distance is big enougth tyo be noticeable and focus centering is positive
            if (distance > 0.01f && focusCentering > 0f)
            {
                //interpolate between current focus and target using (1-focusCentering)^deltatime as interpolator
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }

            if (distance > focusRadius)
            {
                //pull focus towards target
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        //otherwise set focus point to targer
        else
            focusPoint = targetPoint;
    }
}
