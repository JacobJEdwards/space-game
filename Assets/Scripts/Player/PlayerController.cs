using System;
using JetBrains.Annotations;
using Managers;
using Movement;
using Spaceship;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Player
{
    public enum PlayerState
    {
        OnShip,
        InZeroG,
        InGravity
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health), typeof(Oxygen))]
    [RequireComponent(typeof(SpaceMovement), typeof(PlanetaryMovement))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera Settings")] [SerializeField]
        private CinemachineCamera playerCamera;

        [SerializeField] private CameraController cameraController;
        [SerializeField] private InteractionManager interactionManager;
        [SerializeField] private UiManager uiManager;
        [SerializeField] private Transform aim;

        [Header("Movement Settings")] [SerializeField]
        private MovementSettings movementSettings;

        [CanBeNull] public ShipController shipToEnter;

        public UnityEvent onEnterShip;
        public UnityEvent onExitShip;
        private PlanetaryMovement _planetaryMovement;
        private Health _playerHealth;
        private Oxygen _playerOxygen;

        private PlayerState _playerState = PlayerState.InZeroG;
        private Rigidbody _rb;

        private SpaceMovement _spaceMovement;

        private void Start()
        {
            movementSettings.groundLayer = LayerMask.GetMask("PlanetSurface");
            InitialiseComponents();
            ValidateComponents();
            UpdateMovementComponents();

            HideLockMouse(true);
        }

        private void FixedUpdate()
        {
            UpdateOxygenAndHealth();
            UpdateMovementState();
        }

        private void OnEnable()
        {
            if (playerCamera) CameraController.Register(playerCamera);
            CameraController.SetActiveCamera(playerCamera);
        }

        private void OnDisable()
        {
            if (playerCamera) CameraController.Unregister(playerCamera);
        }

        private static void HideLockMouse(bool on)
        {
            if (on)
            {
                if (Cursor.visible) Cursor.visible = false;
                if (Cursor.lockState != CursorLockMode.Locked) Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                if (Cursor.visible == false) Cursor.visible = true;
                if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
            }
        }


        private void InitialiseComponents()
        {
            var inputManager = FindFirstObjectByType<InputManager>();
            inputManager.SetOnInteractPressed(OnInteractionInput);
            inputManager.SetOnInventoryPress(OnToggleInventory);

            _playerHealth = GetComponent<Health>();
            _playerOxygen = GetComponent<Oxygen>();
            _rb = GetComponent<Rigidbody>();

            _spaceMovement = GetComponentInChildren<SpaceMovement>();
            _planetaryMovement = GetComponentInChildren<PlanetaryMovement>();

            if (_planetaryMovement) _planetaryMovement.enabled = false;
        }

        private void ValidateComponents()
        {
            Assert.IsNotNull(_playerHealth);
            Assert.IsNotNull(_playerOxygen);
            Assert.IsNotNull(_rb);
            Assert.IsNotNull(_spaceMovement);
        }

        private void OnInteractionInput()
        {
            interactionManager.OnInteractInput();
        }

        private void OnToggleInventory()
        {
            uiManager.ToggleInventory();
        }

        private void UpdateOxygenAndHealth()
        {
            if (_playerState == PlayerState.InZeroG) _playerOxygen.TakeDamage(1f * Time.fixedDeltaTime);

            if (_playerOxygen.CurrentOxygen <= 0) _playerHealth.TakeDamage(1);
        }

        private void UpdateMovementState()
        {
            switch (_playerState)
            {
                case PlayerState.OnShip:
                    return;
                case PlayerState.InZeroG:
                    UpdateZeroGMovement();
                    break;
                case PlayerState.InGravity:
                    UpdatePlanetaryMovement();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdatePlanetaryMovement()
        {
            var colliders = new Collider[1];

            if (Physics.OverlapSphereNonAlloc(transform.position, 50, colliders, movementSettings.groundLayer) !=
                0) return;

            _playerState = PlayerState.InZeroG;
            UpdateMovementComponents();
        }

        private void UpdateZeroGMovement()
        {
            for (var i = 0; i < 16; i++)
            {
                var direction = Quaternion.Euler(0, i * 22.5f, 0) * transform.forward;

                if (!Physics.Raycast(transform.position, direction, out _, 20f, movementSettings.groundLayer))
                    continue;

                _playerState = PlayerState.InGravity;
                UpdateMovementComponents();
                return;
            }
        }

        public void EnterShip(ShipController ship)
        {
            shipToEnter = ship;
            transform.parent = ship.transform;
            gameObject.SetActive(false);

            _playerState = PlayerState.OnShip;

            onEnterShip?.Invoke();
            _playerOxygen.Reset();
            uiManager.ClearHint();
            uiManager.TransitionToState(UIState.Ship);
            UpdateMovementComponents();
        }

        public void ExitShip()
        {
            transform.parent = null;
            gameObject.SetActive(true);

            CameraController.SetActiveCamera(playerCamera);

            if (shipToEnter && shipToEnter.CurrentState == ShipState.Landed)
                _playerState = PlayerState.InGravity;
            else
                _playerState = PlayerState.InZeroG;

            onExitShip?.Invoke();
            shipToEnter = null;
            uiManager.ClearHint();
            uiManager.TransitionToState(UIState.ZeroG); // FIX
            UpdateMovementComponents();
        }

        private void UpdateMovementComponents()
        {
            switch (_playerState)
            {
                case PlayerState.InZeroG:
                    EnableZeroGMovement();
                    break;
                case PlayerState.OnShip:
                    DisableAllMovement();
                    break;
                case PlayerState.InGravity:
                    EnablePlanetaryMovement();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnableZeroGMovement()
        {
            _spaceMovement.enabled = true;
            _planetaryMovement.enabled = false;
            _rb.constraints = RigidbodyConstraints.None;
        }

        private void EnablePlanetaryMovement()
        {
            _spaceMovement.enabled = false;
            _planetaryMovement.enabled = true;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void DisableAllMovement()
        {
            _spaceMovement.enabled = false;
            _planetaryMovement.enabled = false;
        }

        [Serializable]
        private class MovementSettings
        {
            public LayerMask groundLayer;
        }
    }
}