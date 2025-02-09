using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using Player;

public class ShipInteractionZone : MonoBehaviour
{
    public UnityEvent<PlayerController> onPlayerEnterZone;
    public UnityEvent<PlayerController> onPlayerExitZone;

    public void Interact()
    {
    }

    public void OnHover()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
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