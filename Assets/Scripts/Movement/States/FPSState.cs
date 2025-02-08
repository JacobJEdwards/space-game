using UnityEngine;

public class FPSState : IMovementState
{
    private readonly FPSMovementCapability _movementCapability;
    private readonly GameObject _playerObject;

    public FPSState(FPSMovementCapability movementCapability, GameObject playerObject)
    {
        _movementCapability = movementCapability;
        _playerObject = playerObject;
    }

    public void EnterState()
    {
        // Lock cursor for FPS control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitState()
    {
        // Unlock cursor when leaving FPS state
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdateState()
    {
        _movementCapability.ProcessMovement();
    }

    public void HandleInput(MovementInputData inputData)
    {
        _movementCapability.HandleInput(inputData);
    }

}