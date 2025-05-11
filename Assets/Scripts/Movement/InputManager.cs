#nullable enable

using UnityEngine;
using UnityEngine.Events;

namespace Movement
{
    public class InputManager : MonoBehaviour
    {
        private PlayerControls _playerControls = null!;
        public static InputManager Instance { get; private set; } = null!;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _playerControls = new PlayerControls();
            _playerControls.Enable();
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

        // how can i do this without using unity action?
        // answer:
        // public void SetOnInteractPressed(Action action)
        public void SetOnInteractPressed(UnityAction action)
        {
            _playerControls.SpaceControls.Interact.performed += _ => action.Invoke();
        }

        public void SetOnHyperdrivePressed(UnityAction action)
        {
            _playerControls.SpaceControls.Hyperdrive.performed += _ => action.Invoke();
        }

        public void SetOnInventoryPress(UnityAction action)
        {
            _playerControls.SpaceControls.ToggleInventory.performed += _ => action.Invoke();
        }

        public void SetOnPausePressed(UnityAction action)
        {
            _playerControls.SpaceControls.TogglePause.performed += _ => action.Invoke();
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