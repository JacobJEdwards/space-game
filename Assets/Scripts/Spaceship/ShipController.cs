using Unity.Cinemachine;
using UnityEngine;
using Player;
using Managers;

namespace Spaceship
{

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Camera Settings")] [SerializeField]
    private CameraController cameraController;
    [SerializeField] private CinemachineCamera shipThirdPersonCamera;
    [SerializeField] private CinemachineCamera shipFirstPersonCamera;
    [SerializeField] private UiManager uiManager;


    private Health _health;
    private Rigidbody _rb;
    private PlayerController _currentPlayer;

    public bool IsOccupied => _currentPlayer;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        var spaceInput = GetComponentInChildren<SpaceInput>();
        spaceInput.OnInteractPressed += OnInteract;

        _health = GetComponentInChildren<Health>();
        // _health.onDeath.AddListener(ForcePlayerExit); //todo

    }

    private void FixedUpdate()
    {
        _rb.isKinematic = !IsOccupied;
    }

    private void OnEnable()
    {
        if (shipThirdPersonCamera) CameraController.Register(shipThirdPersonCamera);
        if (shipFirstPersonCamera) CameraController.Register(shipFirstPersonCamera);
    }

    private void OnDisable()
    {
        if (shipThirdPersonCamera) CameraController.Unregister(shipThirdPersonCamera);
        if (shipFirstPersonCamera) CameraController.Unregister(shipFirstPersonCamera);
    }


    public void PlayerEnteredShip(PlayerController player)
    {
        _currentPlayer = player;
        CameraController.SetActiveCamera(shipThirdPersonCamera);
    }

    public void PlayerExitShip()
    {
        if (!_currentPlayer) return;

        _currentPlayer.ExitShip();
        _currentPlayer = null;
        uiManager.SetHint("");
        uiManager.TransitionToState(UIState.ZeroG);
    }

    public void ForcePlayerExit()
    {
        if (!_currentPlayer) return;
        _currentPlayer.ExitShip();
        _currentPlayer = null;
    }

    public void OnInteract()
    {
        if (IsOccupied) PlayerExitShip();
    }

    public void OnSwitchCamera()
    {
        if (!IsOccupied) return;

        CameraController.SetActiveCamera(CameraController.IsActive(shipThirdPersonCamera)
            ? shipFirstPersonCamera
            : shipThirdPersonCamera);
    }
}
}
