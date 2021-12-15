
using UnityEngine;
using UnityEngine.Events;

public class DetectionArea : MonoBehaviour
{
    [SerializeField]
    UnityEvent onEnter = default, onExit = default;

    private void OnTriggerEnter(Collider other)
    {
        onEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        onExit.Invoke();
    }
}
