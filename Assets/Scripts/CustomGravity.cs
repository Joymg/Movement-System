using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that adds support to Spherical gravity
/// </summary>
public static class CustomGravity 
{
    static List<GravitySource> sources = new List<GravitySource>();

    /// <summary>
    /// Computes the force applied to a body in a specific postion
    /// </summary>
    /// <param name="position">Position of the body</param>
    /// <returns>Sum of gravity forces</returns>
    public static Vector3 GetGravity(Vector3 position)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }
        return g;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }
        upAxis = -g.normalized;
        return g;
    }

    /// <summary>
    /// Determines Up axis for player an orbit camera
    /// </summary>
    /// <param name="position"></param>
    /// <returns>Up axis from current position</returns>
    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            g += sources[i].GetGravity(position);
        }
        return -g.normalized;
    }


}
