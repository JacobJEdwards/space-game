using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ZeroGMovement : MonoBehaviour
{
    [SerializeField]
    private ZeroGMovementConfig zeroGConfig;
    [SerializeField]
    private CinemachineCamera playerCamera;
    [SerializeField]
    private MovementStateManager movementStateManager;

    private void Start()
    {
        movementStateManager.InitialiseZeroGState(GetComponent<Rigidbody>(), zeroGConfig, playerCamera);
        playerCamera.enabled = true;
    }

    #region Input Methods
    public void OnThrust(InputAction.CallbackContext context)
    {
        movementStateManager.HandleInput(new MovementInputData { Thrust = context.ReadValue<float>() });
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        movementStateManager.HandleInput(new MovementInputData { UpDown = context.ReadValue<float>() });
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        movementStateManager.HandleInput(new MovementInputData { Strafe = context.ReadValue<float>() });
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        movementStateManager.HandleInput(new MovementInputData { Roll = context.ReadValue<float>() });
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        movementStateManager.HandleInput(new MovementInputData { IsBoosting = context.ReadValueAsButton() });
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new NotImplementedException("OnInteract");
    }
    #endregion
}
