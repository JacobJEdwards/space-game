using UnityEngine;

public abstract class BaseMovementCapability : IMovementCapability
{
    protected readonly Rigidbody Rigidbody;
    protected readonly MovementConfig Config;
    protected MovementInputData Input;

    protected float Glide;
    protected float VerticalGlide;
    protected float HorizontalGlide;
    protected float CurrentBoostAmount;
    protected bool IsBoosting;

    protected BaseMovementCapability(Rigidbody rigidbody, MovementConfig config)
    {
        Rigidbody = rigidbody;
        Config = config;
        CurrentBoostAmount = config.maxBoostAmount;
    }

    public virtual void HandleInput(MovementInputData input)
    {
        Input = input;
        IsBoosting = input.IsBoosting;
    }

    public virtual void ProcessMovement()
    {
        HandleBoosting();
        HandleThrust();
        HandleVerticalMovement();
        HandleHorizontalMovement();
    }

    protected virtual void HandleBoosting()
    {
        if (IsBoosting && CurrentBoostAmount > 0f)
        {
            CurrentBoostAmount -= Config.boostDepreciationRate;
            IsBoosting = CurrentBoostAmount > 0f;
        }
        else if (CurrentBoostAmount < Config.maxBoostAmount)
        {
            CurrentBoostAmount += Config.boostRechargeRate;
        }
    }

    protected abstract void HandleThrust();
    protected abstract void HandleVerticalMovement();
    protected abstract void HandleHorizontalMovement();
}