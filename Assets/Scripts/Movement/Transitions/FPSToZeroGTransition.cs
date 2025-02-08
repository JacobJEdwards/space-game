using UnityEngine;

public class FPSToZeroGTransition : BaseTransitionState
{
    private readonly Transform _playerTransform;
    private readonly CharacterController _characterController;
    private readonly Rigidbody _rigidbody;
    private readonly Vector3 _launchPoint;
    private readonly Vector3 _launchVelocity;
    private readonly Vector3 _initialPosition;
    private readonly Quaternion _initialRotation;

    public FPSToZeroGTransition(
        IMovementState fromState,
        IMovementState toState,
        Transform playerTransform,
        CharacterController characterController,
        Rigidbody rigidbody,
        Vector3 launchVelocity,
        float duration = 0.5f) : base(fromState, toState, duration)
    {
        _playerTransform = playerTransform;
        _characterController = characterController;
        _rigidbody = rigidbody;
        _launchVelocity = launchVelocity;
        _initialPosition = playerTransform.position;
        _initialRotation = playerTransform.rotation;
        _launchPoint = _initialPosition + Vector3.up * 2f; // Small upward offset for launch
    }

    public override void StartTransition()
    {
        base.StartTransition();

        // Disable character controller during transition
        _characterController.enabled = false;
    }

    protected override void HandleTransitionUpdate()
    {
        var t = TransitionProgress;

        // Move slightly upward before enabling zero-G
        _playerTransform.position = Vector3.Lerp(_initialPosition, _launchPoint, t);
    }

    public override bool IsTransitionComplete()
    {
        if (!base.IsTransitionComplete()) return false;

        // Enable zero-G physics
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = _launchVelocity;
        return true;
    }
}