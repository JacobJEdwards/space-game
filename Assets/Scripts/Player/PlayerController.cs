using JetBrains.Annotations;
using Managers;
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
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField]
    private CinemachineCamera playerCamera;
    [SerializeField] private CameraController cameraController;
    [SerializeField]
    private InteractionManager interactionManager;

    [CanBeNull] public ShipController shipToEnter;

    public UnityEvent onEnterShip;
    public UnityEvent onExitShip;

    private PlayerState PlayerState { get; set; }

    private Health _playerHealth;
    private Oxygen _playerOxygen;

    private void Start()
    {
        var spaceInput = GetComponentInChildren<SpaceInput>();
        spaceInput.OnInteractPressed += OnInteract;

        _playerHealth = GetComponentInChildren<Health>();
        _playerOxygen = GetComponent<Oxygen>();

        UnityEngine.Assertions.Assert.IsNotNull(_playerHealth);
        UnityEngine.Assertions.Assert.IsNotNull(_playerOxygen);
        UnityEngine.Assertions.Assert.IsNotNull(interactionManager);

        PlayerState = PlayerState.InZeroG;
    }

    private void OnInteractionInput()
    {
        interactionManager.OnInteractInput();
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

    public void EnterShip(ShipController ship)
    {
        shipToEnter = ship;
        transform.parent = ship.transform;
        gameObject.SetActive(false);

        onEnterShip?.Invoke();
        PlayerState = PlayerState.OnShip;
        _playerOxygen.Reset();
    }

    public void ExitShip()
    {
        transform.parent = null;
        gameObject.SetActive(true);
        CameraController.SetActiveCamera(playerCamera);

        onExitShip?.Invoke();
        PlayerState = PlayerState.InZeroG;
        shipToEnter = null;
    }

    private void OnEnterShip()
    {
        Assert.IsNotNull(shipToEnter);

        transform.parent = shipToEnter!.transform;
        gameObject.SetActive(false);

        onEnterShip?.Invoke();

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
}
