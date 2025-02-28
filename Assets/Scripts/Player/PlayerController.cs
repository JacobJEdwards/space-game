#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
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
        private CinemachineCamera playerCamera = null!;

        private CameraController _cameraController = null!;
        private InteractionManager _interactionManager = null!;
        private UiManager _uiManager = null!;

        [Header("Movement Settings")] [SerializeField]
        private MovementSettings movementSettings = null!;
        [SerializeField] private Transform head = null!;

        public ShipController? shipToEnter;

        public UnityEvent onEnterShip = new();
        public UnityEvent onExitShip = new();

        private PlanetaryMovement _planetaryMovement = null!;
        private Health _playerHealth = null!;
        private Oxygen _playerOxygen = null!;

        private PlayerState _playerState = PlayerState.InZeroG;
        private Rigidbody _rb = null!;

        private SpaceMovement _spaceMovement = null!;

        private void Start()
        {
            _cameraController = CameraController.Instance;
            _uiManager = UiManager.Instance;
            _interactionManager = GetComponent<InteractionManager>();

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
            if (playerCamera) _cameraController?.Register(playerCamera);

            _cameraController?.SetActiveCamera(playerCamera);
        }

        private void OnDisable()
        {
            if (playerCamera) _cameraController.Unregister(playerCamera);
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
            var inputManager = InputManager.Instance;
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
            _interactionManager.OnInteractInput();
        }

        private void OnToggleInventory()
        {
            _uiManager.ToggleInventory();
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

            if (Physics.OverlapSphereNonAlloc(transform.position, 50, colliders, movementSettings.groundLayer) != 0) return;

            _playerState = PlayerState.InZeroG;
            UpdateMovementComponents();
        }

        private void UpdateZeroGMovement()
        {
            var colliders = new Collider[1];

            if (Physics.OverlapSphereNonAlloc(transform.position, 50, colliders, movementSettings.groundLayer) == 0) return;

            _playerState = PlayerState.InGravity;
            UpdateMovementComponents();
        }

        public void EnterShip(ShipController ship)
        {
            shipToEnter = ship;
            transform.parent = ship.transform;
            gameObject.SetActive(false);

            _playerState = PlayerState.OnShip;

            onEnterShip.Invoke();
            _playerOxygen.Reset();
            _uiManager.ClearHint();
            _uiManager.TransitionToState(UIState.Ship);
            UpdateMovementComponents();
        }

        public void ExitShip()
        {
            if (!shipToEnter) return;

            transform.parent = null;
            gameObject.SetActive(true);
            var position = shipToEnter.transform.position + shipToEnter.transform.forward * 2;

            transform.position = position;

            _cameraController.SetActiveCamera(playerCamera);

            if (shipToEnter && shipToEnter.CurrentState == ShipState.Landed)
                _playerState = PlayerState.InGravity;
            else
                _playerState = PlayerState.InZeroG;

            onExitShip.Invoke();
            shipToEnter = null;
            _uiManager.ClearHint();
            _uiManager.TransitionToState(UIState.ZeroG); // FIX
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
            _planetaryMovement.planetTransform = null;
            _planetaryMovement.enabled = false;
            _rb.constraints = RigidbodyConstraints.None;
            StartCoroutine(RealignHead());
        }

        private IEnumerator RealignHead()
        {
            while (head.localRotation != Quaternion.identity)
            {
                head.localRotation = Quaternion.Lerp(head.localRotation, Quaternion.identity, Time.deltaTime * 5);
                yield return null;
            }

            head.localRotation = Quaternion.identity;
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