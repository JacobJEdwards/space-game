using UnityEngine;
using UnityEngine.Events;

namespace Movement {

public class InputManager : MonoBehaviour
{
    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    public void OnEnable()
    {
        _playerControls.Enable();
    }

    public void OnDisable()
    {
        _playerControls.Disable();
    }

    public float GetForward()
    {
        return _playerControls.SpaceControls.Thrust.ReadValue<float>();
    }

    public float GetUpDown()
    {
        return _playerControls.SpaceControls.UpDown.ReadValue<float>();
    }

    public float GetStrafe()
    {
        return _playerControls.SpaceControls.Strafe.ReadValue<float>();
    }

    public float GetRoll()
    {
        return _playerControls.SpaceControls.Roll.ReadValue<float>();
    }

    public bool GetBoost()
    {
        return _playerControls.SpaceControls.Boost.ReadValue<float>() > 0.5f;
    }

    public Vector2 GetPitchYaw()
    {
        return _playerControls.SpaceControls.PitchYaw.ReadValue<Vector2>();
    }

    public void SetOnInteractPressed(UnityAction action)
    {
        _playerControls.SpaceControls.Interact.performed += _ => action.Invoke();
    }

    public void SetOnShootPressed(UnityAction action)
    {
        _playerControls.SpaceControls.WeaponFire.performed += _ => action.Invoke();
    }

    public void SetOnShootRelease(UnityAction action)
    {
        _playerControls.SpaceControls.WeaponFire.canceled += _ => action.Invoke();
    }

    public void SetOnLandingPressed(UnityAction action)
    {
        _playerControls.SpaceControls.Land.performed += _ => action.Invoke();
    }

    public void SetOnJumpPressed(UnityAction action)
    {
        _playerControls.SpaceControls.Jump.performed += _ => action.Invoke();
    }

    public void SetOnSprintPress(UnityAction action)
    {
        _playerControls.SpaceControls.Sprint.performed += _ => action.Invoke();
    }

    public void SetOnSprintRelease(UnityAction action)
    {
        _playerControls.SpaceControls.Sprint.canceled += _ => action.Invoke();
    }

    public void SetOnJetpackPress(UnityAction action)
    {
        _playerControls.SpaceControls.Jetpack.performed += _ => action.Invoke();
    }

    public void SetOnJetpackRelease(UnityAction action)
    {
        _playerControls.SpaceControls.Jetpack.canceled += _ => action.Invoke();
    }
}
}