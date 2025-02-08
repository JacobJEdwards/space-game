using UnityEngine;

public class ZeroGState : IMovementState
{
    private readonly ZeroGMovementCapability _movementCapability;

    public ZeroGState(ZeroGMovementCapability movementCapability)
    {
        _movementCapability = movementCapability;
    }

    public void EnterState() { }

    public void ExitState() { }

    public void UpdateState()
    {
        _movementCapability.ProcessMovement();
    }

    public void HandleInput(MovementInputData inputData)
    {
        _movementCapability.HandleInput(inputData);
    }
}