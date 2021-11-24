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

    //inner area where gravity acts at full strength, and the area where it slowly decreases until reaching 0
    [SerializeField, Min(0f)]
    float innerDistance = 0f, innerFalloffDistance = 0f;

    //Distance outside of the box where gravity works at full strength  and the area where it slowly decreases until reaching 0
    [SerializeField, Min(0f)]
    float outerDistance = 0f, outerFalloffDistance = 0f;

    float innerFalloffFactor, outerFalloffFactor;

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
        //inner falloff distance must be at least as big as the inner distance
        innerFalloffDistance = Mathf.Max(Mathf.Min(innerFalloffDistance, maxInner), innerDistance);
        //outer falloff distance must be at least as big as the outer distance
        outerFalloffDistance = Mathf.Max(outerFalloffDistance, outerDistance);

        innerFalloffFactor = 1f / (innerFalloffDistance - innerDistance);
        outerFalloffFactor = 1f / (outerFalloffDistance - outerDistance);
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

        //Determinate if the given position is inside or outside the box, this is done per dimension
        //counting along how many end up outside

        //checking if position is beyond right face
        int outside = 0;
        if (position.x > boundaryDistance.x)
        {
            //if it is apply force toward the cube's face and count as outside
            vector.x = boundaryDistance.x - position.x;
            outside = 1;
        }
        //if not, it is inside the cube and the force is applied from the inside toward the cube's face
        else if (position.x < -boundaryDistance.x)
        {   
            vector.x = -boundaryDistance.x - position.x;
            outside = 1;
        }
        //same for the rest of the faces
        if (position.y > boundaryDistance.y)
        {
            vector.y = boundaryDistance.y - position.y;
            outside += 1;
        }
        else if (position.y < -boundaryDistance.y)
        {
            vector.y = -boundaryDistance.y - position.y;
            outside += 1;
        }

        if (position.z > boundaryDistance.z)
        {
            vector.z = boundaryDistance.z - position.z;
            outside += 1;
        }
        else if (position.z < -boundaryDistance.z)
        {
            vector.z = -boundaryDistance.z - position.z;
            outside += 1;
        }

        //if outside is greater than 0, position is outside the box
        if (outside > 0)
        {
            float distance = outside == 1 ?
                Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;
            if (distance > outerFalloffDistance)
            {
                //if distance is greater than outer falloff distance gravity is not applied
                return Vector3.zero;
            }
            //otherwise gravity have to be determined
            float g = gravity / distance;
            if (distance > outerDistance)
            {
                g *= 1f - (distance - outerDistance) * outerFalloffFactor;
            }
            return transform.TransformDirection(g * vector);
        }

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

        if (outerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            DrawGizmosOuterCube(outerDistance);
        }
        if (outerFalloffDistance > outerDistance)
        {
            Gizmos.color = Color.cyan;
            DrawGizmosOuterCube(outerFalloffDistance);
        }
    }

    /// <summary>
    /// Given 4 points draws a rectangle 
    /// </summary>
    void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    /// <summary>
    /// Draws a cube given a distance
    /// </summary>
    /// <param name="distance"></param>
    void DrawGizmosOuterCube(float distance)
    {
        //Draw right face
        Vector3 a, b, c, d;
        a.y = b.y = boundaryDistance.y;
        d.y = c.y = -boundaryDistance.y;
        b.z = c.z = boundaryDistance.z;
        d.z = a.z = -boundaryDistance.z;
        a.x = b.x = c.x = d.x = boundaryDistance.x + distance;
        DrawGizmosRect(a, b, c, d);

        //draw left face
        a.x = b.x = c.x = d.x = -a.x;
        DrawGizmosRect(a, b, c, d);

        //draw up face
        a.x = d.x = boundaryDistance.x;
        b.x = c.x = -boundaryDistance.x;
        a.z = b.z = boundaryDistance.z;
        c.z = d.z = -boundaryDistance.z;
        a.y = b.y = c.y = d.y = boundaryDistance.y + distance;
        DrawGizmosRect(a, b, c, d);

        //draw down face
        a.y = b.y = c.y = d.y = -a.y;
        DrawGizmosRect(a, b, c, d);

        //draw forward face
        a.x = d.x = boundaryDistance.x;
        b.x = c.x = -boundaryDistance.x;
        a.y = b.y = boundaryDistance.y;
        c.y = d.y = -boundaryDistance.y;
        a.z = b.z = c.z = d.z = boundaryDistance.z + distance;
        DrawGizmosRect(a, b, c, d);
        //draw back face
        a.z = b.z = c.z = d.z = -a.z;
        DrawGizmosRect(a, b, c, d);

        //distance multiplied by sqrt(1/3)
        distance *= 0.5773502692f;
        Vector3 size = boundaryDistance;
        size.x = 2f * (size.x + distance);
        size.y = 2f * (size.y + distance);
        size.z = 2f * (size.z + distance);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }

#endif
}
