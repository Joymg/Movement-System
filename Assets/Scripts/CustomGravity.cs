using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that adds support to Spherical gravity
/// </summary>
public static class CustomGravity 
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="upAxis"></param>
    /// <returns></returns>
    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        upAxis = -Physics.gravity.normalized;
        return Physics.gravity;
    }

    /// <summary>
    /// Determines Up axis for player an orbit camera
    /// </summary>
    /// <param name="position"></param>
    /// <returns>Up axis from current position</returns>
    public static Vector3 GetUpAxis(Vector3 position)
    {
        return Physics.gravity.normalized;
    }


}
