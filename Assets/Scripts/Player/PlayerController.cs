using JetBrains.Annotations;
using Managers;
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
    [SerializeField] private UiManager uiManager;

    [CanBeNull] public ShipController shipToEnter;

    public UnityEvent onEnterShip;
    public UnityEvent onExitShip;

    private PlayerState PlayerState { get; set; }

    private Health _playerHealth;
    private Oxygen _playerOxygen;

    private void Start()
    {
        var spaceInput = GetComponentInChildren<SpaceInput>();
        spaceInput.OnInteractPressed += OnInteractionInput;

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
        uiManager.SetHint("");
        uiManager.TransitionToState(UIState.Ship);
    }

    public void ExitShip()
    {
        transform.parent = null;
        gameObject.SetActive(true);
        CameraController.SetActiveCamera(playerCamera);

        onExitShip?.Invoke();
        PlayerState = PlayerState.InZeroG;
        shipToEnter = null;
        uiManager.SetHint("");
        uiManager.TransitionToState(UIState.ZeroG);
    }
}
}
