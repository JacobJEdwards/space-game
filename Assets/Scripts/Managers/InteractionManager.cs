using Interfaces;
using UnityEngine;

namespace Managers
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private float interactionRange = 5f;
        [SerializeField] private UiManager uiManager;

        private IInteractable _currentTarget;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            interactionLayer = LayerMask.GetMask("Interaction");
        }

        private void Update()
        {
            HandleInteractionRaycast();
        }

        private void HandleInteractionRaycast()
        {
            var ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out var hit, interactionRange, interactionLayer))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if (!interactable.CanInteract(gameObject)) return;

                    _currentTarget = interactable;

                    uiManager.SetHint(interactable.GetInteractionPrompt(gameObject));
                }
                else
                {
                    _currentTarget = null;
                    uiManager.ClearHint();
                }
            }
            else
            {
                _currentTarget = null;
                uiManager.ClearHint();
            }
        }

        public void OnInteractInput()
        {
            if (_currentTarget != null && _currentTarget.CanInteract(gameObject))
                _currentTarget.OnInteract(gameObject);
        }
    }
}