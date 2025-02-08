public interface IMovementState
{
    void EnterState();
    void ExitState();
    void UpdateState();
    void HandleInput(MovementInputData input);
}