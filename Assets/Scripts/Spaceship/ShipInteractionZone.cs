using UnityEngine;
using UnityEngine.Events;

public class ShipInteractionZone : MonoBehaviour
{
    public UnityEvent<PlayerController> onPlayerEnterZone;
    public UnityEvent<PlayerController> onPlayerExitZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        print("In zone");
        var zeroGMovement = other.gameObject.GetComponentInParent<PlayerController>();
        onPlayerEnterZone?.Invoke(zeroGMovement);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var zeroGMovement = other.gameObject.GetComponentInParent<PlayerController>();
        onPlayerExitZone?.Invoke(zeroGMovement);
    }
}