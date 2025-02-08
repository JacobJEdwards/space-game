using UnityEngine;

[CreateAssetMenu(fileName = "FPSMovementConfig", menuName = "Movement/FPS Config")]
public class FPSMovementConfig : MovementConfig
{
    [Header("FPS Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float groundDistance = 0.2f;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public LayerMask groundMask;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;
}