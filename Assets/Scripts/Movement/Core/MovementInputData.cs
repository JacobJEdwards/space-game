using UnityEngine;

public struct MovementInputData
{
    public float Thrust { get; set; }
    public float UpDown { get; set; }
    public float Strafe { get; set; }
    public float Roll { get; set; }
    public Vector2 PitchYaw { get; set; }
    public bool IsBoosting { get; set; }

    public Vector2 MoveInput { get; set; }
    public Vector2 LookInput { get; set; }
    public bool IsJumping { get; set; }
    public bool IsSprinting { get; set; }
    public bool IsCrouching { get; set; }
}