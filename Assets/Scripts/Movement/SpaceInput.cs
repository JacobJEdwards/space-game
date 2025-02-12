using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SpaceInput : MonoBehaviour
{
    public float forward;
    public float upDown;
    public float strafe;

    public float roll;
    public bool boost;

    public float vertical;
    public float horizontal;

    public Vector2 PitchYaw => new(vertical, horizontal);

    public event UnityAction OnInteractPressed;
    public event UnityAction OnShootPressed;
    public event UnityAction OnLandingPressed;

    #region Inputs

    public void OnForward(InputAction.CallbackContext context)
    {
        forward = context.ReadValue<float>();
    }

    public void OnVertical(InputAction.CallbackContext context)
    {
        upDown = context.ReadValue<float>();
    }

    public void OnHorizontal(InputAction.CallbackContext context)
    {
        strafe = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll = context.ReadValue<float>();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        boost = context.performed;
    }

    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        vertical = value.y;
        horizontal = value.x;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) OnInteractPressed?.Invoke();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed) OnShootPressed?.Invoke();
    }

    public void OnLanding(InputAction.CallbackContext context)
    {
        if (context.performed) OnLandingPressed?.Invoke();
    }

    #endregion
}