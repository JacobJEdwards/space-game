using UnityEngine;

public class FPSMovementCapability : IMovementCapability
{
    private readonly CharacterController _controller;
    private readonly FPSMovementConfig _config;
    private readonly Transform _cameraTransform;
    private readonly Transform _playerTransform;

    private Vector3 _velocity;
    private float _verticalRotation;
    private bool _isGrounded;
    private bool _isCrouching;

    public FPSMovementCapability(
        CharacterController controller,
        FPSMovementConfig config,
        Transform cameraTransform,
        Transform playerTransform)
    {
        _controller = controller;
        _config = config;
        _cameraTransform = cameraTransform;
        _playerTransform = playerTransform;
    }

    public void ProcessMovement()
    {
        HandleGroundCheck();
        HandleGravity();
        HandleMovement();
    }

    public void HandleInput(MovementInputData inputData)
    {
        // Handle looking
        _playerTransform.Rotate(Vector3.up * inputData.LookInput.x * _config.mouseSensitivity);

        _verticalRotation -= inputData.LookInput.y * _config.mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -_config.maxLookAngle, _config.maxLookAngle);
        _cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);

        // Handle movement
        var move = _playerTransform.right * inputData.MoveInput.x +
                  _playerTransform.forward * inputData.MoveInput.y;

        var speed = inputData.IsSprinting ? _config.sprintSpeed : _config.walkSpeed;
        _controller.Move(move * (speed * Time.deltaTime));

        // Handle jumping
        if (inputData.IsJumping && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_config.jumpForce * -2f * _config.gravity);
        }

        // Handle crouching
        if (inputData.IsCrouching != _isCrouching)
        {
            _isCrouching = inputData.IsCrouching;
            var targetHeight = _isCrouching ? _config.crouchHeight : _config.standingHeight;
            _controller.height = targetHeight;
            _controller.center = Vector3.up * (targetHeight / 2f);
        }
    }

    private void HandleGroundCheck()
    {
        // Spherecast check for ground
        _isGrounded = Physics.SphereCast(
            _playerTransform.position,
            _controller.radius,
            Vector3.down,
            out _,
            _config.groundDistance,
            _config.groundMask
        );
    }

    private void HandleGravity()
    {
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Small constant downward force when grounded
        }

        _velocity.y += _config.gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        // Additional movement handling like sliding, wall running, etc. could go here
    }
}