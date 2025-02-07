using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField]
    private float yawTorque = 500f;
    [SerializeField]
    private float pitchTorque = 1000f;
    [SerializeField]
    private float rollTorque = 1000f;
    [SerializeField]
    private float thrust = 100f;
    [SerializeField]
    private float upThrust = 50f;
    [SerializeField]
    private float strafeThrust = 50f;
    [SerializeField, Range(0.001f, 0.999f)]
    private float thrustGlideReduction = 0.99f;
    [SerializeField, Range(0.001f, 0.999f)]
    private float upDownGlideReduction = 0.99f;
    [SerializeField, Range(0.001f, 0.999f)]
    private float leftRightGlideReduction = 0.99f;

    [Header("Ship Boost")]
    [SerializeField]
    private float maxBoostAmount = 2f;
    [SerializeField]
    private float boostDepreciationRate = 0.25f;
    [SerializeField]
    private float boostRechargeRate = 0.1f;
    [SerializeField]
    private float boostMultiplier = 5f;

    private bool _boosting;
    private float _currentBoostAmount;


    private float _glide;
    private float _verticalGlide;
    private float _horizontalGlide;

    private Rigidbody _rb;

    private float _thrustInput;
    private float _upDownInput;
    private float _strafeInput;
    private float _rollInput;
    private Vector2 _pitchYawInput;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentBoostAmount = maxBoostAmount;
    }

    void FixedUpdate()
    {
        HandleBoosting();
        HandleMovement();
    }

    private void HandleBoosting()
    {
        if (_boosting && _currentBoostAmount > 0f)
        {
            _currentBoostAmount -= boostDepreciationRate;
            if (_currentBoostAmount < 0f)
            {
                _boosting = false;
            }
        }
        else
        {
            if (_currentBoostAmount < maxBoostAmount)
            {
                _currentBoostAmount += boostRechargeRate;
            }
        }
    }

    private void HandleMovement()
    {
        // Roll
        _rb.AddRelativeTorque(Vector3.back * (_rollInput * rollTorque * Time.fixedDeltaTime));

        // Pitch
        _rb.AddRelativeTorque(Vector3.right * (Math.Clamp(-_pitchYawInput.y, -1f, 1f) * pitchTorque * Time.fixedDeltaTime));

        // Yaw
        _rb.AddRelativeTorque(Vector3.up * (Math.Clamp(_pitchYawInput.x, -1f, 1f) * yawTorque * Time.fixedDeltaTime));

        // Thrust
        if (_thrustInput is > 0.1f or < -0.1f)
        {
            var currentThrust = thrust;
            if (_boosting)
            {
                currentThrust *= boostMultiplier;
            }

            _rb.AddRelativeForce(Vector3.forward * (_thrustInput * currentThrust * Time.fixedDeltaTime));
            _glide = currentThrust;
        }
        else
        {
            _rb.AddRelativeForce(Vector3.forward * (_glide * Time.fixedDeltaTime));
            _glide *= thrustGlideReduction;
        }

        // Up/Down
        if (_upDownInput is > 0.1f or < -0.1f)
        {
            _rb.AddRelativeForce(Vector3.up * (_upDownInput * upThrust * Time.fixedDeltaTime));
            _verticalGlide = _upDownInput * upThrust;
        }
        else
        {
            _rb.AddRelativeForce(Vector3.up * (_verticalGlide * Time.fixedDeltaTime));
            _verticalGlide *= upDownGlideReduction;
        }

        if (_strafeInput is > 0.1f or < -0.1f)
        {
            _rb.AddRelativeForce(Vector3.right * (_strafeInput * strafeThrust * Time.fixedDeltaTime));
             _horizontalGlide= _strafeInput * strafeThrust;
        }
        else
        {
            _rb.AddRelativeForce(Vector3.right * (_horizontalGlide * Time.fixedDeltaTime));
            _horizontalGlide *= leftRightGlideReduction;
        }



    }

    #region Input Methods
    public void OnThrust(InputAction.CallbackContext context)
    {
        _thrustInput = context.ReadValue<float>();
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        _upDownInput = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        _strafeInput = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        _rollInput = context.ReadValue<float>();
    }

    public void OnPitchYaw(InputAction.CallbackContext context)
    {
        _pitchYawInput = context.ReadValue<Vector2>();
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        _boosting = context.performed;
    }
    #endregion
}
