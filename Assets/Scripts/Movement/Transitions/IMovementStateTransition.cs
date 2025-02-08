public interface IMovementStateTransition
{
    bool CanTransition { get; }
    float TransitionProgress { get; }
    void StartTransition();
    void UpdateTransition();
    bool IsTransitionComplete();
}