using UnityEngine;

public abstract class BaseTransitionState : IMovementStateTransition
{
    protected readonly IMovementState FromState;
    protected readonly IMovementState ToState;
    protected float TransitionDuration;
    protected float CurrentTime;
    protected bool IsActive;

    public float TransitionProgress => Mathf.Clamp01(CurrentTime / TransitionDuration);
    public bool CanTransition { get; protected set; }

    protected BaseTransitionState(IMovementState fromState, IMovementState toState, float duration)
    {
        FromState = fromState;
        ToState = toState;
        TransitionDuration = duration;
        CanTransition = true;
    }

    public virtual void StartTransition()
    {
        IsActive = true;
        CurrentTime = 0f;
    }

    public virtual void UpdateTransition()
    {
        if (!IsActive) return;

        CurrentTime += Time.deltaTime;
        HandleTransitionUpdate();

        if (IsTransitionComplete())
        {
            IsActive = false;
        }
    }

    public virtual bool IsTransitionComplete()
    {
        return CurrentTime >= TransitionDuration;
    }

    protected abstract void HandleTransitionUpdate();
}