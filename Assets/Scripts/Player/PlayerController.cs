using System;
using JetBrains.Annotations;
using Unity.Assertions;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum PlayerState
{
    OnShip,
    InZeroG,
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")] [SerializeField]
    private CinemachineCamera playerCamera;

    [SerializeField] private CameraController cameraController;

    [CanBeNull] public ShipController shipToEnter;

    public UnityEvent onRequestEnterShip;

    public PlayerState PlayerState { get; private set; }

    private Health _playerHealth;
    private Oxygen _playerOxygen;

    private void Start()
    {
        shipToEnter = null;
        var spaceInput = GetComponentInChildren<SpaceInput>();
        spaceInput.OnInteractPressed += OnInteract;

        _playerHealth = GetComponentInChildren<Health>();
        _playerOxygen = GetComponent<Oxygen>();

        UnityEngine.Assertions.Assert.IsNotNull(_playerHealth);
        UnityEngine.Assertions.Assert.IsNotNull(_playerOxygen);

        PlayerState = PlayerState.InZeroG;
    }

    private void FixedUpdate()
    {
        if (PlayerState == PlayerState.InZeroG)
        {
            _playerOxygen.TakeDamage(1f * Time.fixedDeltaTime);
        }

        if (_playerOxygen.CurrentOxygen <= 0)
        {
            _playerHealth.TakeDamage(1);
        }
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

    private void OnEnterShip()
    {
        Assert.IsNotNull(shipToEnter);

        transform.parent = shipToEnter!.transform;
        gameObject.SetActive(false);

        onRequestEnterShip?.Invoke();

        PlayerState = PlayerState.OnShip;
        _playerOxygen.Reset();
    }

    private void OnExitShip()
    {
        transform.parent = null;

        gameObject.SetActive(true);

        CameraController.SetActiveCamera(playerCamera);

        PlayerState = PlayerState.InZeroG;
    }

    public void AssignShipToEnter(ShipController ship)
    {
        shipToEnter = ship;
        shipToEnter?.onRequestExitShip.AddListener(OnExitShip);
    }

    public void RemoveShipToEnter()
    {
        if (!shipToEnter) return;

        shipToEnter.onRequestExitShip.RemoveListener(OnExitShip);
        shipToEnter = null;
    }


    #region Input Methods

    public void OnInteract()
    {
        if (shipToEnter) OnEnterShip();
    }

    #endregion
}