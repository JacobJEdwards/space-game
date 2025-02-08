using UnityEngine;
using UnityEngine.Events;

public class ShipInteractionZone : MonoBehaviour
{
    public UnityEvent onPlayerEnterZone;
    public UnityEvent onPlayerExitZone;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered ship zone");
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered ship zone");
        onPlayerEnterZone?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        onPlayerExitZone?.Invoke();
    }
}
