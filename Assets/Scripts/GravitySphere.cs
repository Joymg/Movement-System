
using UnityEngine;

public class GravitySphere : GravitySource
{
	[SerializeField]
	float gravity = 9.81f;

    //adding support for inverted spheres
    [SerializeField, Min(0f)]
    float innerFalloffRadius = 1f, innerRadius = 5f;

	[SerializeField, Min(0f)]
	float outerRadius = 10f, outerFalloffRadius = 15f;


    /// <summary>
    /// Falloff range
    /// </summary>
    float innerFalloffFactor, outerFalloffFactor;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
        innerRadius = Mathf.Max(innerRadius,innerFalloffRadius);
        outerRadius = Mathf.Max(outerRadius,innerRadius);
        outerFalloffRadius = Mathf.Max(outerFalloffRadius,outerRadius);

        innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    
    public override Vector3 GetGravity(Vector3 position)
    {
        //finds the vector pointing from postition to sphere's center
        Vector3 vector = transform.position - position;
        //distance is vector's magnitude
        float distance = vector.magnitude;

        //if distance is greater than outer falloff radius or less than the inner falloff radius,
        //gravity is not applied
        if (distance > outerFalloffRadius || distance < innerFalloffRadius)
        {
            return Vector3.zero;
        }
        //otherwise is the vector scaled by gravity
        float g = gravity/distance;

        //reducing gravity linearly between radius and radius falloff
        if (distance > outerRadius)
        {
            //distance beyond the outer radius divided by falloff range
            g *= 1f - (distance - outerRadius) * outerFalloffFactor;
        }
        else if (distance < innerRadius)
        {
            g *= 1f - (innerRadius - distance) * innerFalloffFactor;
        }
        return g * vector;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Vector3 p = transform.position;
        if (innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, innerFalloffRadius);
        }
        Gizmos.color = Color.yellow;
        if (innerRadius > 0f && innerRadius < outerRadius)
        {
            Gizmos.DrawWireSphere(p, innerRadius);
        }
        Gizmos.DrawWireSphere(p, outerRadius);
        if (outerFalloffRadius > outerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, outerFalloffRadius);
        }
    }
#endif
}
