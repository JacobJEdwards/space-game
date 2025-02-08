public class ShipState : IMovementState
{
    private readonly ShipMovementCapability _movementCapability;

    public ShipState(ShipMovementCapability movementCapability)
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