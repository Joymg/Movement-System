
using UnityEngine;

public class GravitySphere : GravitySource
{
	[SerializeField]
	float gravity = 9.81f;

	[SerializeField, Min(0f)]
	float outerRadius = 10f, outerFalloffRadius = 15f;

    /// <summary>
    /// Falloff range
    /// </summary>
    float outerFalloffFactor;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        outerFalloffRadius = Mathf.Max(outerFalloffRadius,outerRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    
    public override Vector3 GetGravity(Vector3 position)
    {
        //finds the vector pointing from postition to sphere's center
        Vector3 vector = transform.position - position;
        //distance is vector's magnitude
        float distance = vector.magnitude;

        //if distance is greater the oter fallof radius gravity is not applied
        if (distance > outerFalloffRadius)
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
        return g * vector;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Vector3 p = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, outerRadius);
        if (outerFalloffRadius > outerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, outerFalloffRadius);
        }
    }
#endif
}
