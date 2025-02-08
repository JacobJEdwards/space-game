using UnityEngine;

public class ShipMovementCapability : BaseMovementCapability
{
    private readonly ShipMovementConfig _shipConfig;

    public ShipMovementCapability(Rigidbody rigidbody, ShipMovementConfig config)
        : base(rigidbody, config)
    {
        _shipConfig = config;
    }

    public override void ProcessMovement()
    {
        base.ProcessMovement();
        HandleRotation();
    }

    protected override void HandleThrust()
    {
        if (Mathf.Abs(Input.Thrust) > 0.1f)
        {
            var currentThrust = Config.thrust * (IsBoosting ? Config.boostMultiplier : 1f);
            Rigidbody.AddRelativeForce(Vector3.forward *
                                       (Input.Thrust * currentThrust * Time.fixedDeltaTime));
            Glide = currentThrust;
        }
        else
        {
            Rigidbody.AddRelativeForce(Vector3.forward * (Glide * Time.fixedDeltaTime));
            Glide *= Config.thrustGlideReduction;
        }
    }

    protected override void HandleVerticalMovement()
    {
        if (Mathf.Abs(Input.UpDown) > 0.1f)
        {
            Rigidbody.AddRelativeForce(Vector3.up * (Input.UpDown * Config.upThrust * Time.fixedDeltaTime));
            VerticalGlide = Input.UpDown * Config.upThrust;
        }
        else
        {
            Rigidbody.AddRelativeForce(Vector3.up * (VerticalGlide * Time.fixedDeltaTime));
            VerticalGlide *= Config.upDownGlideReduction;
        }
    }

    protected override void HandleHorizontalMovement()
    {
        if (Mathf.Abs(Input.Strafe) > 0.1f)
        {
            Rigidbody.AddRelativeForce(Vector3.right *
                                       (Input.Strafe * Config.strafeThrust * Time.fixedDeltaTime));
            HorizontalGlide = Input.Strafe * Config.strafeThrust;
        }
        else
        {
            Rigidbody.AddRelativeForce(Vector3.right * (HorizontalGlide * Time.fixedDeltaTime));
            HorizontalGlide *= Config.leftRightGlideReduction;
        }
    }

    private void HandleRotation()
    {
        Rigidbody.AddRelativeTorque(Vector3.back *
                                    (Input.Roll * _shipConfig.rollTorque * Time.fixedDeltaTime));
        Rigidbody.AddRelativeTorque(Vector3.right * (Mathf.Clamp(-Input.PitchYaw.y, -1f, 1f) *
                                                     _shipConfig.pitchTorque * Time.fixedDeltaTime));
        Rigidbody.AddRelativeTorque(Vector3.up * (Mathf.Clamp(Input.PitchYaw.x, -1f, 1f) *
                                                  _shipConfig.yawTorque * Time.fixedDeltaTime));
    }
}