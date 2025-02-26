#nullable enable

using Interfaces;
using UnityEngine;

namespace Managers
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private float interactionRange = 5f;
        private UiManager _uiManager = null!;

        private IInteractable? _currentTarget;

        private Camera? _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            _uiManager = UiManager.Instance;
            interactionLayer = LayerMask.GetMask("Interaction");
        }

        private void Update()
        {
            HandleInteractionRaycast();
        }

        private void HandleInteractionRaycast()
        {
            if (!_mainCamera || !_uiManager) return;

            var ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out var hit, interactionRange, interactionLayer))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if (!interactable.CanInteract(gameObject)) return;

                    _currentTarget = interactable;

                    _uiManager.SetHint(interactable.GetInteractionPrompt(gameObject));
                }
                else
                {
                    _currentTarget = null;
                    _uiManager.ClearHint();
                }
            }
            else
            {
                _currentTarget = null;
                _uiManager.ClearHint();
            }
        }

        public void OnInteractInput()
        {
            if (_currentTarget != null && _currentTarget.CanInteract(gameObject))
                _currentTarget.OnInteract(gameObject);
        }
    }
}