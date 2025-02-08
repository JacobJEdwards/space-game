using UnityEngine;

public class ZeroGToFPSTransition : BaseTransitionState
{
    private readonly Transform _playerTransform;
    private readonly CharacterController _characterController;
    private readonly Rigidbody _rigidbody;
    private readonly Vector3 _landingPoint;
    private readonly Quaternion _targetRotation;
    private readonly Vector3 _initialPosition;
    private readonly Quaternion _initialRotation;

    public ZeroGToFPSTransition(
        IMovementState fromState,
        IMovementState toState,
        Transform playerTransform,
        CharacterController characterController,
        Rigidbody rigidbody,
        Vector3 landingPoint,
        float duration = 1f) : base(fromState, toState, duration)
    {
        _playerTransform = playerTransform;
        _characterController = characterController;
        _rigidbody = rigidbody;
        _landingPoint = landingPoint;
        _initialPosition = playerTransform.position;
        _initialRotation = playerTransform.rotation;

        // Calculate target rotation to align with ground
        _targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up), Vector3.up);
    }

    public override void StartTransition()
    {
        base.StartTransition();

        // Disable physics during transition
        _rigidbody.isKinematic = true;
        _characterController.enabled = false;
    }

    protected override void HandleTransitionUpdate()
    {
        // Smoothly interpolate position and rotation
        float t = TransitionCurve(TransitionProgress);
        _playerTransform.position = Vector3.Lerp(_initialPosition, _landingPoint, t);
        _playerTransform.rotation = Quaternion.Slerp(_initialRotation, _targetRotation, t);
    }

    public override bool IsTransitionComplete()
    {
        if (base.IsTransitionComplete())
        {
            // Enable FPS controller and disable zero-G physics
            _characterController.enabled = true;
            _rigidbody.isKinematic = true;
            return true;
        }
        return false;
    }

    // Custom curve for smooth transition
    private float TransitionCurve(float t)
    {
        return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
    }
}