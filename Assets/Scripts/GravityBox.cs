using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Similar to the sphere but gravity pulls straigth down to the neares dace
public class GravityBox : GravitySource
{
	[SerializeField]
	float gravity = 9.81f;

    /// <summary>
    /// Configurable gravity, somewaht like a radius, 
    /// but each component keeps the distance from the center to de faces
    /// </summary>
	[SerializeField]
	Vector3 boundaryDistance = Vector3.one;

    //inner area where gravity acts at full strength, and the area where it slowly decreases untul reaching 0
    [SerializeField, Min(0f)]
    float innerDistance = 0f, innerFalloffDistance = 0f;

    float innerFalloffFactor;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);
        //maximum of inner distances is equal to the smallest boundary distance
        float maxInner = Mathf.Min(Mathf.Min(boundaryDistance.x, boundaryDistance.y), boundaryDistance.z);
        innerDistance = Mathf.Min(innerDistance,maxInner);
        //inner fallof must be at least as big as the inner distance
        innerFalloffDistance = Mathf.Max(Mathf.Min(innerFalloffDistance, maxInner), innerDistance);

        innerFalloffFactor = 1f / (innerFalloffDistance - innerDistance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="coordinate">Relevant position coordinate relative to box's center</param>
    /// <param name="distance">Distance to the neares face along the relevant axis</param>
    /// <returns>Gravity component along the same axis</returns>
    float GetGravityComponent(float coordinate, float distance)
    {

        if (distance > innerFalloffDistance)
        {
            return 0f; //then player is in the null gravity zone
        }
        //otherwise we check if player is in the reduced gravity area
        float g = gravity;
        if (distance > innerDistance)
        {
            g *= 1f - (distance - innerDistance) * innerFalloffFactor;
        }
        //gravity is fliped ig the coordinate is less than zero, couse player will be in the other side of the center
        return coordinate > 0f ? -g : g;
    }

    /// <summary>
    /// Makes the position relative to the box's position
    /// </summary>
    /// <param name="position">Body position</param>
    /// <returns>Gravity force applied to a body in a specific position</returns>
    public override Vector3 GetGravity(Vector3 position)
    {

        //to support cube with arbitary rotation, rotate the relative position to align with the cube
        position = transform.InverseTransformDirection(position - transform.position);

        //calculate the absolute distances from the center
        Vector3 vector = Vector3.zero;
        Vector3 distances;
        distances.x = boundaryDistance.x - Mathf.Abs(position.x);
        distances.y = boundaryDistance.y - Mathf.Abs(position.y);
        distances.z = boundaryDistance.z - Mathf.Abs(position.z);

        //get the smallest distance and assign the result to the appropiate component of the vector
        if (distances.x < distances.y)
        {
            if (distances.x < distances.z)
            {
                vector.x = GetGravityComponent(position.x, distances.x);
            }
            else
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }
        }
        else if (distances.y < distances.z)
        {
            vector.y = GetGravityComponent(position.y, distances.y);
        }
        else
        {
            vector.z = GetGravityComponent(position.z, distances.z);
        }
        //resutl is gravity that pulls straigth down to relative neares face
        return transform.TransformDirection(vector);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 size;
        if (innerFalloffDistance > innerDistance)
        {
            Gizmos.color = Color.cyan;
            size.x = 2f * (boundaryDistance.x - innerFalloffDistance);
            size.y = 2f * (boundaryDistance.y - innerFalloffDistance);
            size.z = 2f * (boundaryDistance.z - innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
        if (innerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            size.x = 2f * (boundaryDistance.x - innerDistance);
            size.y = 2f * (boundaryDistance.y - innerDistance);
            size.z = 2f * (boundaryDistance.z - innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2 * boundaryDistance);
    }

#endif
}
