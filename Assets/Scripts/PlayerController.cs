using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private ZeroGMovementConfig zeroGConfig;
    [SerializeField] private ShipMovementConfig shipConfig;
    [SerializeField] private CinemachineCamera playerCamera;



    private IMovementState _currentState;
    private Rigidbody _rigidbody;
    private MovementInputData _inputData;

    private bool _isControllingShip;
    private bool _isInShipZone;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _inputData = new MovementInputData();

        SwitchToZeroGState();

        var shipInteractionZone = FindFirstObjectByType<ShipInteractionZone>();
        if (shipInteractionZone)
        {
            shipInteractionZone.onPlayerEnterZone.AddListener(OnEnterShipZone);
            shipInteractionZone.onPlayerExitZone.AddListener(OnExitShipZone);
        }
    }

    private void SwitchToZeroGState()
    {
        var capability = new ZeroGMovementCapability(_rigidbody, zeroGConfig, playerCamera);
        _currentState = new ZeroGState(capability);
        _isControllingShip = false;
    }

    private void SwitchToShipState()
    {
        var capability = new ShipMovementCapability(_rigidbody, shipConfig);
        _currentState = new ShipState(capability);
        _isControllingShip = true;
    }

    private void OnEnterShipZone()
    {
        _isInShipZone = true;
    }

    private void OnExitShipZone()
    {
        _isInShipZone = false;
    }

    private void FixedUpdate()
    {
        _currentState.UpdateState();
    }

    #region Input Methods
    public void OnThrust(InputAction.CallbackContext context)
    {
        _inputData.Thrust = context.ReadValue<float>();
        _currentState.HandleInput(_inputData);
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        _inputData.UpDown = context.ReadValue<float>();
        _currentState.HandleInput(_inputData);
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        _inputData.Strafe = context.ReadValue<float>();
        _currentState.HandleInput(_inputData);
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        _inputData.Roll = context.ReadValue<float>();
        _currentState.HandleInput(_inputData);
    }

    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        _inputData.PitchYaw = context.ReadValue<Vector2>();
        _currentState.HandleInput(_inputData);
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        _inputData.IsBoosting = context.ReadValueAsButton();
        _currentState.HandleInput(_inputData);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_isInShipZone) return;

        if (_isControllingShip)
        {
            SwitchToZeroGState();
            _rigidbody.isKinematic = false;
            playerCamera.enabled = true;
            playerCamera.Priority = 10;
            var shipCamera = GameObject.FindGameObjectWithTag("ShipCamera")?.GetComponent<CinemachineCamera>();
            if (shipCamera)
            {
                shipCamera.enabled = false;
                shipCamera.Priority = 0;
            }
        }
        else
        {
            SwitchToShipState();
            _rigidbody.isKinematic = true;
            playerCamera.enabled = false;
            playerCamera.Priority = 0;
            var shipCamera = GameObject.FindGameObjectWithTag("ShipCamera")?.GetComponent<CinemachineCamera>();
            if (shipCamera)
            {
                shipCamera.enabled = true;
                shipCamera.Priority = 10;
            }
        }
    }
    #endregion
}
