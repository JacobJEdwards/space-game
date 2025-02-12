using System;
using UnityEngine;

namespace Movement
{
[RequireComponent(typeof(SpaceInput))]
public class SpaceMovement : MonoBehaviour
{
    [SerializeField] private SpaceMovementConfig config;

    private Camera _camera;

    private float _glide;
    private float _horizontalGlide;

    private Rigidbody _rb;

    public SpaceInput SpaceInput { get; private set; }

    private float _verticalGlide;

    public float CurrentBoostAmount { get; private set; }

    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _camera = Camera.main;
        SpaceInput = GetComponent<SpaceInput>();

        CurrentBoostAmount = config.MaxBoostAmount;

        if (config.UseCameraDirection && _camera)
        {
            _rb.rotation = _camera.transform.rotation;
        }
    }

    private void FixedUpdate()
    {
        HandleBoosting();
        HandleMovement();
    }

    private void HandleBoosting()
    {
        if (SpaceInput.boost && CurrentBoostAmount > 0f)
        {
            CurrentBoostAmount -= config.BoostDepreciationRate;
            SpaceInput.boost = CurrentBoostAmount > 0f;
        }
        else
        {
            if (CurrentBoostAmount < config.MaxBoostAmount) CurrentBoostAmount += config.BoostRechargeRate;
        }
    }

    private void HandleMovement()
    {
        var back = Vector3.back;
        var forward = Vector3.forward;
        var right = Vector3.right;
        var up = Vector3.up;

        if (config.UseCameraDirection)
        {
            back = -_camera.transform.forward;
            forward = _camera.transform.forward;
            right = _camera.transform.right;
            up = _camera.transform.up;
        }

        // Roll
        _rb.AddRelativeTorque(back * (SpaceInput.roll * config.RollTorque * Time.fixedDeltaTime));

        // Pitch
        _rb.AddRelativeTorque(right *
                              (Math.Clamp(-SpaceInput.vertical, -1f, 1f) * config.PitchTorque * Time.fixedDeltaTime));

        // Yaw
        _rb.AddRelativeTorque(up *
                              (Math.Clamp(SpaceInput.horizontal, -1f, 1f) * config.YawTorque * Time.fixedDeltaTime));

        // Thrust
        if (Mathf.Abs(SpaceInput.forward) > 0.1f)
        {
            var currentThrust = config.Thrust;
            if (SpaceInput.boost) currentThrust *= config.BoostMultiplier;

            if (config.UseCameraDirection)
            {
                _rb.AddForce(forward * (SpaceInput.forward * currentThrust * Time.fixedDeltaTime));
            }
            else
            {
                _rb.AddRelativeForce(forward * (SpaceInput.forward * currentThrust * Time.fixedDeltaTime));
            }
            _glide = currentThrust;
        }
        else
        {
            if (config.UseCameraDirection)
            {
                _rb.AddForce(forward * (_glide * Time.fixedDeltaTime));
            } else {
                _rb.AddRelativeForce(forward * (_glide * Time.fixedDeltaTime));
            }
            _glide *= config.ThrustGlideReduction;
        }

        // Up/Down
        if (Mathf.Abs(SpaceInput.upDown) > 0.1f)
        {
            if (config.UseCameraDirection)
            {
                _rb.AddForce(up * (SpaceInput.upDown * config.UpThrust * Time.fixedDeltaTime));
            } else {
                _rb.AddRelativeForce(up * (SpaceInput.upDown * config.UpThrust * Time.fixedDeltaTime));
            }
            _verticalGlide = SpaceInput.upDown * config.UpThrust;
        }
        else
        {
            if (config.UseCameraDirection)
            {
                _rb.AddForce(up * (_verticalGlide * Time.fixedDeltaTime));
            } else {
                _rb.AddRelativeForce(up * (_verticalGlide * Time.fixedDeltaTime));
            }
            _verticalGlide *= config.UpDownGlideReduction;
        }

        // Strafe
        if (Mathf.Abs(SpaceInput.strafe) > 0.1f)
        {
            if (config.UseCameraDirection)
            {
                _rb.AddForce(right * (SpaceInput.strafe * config.StrafeThrust * Time.fixedDeltaTime));
            } else {
                _rb.AddRelativeForce(right * (SpaceInput.strafe * config.StrafeThrust * Time.fixedDeltaTime));
            }
            _horizontalGlide = SpaceInput.strafe * config.StrafeThrust;
        }
        else
        {
            if (config.UseCameraDirection)
            {
                _rb.AddForce(right * (_horizontalGlide * Time.fixedDeltaTime));
            } else {
                _rb.AddRelativeForce(right * (_horizontalGlide * Time.fixedDeltaTime));
            }
            _horizontalGlide *= config.LeftRightGlideReduction;
        }
    }
}
}
