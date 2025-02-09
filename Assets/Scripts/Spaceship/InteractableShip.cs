using Interfaces;
using UnityEngine;
using Player;

namespace Spaceship
{
    [RequireComponent(typeof(Collider)), RequireComponent(typeof(ShipController))]
    public class InteractableShip : MonoBehaviour, IInteractable
    {
        private ShipController _shipController;

        private void Start()
        {
            _shipController = GetComponent<ShipController>();
        }

        public bool CanInteract(GameObject interactor)
        {
            var player = interactor.GetComponent<PlayerController>();
            if (!player) return false;

            return !_shipController.IsOccupied;
        }

        public void OnInteract(GameObject interactor)
        {
            var player = interactor.GetComponent<PlayerController>();
            if (!player) return;

            if (_shipController.IsOccupied)
            {
                _shipController.OnInteract();
            }
            else
            {
                player.AssignShipToEnter(_shipController);
                player.OnInteract();
            }
        }

        public string GetInteractionPrompt(GameObject interactor)
        {
            return _shipController.IsOccupied ? "Press E to exit" : "Press E to enter";
        }

    }
}