using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitalCamera : MonoBehaviour
{

    Camera regularCamera;
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

    /// <summary>
    /// Contraints for vertical rotation
    /// </summary>
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    /// <summary>
    /// Delay until the camera places itself behind the target
    /// </summary>
    [SerializeField, Min(0f)]
    float alignDelay = 5f;

    /// <summary>
    /// Angle between current and desired angle when aligment will go at full rotationSpeed
    /// </summary>
    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f;

    Vector3 focusPoint, previousFocusPoint;

    /// <summary>
    /// Orientation of the camera
    /// </summary>
    Vector2 orbitAngles = new Vector2(45f, 0f);

    /// <summary>
    /// Required Vector for boxCast containing he half extends of a box, which means half its width, height, and depth.
    /// <br>Height/2 = Tangent of half the camera FoV angle in radians, scaled by its near clip plane distance.</br>
    /// <br>Width/2 = Height/2 * cameras aspect ratio</br>
    /// <br>Depth /2 = 0.</br>
    /// </summary>
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    float lastManualRotationTime;

    private void Awake()
    {
        regularCamera = GetComponent<Camera>();
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }


    /// <summary>
    /// Just in case wrong values are introduced in the editor. Max value should never be less than min value
    /// </summary>
    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    //using late updat in case anything moves the target in update
    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;

        //constraining the angles only when moving the camera
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        //casting box cast until camera near clip plane
        if (Physics.BoxCast(focusPoint, CameraHalfExtends,-lookDirection, out RaycastHit hit, lookRotation, distance-regularCamera.nearClipPlane))
        {
            //if something gets hitted final distance is hit distance + nearplane distance
            lookPosition = focusPoint - lookDirection * (hit.distance + regularCamera.nearClipPlane);
            //INFO: this can make the camera's postition end up inside the geometry , but its near plane will always remain outside

        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {

        previousFocusPoint = focusPoint;
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

    bool ManualRotation()
    {
        //gets the input
        Vector2 input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;

            //saving when the last manual rotation happened
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    bool AutomaticRotation()
    {
        //start aligning after the  delay if a manual rotation ocurred
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }

        //calculate movement between current and previous focusPoints
        Vector2 movement = new Vector2(
            focusPoint.x - previousFocusPoint.x,
            focusPoint.z - previousFocusPoint.z
        );
        float movementDeltaSqr = movement.sqrMagnitude;
        //if sqr magnitude is smaller than a smal threshold then there is no movement
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }


        //normalized movement vector (as sqrMagnitude is calculated is more efficient to normalize it this way)
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

        //if this value is in the smooth range, rotation speed will ge scaled accordingly
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));

        //using rotationSpeed to make a smooth rotation like in manual rotation
        //also dampening rotation of tiny angle
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }

        //also reducing rotationSpeed moving towards the camera
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }

        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y,headingAngle,rotationChange);

        return true;
    }

    /// <summary>
    /// Clamps the vertical orbit angle, horizontal orbit has no limit, but will be kept in 0-360 range
    /// </summary>
    void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }

    }

    /// <summary>
    /// Calculate the horizontal angle of a direction
    /// </summary>
    /// <param name="direction">Current direction</param>
    /// <returns></returns>
    static float GetAngle(Vector2 direction)
    {
        // Y direction component is cosine of angle looked for, so Acos and rad2deg
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        //rotation could be clockwise or counter clockwise, x negative : ccw, x positive: cw
        return direction.x < 0f ? 360f - angle : angle;
    }


}
