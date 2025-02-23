using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace NPC
{

internal enum NpcState
{
    Idle,
    Wander,
    Follow,
    Interact,
    ObservePlayer,
    Flee
}


[RequireComponent(typeof(Rigidbody), typeof(Life))]
public class NpcMovement : MonoBehaviour
{

    [Header("References")]
    [SerializeField] public Transform planet;
    [SerializeField] public Transform target;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float followDistance = 10f;
    [SerializeField] private float stopDistance = 2f;

    [Header("Player Observation")]
    [SerializeField] private float observationDistance = 8f;
    [SerializeField] private float minObservationTime = 3f;
    [SerializeField] private float maxObservationTime = 8f;
    [SerializeField] private float minLookDistance = 4f;
    [SerializeField] private float headTrackingSpeed = 3f;
    [SerializeField] private Transform headBone;

    [Header("Group Behavior")]
    [SerializeField] private float groupRadius = 5f;
    [SerializeField] private float separationDistance = 2f;
    [SerializeField] private float cohesionWeight = 0.5f;
    [SerializeField] private float separationWeight = 1f;
    [SerializeField] private LayerMask npcLayer;

    [Header("Idle Behavior")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minIdleTime = 5f;
    [SerializeField] private float maxIdleTime = 15f;
    [SerializeField] private float interactionRadius = 3f;

    [Header("Physics Settings")]
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private float uprightSpeed = 5f;

    [Header("Avoidance Settings")]
    [SerializeField] private float waterCheckDistance = 2f;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private float waterAvoidanceAngle = 45f;

    private Rigidbody _rb;
    private Vector3 _surfaceNormal = Vector3.up;
    private Vector3 _wanderTarget;
    private bool _isMoving;
    private bool _isWandering;
    private bool _isInteracting;
    private bool _isObservingPlayer;
    private NpcState _currentState;
    private Quaternion _originalHeadRotation;

    private static readonly int IsMoving = Animator.StringToHash("Walking");
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Interact = Animator.StringToHash("Interact");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();
        if (headBone)
        {
            _originalHeadRotation = headBone.localRotation;
        }
    }

    private void ConfigureRigidbody()
    {
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rb.useGravity = false;
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        ValidateComponents();
        waterLayer = LayerMask.GetMask("Water", "Rock");
        npcLayer = LayerMask.GetMask("NPC");
        var health = GetComponent<Health>();
        health.onHealthChanged.AddListener(OnHealthChanged);
        health.onDeath.AddListener(OnDeath);

        StartCoroutine(IdleBehavior());
    }

    private void OnHealthChanged(float dt)
    {
        // make flee probably if health is low
        // TDOD
    }

    private void OnDeath()
    {
        // probably play death animation and enemy pool and stuff
        Destroy(gameObject);
    }

    private bool ShouldObservePlayer()
    {
        if (!target) return false;

        var distanceToPlayer = Vector3.Distance(transform.position, target.position);
        return distanceToPlayer <= observationDistance && distanceToPlayer > minLookDistance;
    }

    private void UpdatePlayerObservation()
    {
        if (!ShouldObservePlayer())
        {
            if (_isObservingPlayer)
            {
                StopObservingPlayer();
            }

            return;
        }

        if (!_isObservingPlayer)
        {
            StartObservingPlayer();
        }

        if (!headBone) return;

        var directionToPlayer = target.position - headBone.position;
        var targetRotation = Quaternion.LookRotation(directionToPlayer, transform.up);
        headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, Time.deltaTime * headTrackingSpeed);
    }

    private void StartObservingPlayer()
    {
        _isObservingPlayer = true;
        _isWandering = false;
        _isInteracting = false;

        var directionToPlayer = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(directionToPlayer, _surfaceNormal), _surfaceNormal);

        // animator.SetBool(ObservePlayer, true);
    }

    private void StopObservingPlayer()
    {
        _isObservingPlayer = false;
        // animator.SetBool(ObservePlayer, false);

        if (headBone)
        {
            headBone.localRotation = _originalHeadRotation;
        }
    }

    private IEnumerator IdleBehavior()
    {
        while (true)
        {
            if (!_isObservingPlayer && (!target || Vector3.Distance(transform.position, target.position) > followDistance))
            {
                if (Random.value < 0.7f)
                {
                    StartWandering();
                }
                else
                {
                    TryInteractWithNpc();
                }

                yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void StartWandering()
    {
        _isWandering = true;
        _isInteracting = false;

        var randomDirection = Random.insideUnitSphere;
        _wanderTarget = transform.position + Vector3.ProjectOnPlane(randomDirection, _surfaceNormal).normalized * wanderRadius;

        if (IsWaterAhead((_wanderTarget - transform.position).normalized))
        {
            _wanderTarget = FindSafeDirection((_wanderTarget - transform.position).normalized) * wanderRadius;
        }
    }

    private void TryInteractWithNpc()
    {
        var results = new Collider[10];
        var size = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, results, npcLayer);
        for (var i = 0; i < size; i++)
        {
            var npc = results[i];

            if (npc.transform == transform) continue;
            _isInteracting = true;
            _isWandering = false;

            var direction = (npc.transform.position - transform.position).normalized;
            var targetRotation = Quaternion.LookRotation(direction, _surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            return;
        }
    }

    private Vector3 CalculateGroupBehavior()
    {
        var cohesion = Vector3.zero;
        var separation = Vector3.zero;
        var neighborCount = 0;

        var results = new Collider[10];
        var size = Physics.OverlapSphereNonAlloc(transform.position, groupRadius, results, npcLayer);

        for (var i = 0; i < size; i++)
        {
            var neighbor = results[i];

            if (neighbor.gameObject == gameObject) continue;

            var directionToNeighbor = neighbor.transform.position - transform.position;
            var distance = directionToNeighbor.magnitude;

            cohesion += neighbor.transform.position;

            if (distance < separationDistance)
            {
                separation += -directionToNeighbor.normalized / distance;
            }

            neighborCount++;
        }

        if (neighborCount <= 0) return cohesion + separation;

        cohesion = (cohesion / neighborCount - transform.position) * cohesionWeight;
        separation *= separationWeight;

        return cohesion + separation;
    }

    private void ValidateComponents()
    {
        Assert.IsNotNull(animator, "Animator is missing");
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!planet) return;

        UpdateGroundedState();
        UpdatePlayerObservation();

        if (_isInteracting || _isObservingPlayer)
        {
            _rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 moveDirection;
        var distanceToTarget = target ? Vector3.Distance(transform.position, target.position) : float.MaxValue;

        if (distanceToTarget <= minLookDistance)
        {
            _isWandering = false;
            moveDirection = CalculateMovementDirection(transform.position + (transform.position - target.position));
        }
        else if (target && Vector3.Distance(transform.position, target.position) < followDistance)
        {
            moveDirection = CalculateMovementDirection(target.position);
            _isWandering = false;
        }
        else if (_isWandering)
        {
            moveDirection = CalculateMovementDirection(_wanderTarget);

            if (Vector3.Distance(transform.position, _wanderTarget) < stopDistance)
            {
                _isWandering = false;
            }
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        Vector3 groupInfluence = CalculateGroupBehavior();
        moveDirection += groupInfluence;
        moveDirection.Normalize();

        HandleMovement(moveDirection);
        ApplyGravity();
        UpdateAnimator();
    }

    private Vector3 CalculateMovementDirection(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        return Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;
    }

    private void UpdateGroundedState()
    {
        var direction = (planet.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, direction, out var hit, 20f))
        {
            _surfaceNormal = hit.normal;
        }
    }

    private bool IsWaterAhead(Vector3 moveDirection)
    {
        if (Physics.Raycast(transform.position, moveDirection, waterCheckDistance, waterLayer))
        {
            return true;
        }

        var left = Quaternion.Euler(0, -30, 0) * moveDirection;
        var right = Quaternion.Euler(0, 30, 0) * moveDirection;

        return Physics.Raycast(transform.position, left, waterCheckDistance, waterLayer) ||
               Physics.Raycast(transform.position, right, waterCheckDistance, waterLayer);
    }

    private Vector3 FindSafeDirection(Vector3 originalDirection)
    {
        for (float angle = 0; angle <= waterAvoidanceAngle; angle += 5)
        {
            var left = Quaternion.Euler(0, -angle, 0) * originalDirection;
            if (!IsWaterAhead(left))
            {
                return left;
            }

            var right = Quaternion.Euler(0, angle, 0) * originalDirection;

            if (!IsWaterAhead(right))
            {
                return right;
            }
        }

        return Vector3.zero;
    }

    private void HandleMovement(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero)
        {
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
            _isMoving = false;
            return;
        }

        if (IsWaterAhead(moveDirection))
        {
            moveDirection = FindSafeDirection(moveDirection);

            if (moveDirection == Vector3.zero)
            {
                _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
                _isMoving = false;
                return;
            }
        }

        var targetRotation = Quaternion.LookRotation(moveDirection, _surfaceNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        _rb.linearVelocity = moveDirection * speed;
        _isMoving = true;
    }

    private void ApplyGravity()
    {
        var gravityDir = -(transform.position - planet.position).normalized;
        _rb.AddForce(gravityDir * (Physics.gravity.magnitude * gravityMultiplier), ForceMode.Acceleration);

        var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * uprightSpeed);
    }

    private void UpdateAnimator()
    {
        animator.Play(_isMoving ? IsMoving : Idle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (!Application.isPlaying) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * waterCheckDistance);

        var leftDirection = Quaternion.Euler(0, -30, 0) * transform.forward;
        var rightDirection = Quaternion.Euler(0, 30, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDirection * waterCheckDistance);
        Gizmos.DrawRay(transform.position, rightDirection * waterCheckDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, groupRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        if (_isWandering && Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, _wanderTarget);
        }
    }
}
}
