
using UnityEngine;

public class GravitySphere : GravitySource
{
	[SerializeField]
	float gravity = 9.81f;

	[SerializeField, Min(0f)]
	float outerRadius = 10f, outerFalloffRadius = 15f;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        outerFalloffRadius = Mathf.Max(outerFalloffRadius,outerRadius);
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
