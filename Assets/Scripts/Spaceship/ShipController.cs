using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Camera Settings")] [SerializeField]
    private CameraController cameraController;

    [SerializeField] private CinemachineCamera shipThirdPersonCamera;

    [SerializeField] private CinemachineCamera shipFirstPersonCamera;

    [SerializeField] private PlayerController player;

    private ShipInteractionZone _shipInteractionZone;

    public UnityEvent onRequestExitShip;

    private Rigidbody _rb;

    public bool IsOccupied { get; private set; }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        var spaceInput = GetComponentInChildren<SpaceInput>();

        spaceInput.OnInteractPressed += OnInteract;

        _shipInteractionZone = GetComponentInChildren<ShipInteractionZone>();

        _shipInteractionZone.onPlayerEnterZone.AddListener(AssignPlayer);
        _shipInteractionZone.onPlayerExitZone.AddListener(RemovePlayer);

        player.onRequestEnterShip.AddListener(PlayerEnteredShip);
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

    private void AssignPlayer(PlayerController zeroGController)
    {
        player = zeroGController;
        player.AssignShipToEnter(this);
    }

    private void RemovePlayer(PlayerController zeroGController)
    {
        player.RemoveShipToEnter();
        player = null;
    }

    private void PlayerEnteredShip()
    {
        IsOccupied = true;
        CameraController.SetActiveCamera(shipThirdPersonCamera);
    }

    private void PlayerExitedShip()
    {
        IsOccupied = false;
        onRequestExitShip?.Invoke();
    }

    public void OnInteract()
    {
        if (IsOccupied) PlayerExitedShip();
    }

    public void OnSwitchCamera()
    {
        if (!IsOccupied) return;

        CameraController.SetActiveCamera(CameraController.IsActive(shipThirdPersonCamera)
            ? shipFirstPersonCamera
            : shipThirdPersonCamera);
    }
}