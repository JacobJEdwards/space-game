using System;
using JetBrains.Annotations;
using Managers;
using Movement;
using Spaceship;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Player
{

public enum PlayerState
{
    OnShip,
    InZeroG,
    InGravity
}

[RequireComponent(typeof(Rigidbody), typeof(SpaceInput))]
[RequireComponent(typeof(Health), typeof(Oxygen))]
[RequireComponent(typeof(SpaceMovement), typeof(PlanetaryMovement))]
public class PlayerController : MonoBehaviour
{
    [Serializable]
    private class MovementSettings
    {
        public float runSpeed = 6.0f;
        public float jumpForce = 8.0f;
        public float groundCheckDistance = 0.1f;
        public LayerMask groundLayer;
    }

    [Header("Camera Settings")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private InteractionManager interactionManager;
    [SerializeField] private UiManager uiManager;

    [Header("Movement Settings")]
    [SerializeField] private MovementSettings movementSettings;

    [CanBeNull] public ShipController shipToEnter;

    public UnityEvent onEnterShip;
    public UnityEvent onExitShip;

    private PlayerState _playerState = PlayerState.InZeroG;
    private Health _playerHealth;
    private Oxygen _playerOxygen;
    private Rigidbody _rb;
    private SpaceInput _spaceInput;
    private Vector3 _surfaceNormal;
    private bool _isGrounded;
    private SpaceMovement _spaceMovement;
    private PlanetaryMovement _planetaryMovement;

    private void Start()
    {
        InitialiseComponents();
        ValidateComponents();
    }

    private void InitialiseComponents()
    {
        _spaceInput = GetComponentInChildren<SpaceInput>();
        _spaceInput.OnInteractPressed += OnInteractionInput;

        _playerHealth = GetComponent<Health>();
        _playerOxygen = GetComponent<Oxygen>();
        _rb = GetComponent<Rigidbody>();

        _spaceMovement = GetComponentInChildren<SpaceMovement>();
        _planetaryMovement = GetComponentInChildren<PlanetaryMovement>();

        if (_planetaryMovement) _planetaryMovement.enabled = false;
    }

    private void ValidateComponents()
    {
        UnityEngine.Assertions.Assert.IsNotNull(_spaceInput);
        UnityEngine.Assertions.Assert.IsNotNull(_playerHealth);
        UnityEngine.Assertions.Assert.IsNotNull(_playerOxygen);
        UnityEngine.Assertions.Assert.IsNotNull(_rb);
        UnityEngine.Assertions.Assert.IsNotNull(_spaceMovement);
    }

    private void OnInteractionInput()
    {
        interactionManager.OnInteractInput();
    }

    private void FixedUpdate()
    {
        UpdateOxygenAndHealth();
        UpdateMovementState();
    }

    private void UpdateOxygenAndHealth()
    {
        if (_playerState == PlayerState.InZeroG)
        {
            _playerOxygen.TakeDamage(1f * Time.fixedDeltaTime);
        }

        if (_playerOxygen.CurrentOxygen <= 0)
        {
            _playerHealth.TakeDamage(1);
        }
    }

    private void UpdateMovementState()
    {
        if (_playerState == PlayerState.OnShip) return;

        if (_playerState == PlayerState.InZeroG)
        {
            UpdateZeroGMovement();
        }
        else if (_playerState == PlayerState.InGravity)
        {
            UpdatePlanetaryMovement();
        }

    }

    private void UpdateZeroGMovement()
    {
        // check if near a planet in any direction
        // if near a planet, switch to planetary movement

        for (var i = 0; i < 16; i++)
        {
            var direction = Quaternion.Euler(0, i * 22.5f, 0) * transform.forward;

            if (!Physics.Raycast(transform.position, direction, out var hit, 20f, movementSettings.groundLayer))
                continue;

            _playerState = PlayerState.InGravity;
            _planetaryMovement.SetPlanet(hit.transform);
            UpdateMovementComponents();
            return;
        }
    }

    private void UpdatePlanetaryMovement()
    {
        UpdateGroundedState();
        AlignWithSurface();
    }

    private void UpdateGroundedState()
    {
        if (Physics.Raycast(transform.position, -transform.up, out var hit,
                movementSettings.groundCheckDistance, movementSettings.groundLayer))
        {
            _isGrounded = true;
            _surfaceNormal = hit.normal;
        }
        else
        {
            _isGrounded = false;
        }
    }

    private void AlignWithSurface()
    {
        if (!_isGrounded) return;

        var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
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
        {
            _playerState = PlayerState.InGravity;
        }
        else
        {
            _playerState = PlayerState.InZeroG;
        }

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
    }

    private void EnablePlanetaryMovement()
    {
        print("Enabling planetary movement");
        _spaceMovement.enabled = false;
        _planetaryMovement.enabled = true;
    }

    private void DisableAllMovement()
    {
        _spaceMovement.enabled = false;
        _planetaryMovement.enabled = false;
    }
}
}
