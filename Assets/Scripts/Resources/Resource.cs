using System;
using Interfaces;
using Player;
using UnityEngine;

namespace CollectableResources
{
public class Resource : MonoBehaviour, IInteractable
{
    public ResourceObject resourceObject;
    public int amountMultiplier = 1;


    public void OnInteract(GameObject interactor)
    {
        var inventory = interactor.GetComponent<Inventory>();

        if (!inventory) return;

        var resource = ScriptableObject.CreateInstance<ResourceObject>();
        resource.resourceName = resourceObject.resourceName;
        resource.resourceAmount = resourceObject.resourceAmount * amountMultiplier;
        resource.resourceSprite = resourceObject.resourceSprite;

        inventory.AddResource(resource);

        Destroy(gameObject);
    }

    public bool CanInteract(GameObject interactor)
    {
        return interactor.GetComponent<Inventory>();
    }

    public string GetInteractionPrompt(GameObject interactor)
    {
        return "Press F to pick up";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CanInteract(other.gameObject))
        {
            OnInteract(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
}
