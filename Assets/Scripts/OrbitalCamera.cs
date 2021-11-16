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
    Transform target = default;

    /// <summary>
    /// Distance the camera will follow the target
    /// </summary>
    [SerializeField, Range(1f, 20f)]
    float distance = 5f;

    //using late updat in case anything moves the target in update
    private void LateUpdate()
    {
        Vector3 targetPosition = target.position;
        Vector3 lookDirection = transform.forward;
        transform.localPosition = targetPosition - lookDirection * distance;
    }
}
