using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class MovementStateManager : MonoBehaviour
{
    [SerializeField] private FPSMovementConfig fpsConfig;
    [SerializeField] private ZeroGMovementConfig zeroGConfig;
    [SerializeField] private ShipMovementConfig shipConfig;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;

    private MovementStateType CurrentStateType { get; set; }

    private readonly Dictionary<MovementStateType, IMovementState> _states = new();
    private IMovementState _currentState;
    private IMovementStateTransition _activeTransition;

    public void InitialiseZeroGState(Rigidbody rb, ZeroGMovementConfig config, CinemachineCamera cinemachineCamera)
    {
        var capability = new ZeroGMovementCapability(rb, config, cinemachineCamera);
        _states[MovementStateType.ZeroG] = new ZeroGState(capability);
    }

    public void InitialiseFPSState(CharacterController controller, FPSMovementConfig config, Transform cameraTrans,
        GameObject player)
    {
        var capability = new FPSMovementCapability(controller, config, cameraTrans, player.transform);
        _states[MovementStateType.FPS] = new FPSState(capability, player);
    }

    public void InitialiseShipState(Rigidbody rb, ShipMovementConfig config)
    {
        var capability = new ShipMovementCapability(rb, config);
         _states[MovementStateType.Ship] = new ShipState(capability);
    }

    public void HandleInput(MovementInputData input)
    {
        _currentState.HandleInput(input);
    }

    private void Start()
    {
        TransitionToState(MovementStateType.ZeroG);
    }

    private void Update()
    {
        if (_activeTransition != null)
        {
            _activeTransition.UpdateTransition();
            if (_activeTransition.IsTransitionComplete())
            {
                _activeTransition = null;
            }
        }
        else
        {
            _currentState?.UpdateState();
        }
    }

    private void TransitionToState(MovementStateType newStateType)
    {
        if (CurrentStateType == newStateType || !_states.TryGetValue(newStateType, out var newState)) return;

        // Create appropriate transition based on states
        var transition = CreateTransition(_currentState, newState, newStateType);

        if (transition is { CanTransition: true })
        {
            _activeTransition = transition;
            _activeTransition.StartTransition();
        }
        else
        {
            // Immediate transition if no smooth transition is needed
            _currentState?.ExitState();
            _currentState = newState;
            _currentState.EnterState();
        }

        CurrentStateType = newStateType;
    }

    private IMovementStateTransition CreateTransition(IMovementState fromState, IMovementState toState, MovementStateType targetType)
    {
        // Create appropriate transition based on state types
        return CurrentStateType switch
        {
            MovementStateType.ZeroG when targetType == MovementStateType.FPS => new ZeroGToFPSTransition(fromState,
                toState, transform, characterController, GetComponent<Rigidbody>(), FindLandingPoint()),
            MovementStateType.FPS when targetType == MovementStateType.ZeroG => new FPSToZeroGTransition(fromState,
                toState, transform, characterController, GetComponent<Rigidbody>(),
                transform.up * 5f) // Initial launch velocity
            ,
            _ => null
        };
    }

    private Vector3 FindLandingPoint()
    {
        // Raycast to find ground point for landing
        if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100f))
        {
            return hit.point + Vector3.up * characterController.height / 2f;
        }
        return transform.position;
    }
}
