using UnityEngine;

namespace Interfaces
{
    public interface IInteractable
    {
        void OnInteract(GameObject interactor);
        bool CanInteract(GameObject interactor);
        string GetInteractionPrompt(GameObject interactor);
    }
}