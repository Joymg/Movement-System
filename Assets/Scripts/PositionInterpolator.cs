using UnityEngine;

public class PositionInterpolator : MonoBehaviour
{
    [SerializeField]
    Rigidbody body = default;

    [SerializeField]
    Vector3 from = default, to = default;

    /// <summary>
    /// Position Interpolation will be relative to this transform
    /// </summary>
    [SerializeField]
    Transform relativeTo = default;

    public void Interpolate(float t)
    {
        Vector3 p;
        //if there is an object to move relatively to
        if (relativeTo)
        {
            //Interpolate between positions in world coordinates
            p = Vector3.LerpUnclamped(
                relativeTo.TransformPoint(from), relativeTo.TransformPoint(to), t
            );
        }
        else
        {
            p = Vector3.LerpUnclamped(from, to, t);
        }
        body.MovePosition(p);
    }
}
