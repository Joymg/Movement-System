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

    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f;


    Vector3 focusPoint;

    /// <summary>
    /// Orientation of the camera
    /// </summary>
    Vector2 orbitAngles = new Vector2(45f, 0f);

    private void Awake()
    {
        focusPoint = focus.position;
    }

    //using late updat in case anything moves the target in update
    private void LateUpdate()
    {
        UpdateFocusPoint();
        ManualRotation();
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
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

    void ManualRotation()
    {
        //gets the input
        Vector2 input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e )
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
        }
    }
}
