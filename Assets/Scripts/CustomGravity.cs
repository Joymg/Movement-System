using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that adds support to Spherical gravity
/// </summary>
public static class CustomGravity 
{

    public static Vector3 GetGravity(Vector3 position)
    {
        return position.normalized * Physics.gravity.y;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 up = position.normalized;
        upAxis = Physics.gravity.y < 0f ? up : -up;
        return upAxis * Physics.gravity.y;
    }

    /// <summary>
    /// Determines Up axis for player an orbit camera
    /// </summary>
    /// <param name="position"></param>
    /// <returns>Up axis from current position</returns>
    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 up = position.normalized;
        return Physics.gravity.y < 0f ? up : -up;
    }


}
