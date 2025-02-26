#nullable enable

using System;
using System.Collections;
using Animancer;
using Managers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace NPC
{
    internal enum NpcState
    {
        Idle,
        Wander,
        Follow,
        Interact,
        ObservePlayer,
        Flee,
        Death,
        Falling
    }

    [RequireComponent(typeof(Rigidbody), typeof(Life))]
    public class NpcMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] public Transform planet = null!;
        [SerializeField] public Transform target = null!;
        [SerializeField] private AnimancerComponent animancer = null!;

        [SerializeField] private AnimationClip idleAnimation = null!;
        [SerializeField] private AnimationClip walkAnimation = null!;
        [SerializeField] private AnimationClip interactAnimation = null!;
        [SerializeField] private AnimationClip deathAnimation = null!;
        [SerializeField] private AnimationClip floatingAnimation = null!;
        [SerializeField] private AnimationClip fleeAnimation = null!;

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
        [SerializeField] private Transform? headBone;

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

        [Header("Flee Settings")]
        [SerializeField] private float fleeSpeed = 8f;
        [SerializeField] private float fleeDistance = 15f;
        [SerializeField] private float healthFleeThreshold = 0.3f;
        [SerializeField] private float fleeDuration = 10f;

        [Header("Audio")]
        [SerializeField] private AudioSource moveAudioSource = null!;
        [SerializeField] private AudioClip[] moveClips = null!;
        [SerializeField] private AudioSource interactAudioSource = null!;
        [SerializeField] private AudioClip[] interactClips = null!;
        [SerializeField] private AudioClip[] fleeClips = null!;

        [SerializeField] private float stateCooldown = 1f;

        private Rigidbody _rb = null!;
        private Health _health = null!;
        private Vector3 _surfaceNormal = Vector3.up;
        private bool _isGrounded;
        private Vector3 _wanderTarget;
        private Vector3 _fleeTarget;
        private float _stateTimer;
        private float _originalSpeed;

        [SerializeField] private NpcState currentState = NpcState.Idle;
        private Quaternion _originalHeadRotation;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _originalSpeed = speed;
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

            _health.onHealthChanged.AddListener(OnHealthChanged);
            _health.onDeath.AddListener(OnDeath);

            ChangeState(NpcState.Idle);
        }

        private void OnHealthChanged(float healthValue)
        {
            // Start fleeing if health drops below threshold and not already fleeing
            if (healthValue / _health.MaxHealth < healthFleeThreshold && currentState != NpcState.Flee)
            {
                ChangeState(NpcState.Flee);
            }
        }

        private void OnDeath()
        {
            StopAllCoroutines();

            _rb.isKinematic = true;
            currentState = NpcState.Death;
            animancer.Play(deathAnimation);

            Invoke(nameof(DestroyAfterAnimation), 1.5f);

        }

        private void DestroyAfterAnimation()
        {
            gameObject.SetActive(false);
        }

        private void ChangeState(NpcState newState)
        {
            if (currentState == NpcState.Death) return;

            if (Time.time - _stateTimer < stateCooldown) return;

            _stateTimer = Time.time;

            // Exit current state
            StopAllCoroutines();

            switch (currentState)
            {
                case NpcState.ObservePlayer:
                    if (headBone)
                    {
                        headBone.localRotation = _originalHeadRotation;
                    }
                    break;
                case NpcState.Flee:
                    speed = _originalSpeed;
                    break;
                case NpcState.Idle:
                case NpcState.Wander:
                case NpcState.Follow:
                case NpcState.Death:
                case NpcState.Falling:
                case NpcState.Interact:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Enter new state
            currentState = newState;
            _stateTimer = 0f;

            switch (currentState)
            {
                case NpcState.Idle:
                    StartCoroutine(IdleState());
                    break;
                case NpcState.Wander:
                    StartWandering();
                    break;
                case NpcState.Follow:
                    break;
                case NpcState.Interact:
                    StartCoroutine(InteractState());
                    break;
                case NpcState.ObservePlayer:
                    StartCoroutine(ObservePlayerState());
                    break;
                case NpcState.Flee:
                    StartCoroutine(FleeState());
                    break;
                case NpcState.Death:
                case NpcState.Falling:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region State Implementations

        private IEnumerator IdleState()
        {
            while (true)
            {
                if (target && Vector3.Distance(transform.position, target.position) < followDistance)
                {
                    ChangeState(NpcState.Follow);
                    yield break;
                }

                if (ShouldObservePlayer())
                {
                    ChangeState(NpcState.ObservePlayer);
                    yield break;
                }

                if (Random.value < 0.1f)
                {
                    AudioManager.Instance.PlaySound(interactAudioSource, Utils.RandomElement(interactClips));
                }

                // Decide what to do next
                if (Random.value < 0.7f)
                {
                    ChangeState(NpcState.Wander);
                    yield break;
                }
                    // Check if there are nearby NPCs to interact with
                    var results = new Collider[10];
                    var size = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, results, npcLayer);

                    for (var i = 0; i < size; i++)
                    {
                        var npc = results[i];
                        if (npc.transform == transform) continue;

                        ChangeState(NpcState.Interact);
                        yield break;
                    }

                yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
            }
        }

        private void StartWandering()
        {
            var randomDirection = Random.insideUnitSphere;
            _wanderTarget = transform.position + Vector3.ProjectOnPlane(randomDirection, _surfaceNormal).normalized * wanderRadius;

            if (IsWaterAhead((_wanderTarget - transform.position).normalized))
            {
                _wanderTarget = transform.position + FindSafeDirection((_wanderTarget - transform.position).normalized) * wanderRadius;
            }

            StartCoroutine(WanderState());
        }

        private IEnumerator WanderState()
        {
            while (Vector3.Distance(transform.position, _wanderTarget) > stopDistance)
            {
                if (target && Vector3.Distance(transform.position, target.position) < followDistance
                            && Vector3.Distance(transform.position, target.position) > minLookDistance)
                {
                    ChangeState(NpcState.Follow);
                    yield break;
                }

                if (ShouldObservePlayer())
                {
                    ChangeState(NpcState.ObservePlayer);
                    yield break;
                }

                yield return null;
            }

            ChangeState(NpcState.Idle);
        }

        private IEnumerator InteractState()
        {
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, results, npcLayer);
            Transform? interactionTarget = null;

            for (var i = 0; i < size; i++)
            {
                var npc = results[i];
                if (npc.transform == transform) continue;
                interactionTarget = npc.transform;
                break;
            }

            if (!interactionTarget)
            {
                ChangeState(NpcState.Idle);
                yield break;
            }

            AudioManager.Instance.PlaySound(interactAudioSource, Utils.RandomElement(interactClips));

            var interactTime = Random.Range(3f, 8f);
            var timer = 0f;

            while (timer < interactTime)
            {
                if (!interactionTarget)
                {
                    break;
                }

                var direction = (interactionTarget.position - transform.position).normalized;
                direction = Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;

                var targetRotation = Quaternion.LookRotation(direction, _surfaceNormal);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

                timer += Time.deltaTime;
                yield return null;
            }

            ChangeState(NpcState.Idle);
        }

        private IEnumerator ObservePlayerState()
        {
            while (true)
            {
                var direction = (target.position - transform.position).normalized;
                direction = Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;

                var targetRotation = Quaternion.LookRotation(direction, _surfaceNormal);

                // if target rotation too much, start coroutine to rotate head
                if (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
                {
                    StartCoroutine(RotateToFace(targetRotation));
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }

                if (!ShouldObservePlayer())
                {
                    ChangeState(Vector3.Distance(target.transform.position, transform.position) < followDistance
                        ? NpcState.Follow
                        : NpcState.Idle);

                    yield break;
                }

                if (headBone)
                {

                    var lookDir = target.position - headBone.position;
                    var lookRot = Quaternion.LookRotation(lookDir, transform.up);
                    headBone.rotation = Quaternion.Slerp(headBone.rotation, lookRot, Time.deltaTime * headTrackingSpeed);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator RotateToFace(Quaternion targetRotation)
        {
            while (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }
        }

        private IEnumerator FleeState()
        {
            if (fleeClips.Length > 0)
            {
                AudioManager.Instance.PlaySound(interactAudioSource, Utils.RandomElement(fleeClips));
            }

            speed = fleeSpeed;

            if (target)
            {
                var fleeDirection = (transform.position - target.position).normalized;
                _fleeTarget = transform.position + fleeDirection * fleeDistance;

                if (IsWaterAhead(fleeDirection))
                {
                    var safeDirection = FindSafeDirection(fleeDirection);
                    if (safeDirection != Vector3.zero)
                    {
                        _fleeTarget = transform.position + safeDirection * fleeDistance;
                    }
                }
            }
            else
            {
                var randomDirection = Random.insideUnitSphere;
                randomDirection = Vector3.ProjectOnPlane(randomDirection, _surfaceNormal).normalized;

                if (IsWaterAhead(randomDirection))
                {
                    randomDirection = FindSafeDirection(randomDirection);
                }

                _fleeTarget = transform.position + randomDirection * fleeDistance;
            }

            float fleeTimer = 0;

            while (fleeTimer < fleeDuration)
            {
                if (_health.CurrentHealth / _health.MaxHealth > healthFleeThreshold * 1.5f)
                {
                    ChangeState(NpcState.Idle);
                    yield break;
                }

                if (target && Vector3.Distance(transform.position, target.position) < fleeDistance * 0.5f)
                {
                    var fleeDirection = (transform.position - target.position).normalized;
                    if (IsWaterAhead(fleeDirection))
                    {
                        fleeDirection = FindSafeDirection(fleeDirection);
                    }

                    _fleeTarget = transform.position + fleeDirection * fleeDistance;
                }

                if (Vector3.Distance(transform.position, _fleeTarget) < stopDistance)
                {
                    var randomDirection = Random.insideUnitSphere;
                    randomDirection = Vector3.ProjectOnPlane(randomDirection, _surfaceNormal).normalized;

                    if (IsWaterAhead(randomDirection))
                    {
                        randomDirection = FindSafeDirection(randomDirection);
                    }

                    _fleeTarget = transform.position + randomDirection * fleeDistance;
                }

                fleeTimer += Time.deltaTime;
                yield return null;
            }

            speed = _originalSpeed;
            ChangeState(NpcState.Idle);
        }

        #endregion

        private bool ShouldObservePlayer()
        {
            var distanceToPlayer = Vector3.Distance(transform.position, target.position);
            return distanceToPlayer <= observationDistance;
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

            if (neighborCount <= 0) return Vector3.zero;

            cohesion = (cohesion / neighborCount - transform.position) * cohesionWeight;
            separation *= separationWeight;

            return cohesion + separation;
        }

        private void ValidateComponents()
        {
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();
            ApplyGravity();

            var moveDirection = Vector3.zero;

            switch (currentState)
            {
                case NpcState.Idle:
                    break;

                case NpcState.Wander:
                    moveDirection = CalculateMovementDirection(_wanderTarget);
                    break;

                case NpcState.Follow:
                {

                    var distanceToTarget = Vector3.Distance(transform.position, target.position);
                    if (distanceToTarget <= stopDistance)
                    {
                        moveDirection = CalculateMovementDirection(transform.position + (transform.position - target.position));
                    } else if (distanceToTarget <= minLookDistance)
                    {
                        ChangeState(NpcState.ObservePlayer);
                    } else if (distanceToTarget > followDistance)
                    {
                        ChangeState(NpcState.Idle);
                    }
                    else
                    {
                        moveDirection = CalculateMovementDirection(target.position);
                    }
                }
                    break;

                case NpcState.Flee:
                    moveDirection = CalculateMovementDirection(_fleeTarget);
                    break;

                case NpcState.Interact:
                case NpcState.ObservePlayer:
                {
                    var distanceToTarget = Vector3.Distance(transform.position, target.position);

                    if (distanceToTarget <= minLookDistance)
                    {
                        moveDirection =
                            CalculateMovementDirection(transform.position + (transform.position - target.position));
                    }
                    else if (distanceToTarget > followDistance)
                    {
                        ChangeState(NpcState.Idle);
                    }
                }

                    break;
                case NpcState.Death:
                    return;
                case NpcState.Falling:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (currentState != NpcState.Flee && currentState != NpcState.Death && currentState != NpcState.Falling)
            {
                var groupInfluence = CalculateGroupBehavior();
                moveDirection += groupInfluence;
                if (moveDirection.magnitude > 0)
                {
                    moveDirection.Normalize();
                }
            }

            HandleMovement(moveDirection);
            UpdateAnimator();
        }

        private Vector3 CalculateMovementDirection(Vector3 destination)
        {
            var direction = destination - transform.position;
            return Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;
        }

        private void UpdateGroundedState()
        {
            var direction = -(transform.position - planet.position).normalized;

            var origin = transform.position + _surfaceNormal * 0.1f;
            if (Physics.Raycast(origin, direction, out var hit, 5f))
            {
                _surfaceNormal = hit.normal;
                if (_isGrounded) return;

                _isGrounded = true;
                ChangeState(NpcState.Idle);
            } else
            {
                _isGrounded = false;
                if (currentState != NpcState.Falling)
                {
                    ChangeState(NpcState.Falling);
                }
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
                return;
            }

            if (IsWaterAhead(moveDirection))
            {
                moveDirection = FindSafeDirection(moveDirection);

                if (moveDirection == Vector3.zero)
                {
                    _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, Time.deltaTime * 5f);
                    return;
                }
            }

            var targetRotation = Quaternion.LookRotation(moveDirection, _surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            _rb.MovePosition(_rb.position + moveDirection * (speed * Time.deltaTime));

            if (!moveAudioSource.isPlaying)
            {
                AudioManager.Instance.PlaySound(moveAudioSource, Utils.RandomElement(moveClips));
            }
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
            var isMoving = _rb.linearVelocity.magnitude > 0.1f;

            switch (currentState)
            {
                case NpcState.Interact:
                        animancer.Play(interactAnimation);
                    break;
                case NpcState.Flee:
                        animancer.Play(fleeAnimation);
                    break;
                case NpcState.Falling:
                        animancer.Play(floatingAnimation);
                    break;
                case NpcState.Death:
                    animancer.Play(deathAnimation);
                    break;
                case NpcState.Follow:
                    animancer.Play(walkAnimation);
                    break;
                case NpcState.Idle:
                case NpcState.ObservePlayer:
                    animancer.Play(idleAnimation);
                    break;
                case NpcState.Wander:
                    animancer.Play(walkAnimation);
                    break;
                default:
                    var s = animancer.Play(isMoving ? walkAnimation : idleAnimation);
                    break;
            }
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

            switch (currentState)
            {
                case NpcState.Wander:
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, _wanderTarget);
                    break;
                case NpcState.Flee:
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, _fleeTarget);
                    Gizmos.DrawWireSphere(transform.position, fleeDistance);
                    break;
                case NpcState.Idle:
                case NpcState.Follow:
                case NpcState.Interact:
                case NpcState.ObservePlayer:
                case NpcState.Death:
                    break;
                case NpcState.Falling:
                {
                    var direction = (planet.position - transform.position).normalized;
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(transform.position, direction * groundCheckDistance);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}