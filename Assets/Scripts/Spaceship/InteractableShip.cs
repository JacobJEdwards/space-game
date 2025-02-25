#nullable enable

using Interfaces;
using Player;
using Unity.Assertions;
using UnityEngine;

namespace Spaceship
{
    [RequireComponent(typeof(Collider))]
    public class InteractableShip : MonoBehaviour, IInteractable
    {
        private PlayerController? _player;
        private ShipController _shipController = null!;

        private void Start()
        {
            _shipController = GetComponentInParent<ShipController>();
            Assert.IsNotNull(_shipController, "Ship controller is not set!");
        }

        public bool CanInteract(GameObject interactor)
        {
            _player = _player ? _player : interactor.GetComponent<PlayerController>();

            return _player;
        }

        public void OnInteract(GameObject interactor)
        {
            _player = _player ? _player : interactor.GetComponent<PlayerController>();

            if (!_player)
            {
                return;
            }

            if (_shipController.IsOccupied)
            {
                _shipController.PlayerExitShip();
            }
            else
            {
                _player.EnterShip(_shipController);
                _shipController.PlayerEnteredShip(_player!);
            }
        }

        public string GetInteractionPrompt(GameObject interactor)
        {
            return _shipController.IsOccupied ? "Press F to exit" : "Press F to enter";
        }
    }
}