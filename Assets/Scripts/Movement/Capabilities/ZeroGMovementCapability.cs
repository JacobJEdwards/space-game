using Unity.Cinemachine;
using UnityEngine;

public class ZeroGMovementCapability : BaseMovementCapability
{
    private readonly CinemachineCamera _camera;

    public ZeroGMovementCapability(Rigidbody rigidbody, ZeroGMovementConfig config, CinemachineCamera camera)
        : base(rigidbody, config)
    {
        _camera = camera;
    }

    protected override void HandleThrust()
    {
        if (Mathf.Abs(Input.Thrust) > 0.1f)
        {
            var currentThrust = Config.thrust * (IsBoosting ? Config.boostMultiplier : 1f);
            Rigidbody.AddForce(_camera.transform.forward * (Input.Thrust * currentThrust * Time.fixedDeltaTime));
            Glide = currentThrust;
        }
        else
        {
            Rigidbody.AddForce(_camera.transform.forward * (Glide * Time.fixedDeltaTime));
            Glide *= Config.thrustGlideReduction;
        }
    }

    protected override void HandleVerticalMovement()
    {
        if (Mathf.Abs(Input.UpDown) > 0.1f)
        {
            Rigidbody.AddForce(_camera.transform.up * (Input.UpDown * Config.upThrust * Time.fixedDeltaTime));
            VerticalGlide = Input.UpDown * Config.upThrust;
        }
        else
        {
            Rigidbody.AddForce(_camera.transform.up * (VerticalGlide * Time.fixedDeltaTime));
            VerticalGlide *= Config.upDownGlideReduction;
        }
    }

    protected override void HandleHorizontalMovement()
    {
        if (Mathf.Abs(Input.Strafe) > 0.1f)
        {
            Rigidbody.AddForce(_camera.transform.right * (Input.Strafe * Config.strafeThrust * Time
                .fixedDeltaTime));
            HorizontalGlide = Input.Strafe * Config.strafeThrust;
        }
        else
        {
            Rigidbody.AddForce(_camera.transform.right * (HorizontalGlide * Time.fixedDeltaTime));
            HorizontalGlide *= Config.leftRightGlideReduction;
        }
    }
}