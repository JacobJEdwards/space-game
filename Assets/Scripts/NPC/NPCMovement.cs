#nullable enable

using System;
using System.Collections;
using Animancer;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NPC
{
    internal enum NpcState
    {
        Idle,
        Wander,
        Follow,
        Interact,
        Attacking,
        ObservePlayer,
        Flee,
        Death,
        Falling
    }

    [RequireComponent(typeof(Rigidbody), typeof(Life), typeof(Health))]
    public class NpcMovement : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        public Transform player = null!;

        [SerializeField] private AnimancerComponent animancer = null!;
        [SerializeField] private AudioSource moveAudioSource = null!;

        [SerializeField] private AudioSource interactAudioSource = null!;

        [Header("Animations")] [SerializeField]
        private ClipTransition idleAnimation = null!;

        [SerializeField] private ClipTransition walkAnimation = null!;
        [SerializeField] private ClipTransition interactAnimation = null!;
        [SerializeField] private ClipTransition deathAnimation = null!;
        [SerializeField] private ClipTransition floatingAnimation = null!;
        [SerializeField] private ClipTransition fleeAnimation = null!;
        [SerializeField] private ClipTransition attackAnimation = null!;

        [Header("General Settings")] [SerializeField]
        private float speed = 5f;

        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float stateCooldown = 0.5f;
        [SerializeField] private NpcState currentState = NpcState.Idle;

        [Header("Movement Behavior")] [SerializeField]
        private float followDistance = 10f;

        [SerializeField] private float stopDistance = 2f;
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float minIdleTime = 5f;
        [SerializeField] private float maxIdleTime = 15f;

        [Header("Player Observation")] [SerializeField]
        private float observationDistance = 8f;

        [SerializeField] private float minObservationTime = 3f;
        [SerializeField] private float maxObservationTime = 8f;
        [SerializeField] private float minLookDistance = 4f;
        [SerializeField] private float headTrackingSpeed = 3f;
        [SerializeField] private Transform? headBone;

        [Header("Group Behavior")] [SerializeField]
        private float groupRadius = 5f;

        [SerializeField] private float separationDistance = 2f;
        [SerializeField] private float cohesionWeight = 0.5f;
        [SerializeField] private float separationWeight = 1f;
        [SerializeField] private LayerMask npcLayer;

        [Header("Physics Settings")] [SerializeField]
        private float gravityMultiplier = 2f;

        [SerializeField] private float groundCheckDistance = 20f;
        [SerializeField] private float uprightSpeed = 5f;

        [Header("Avoidance Settings")] [SerializeField]
        private float waterCheckDistance = 2f;

        [SerializeField] private LayerMask waterLayer;
        [SerializeField] private float waterAvoidanceAngle = 45f;

        [Header("Flee Settings")] [SerializeField]
        private float fleeSpeed = 8f;

        [SerializeField] private float fleeDistance = 15f;
        [SerializeField] private float healthFleeThreshold = 0.3f;
        [SerializeField] private float fleeDuration = 10f;

        [Header("Enemy Settings")] [SerializeField]
        private bool isEnemy;

        [SerializeField] private float aggroRange = 12f;
        [SerializeField] private float attackRange = 2.5f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private float attackDamage = 10f;

        [SerializeField] private LayerMask targetLayerMask;

        [Header("Audio Clips")] [SerializeField]
        private AudioClip[] moveClips = null!;

        [SerializeField] private AudioClip[] interactClips = null!;
        [SerializeField] private AudioClip[] fleeClips = null!;
        [SerializeField] private AudioClip[] attackClips = null!;
        private float _attackTimer;
        private Transform? _currentTarget;
        private Vector3 _fleeTarget;
        private Health _health = null!;
        private bool _isGrounded;
        private Life _life = null!;
        private Quaternion _originalHeadRotation;
        private float _originalSpeed;

        private Rigidbody _rb = null!;
        private float _stateTimer;
        private Vector3 _surfaceNormal = Vector3.up;
        private Vector3 _wanderTarget;
        private float _wanderTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _life = GetComponent<Life>();
            _originalSpeed = speed;

            ConfigureRigidbody();
            if (headBone) _originalHeadRotation = headBone.localRotation;
        }

        private void Start()
        {
            waterLayer = LayerMask.GetMask("Water", "Rock", "Ship");
            npcLayer = LayerMask.GetMask("NPC", "Ship");

            if (_health)
            {
                _health.onHealthChanged.AddListener(OnHealthChanged);
                _health.onDeath.AddListener(OnDeath);
            }
            else
            {
                Debug.LogError("Health component not found!", this);
            }

            if (!_life) Debug.LogError("Life component not found!", this);

            if (!player && isEnemy)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject) player = playerObject.transform;
                else
                    Debug.LogWarning(
                        "Player transform not assigned and could not be found by tag 'Player'. Enemy AI might not function correctly.",
                        this);
            }


            ChangeState(NpcState.Idle, true);
        }

        private void Update()
        {
            _stateTimer += Time.deltaTime;
            _attackTimer += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (currentState == NpcState.Death) return;

            UpdateGroundedState();
            if (!_isGrounded) return;

            var moveDirection = Vector3.zero;
            var applyGroupBehavior = true;

            switch (currentState)
            {
                case NpcState.Idle:
                case NpcState.Interact:
                case NpcState.ObservePlayer:
                    if (isEnemy && !_currentTarget && FindTarget())
                    {
                        ChangeState(NpcState.Attacking);
                        return;
                    }

                    if (currentState is NpcState.ObservePlayer or NpcState.Interact)
                        if (player && Vector3.Distance(transform.position, player.position) <= minLookDistance)
                            moveDirection = CalculateAvoidanceDirection(player.position);

                    break;

                case NpcState.Wander:
                    moveDirection = CalculateMovementDirection(_wanderTarget);
                    if (isEnemy && !_currentTarget && FindTarget()) ChangeState(NpcState.Attacking);
                    break;

                case NpcState.Follow:
                    if (!player)
                    {
                        ChangeState(NpcState.Idle);
                        return;
                    }

                    var distanceToPlayer = Vector3.Distance(transform.position, player.position);
                    if (distanceToPlayer <= stopDistance) moveDirection = Vector3.zero;
                    else if (distanceToPlayer > followDistance) ChangeState(NpcState.Idle);
                    else moveDirection = CalculateMovementDirection(player.position);

                    if (isEnemy && !_currentTarget && FindTarget()) ChangeState(NpcState.Attacking);
                    break;

                case NpcState.Attacking:
                    applyGroupBehavior = false;
                    if (!_currentTarget || !_currentTarget.gameObject.activeInHierarchy)
                    {
                        ChangeState(NpcState.Idle);
                        return;
                    }

                    var distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);
                    moveDirection = distanceToTarget > attackRange
                        ? CalculateMovementDirection(_currentTarget.position)
                        : Vector3.zero;

                    break;

                case NpcState.Flee:
                    moveDirection = CalculateMovementDirection(_fleeTarget);
                    applyGroupBehavior = false;
                    break;

                case NpcState.Falling:
                case NpcState.Death:
                    applyGroupBehavior = false;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (applyGroupBehavior)
            {
                var groupInfluence = CalculateGroupBehavior();
                if (moveDirection.magnitude > 0.01f || groupInfluence.magnitude > 0.01f)
                    moveDirection = (moveDirection + groupInfluence).normalized;
            }

            HandleMovement(moveDirection);
            UpdateAnimator(moveDirection.magnitude > 0.1f);
        }


        private void OnDrawGizmosSelected()
        {
            // Existing Gizmos...
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, followDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, observationDistance);

            if (isEnemy)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, aggroRange);
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, attackRange);
            }

            if (!Application.isPlaying) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f,
                transform.forward * waterCheckDistance);

            var leftDirection = Quaternion.Euler(0, -30, 0) * transform.forward;
            var rightDirection = Quaternion.Euler(0, 30, 0) * transform.forward;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, leftDirection * waterCheckDistance);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, rightDirection * waterCheckDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, groupRadius);

            Gizmos.color = Color.cyan;

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
                case NpcState.Attacking:
                    if (_currentTarget)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(transform.position, _currentTarget.position);
                    }

                    break;
                case NpcState.Idle:
                case NpcState.Follow:
                case NpcState.Interact:
                case NpcState.ObservePlayer:
                case NpcState.Death:
                case NpcState.Falling:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ConfigureRigidbody()
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.useGravity = false;
            _rb.freezeRotation = true;
        }

        private void OnHealthChanged(float currentHealth)
        {
            if (currentHealth / _health.MaxHealth < healthFleeThreshold && currentState != NpcState.Flee &&
                currentState != NpcState.Death) ChangeState(NpcState.Flee);
        }

        private void OnDeath()
        {
            if (currentState == NpcState.Death) return;
            ChangeState(NpcState.Death, true);
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            StopAllCoroutines();

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            animancer.Play(deathAnimation);
            var destroyDelay = deathAnimation.Clip
                ? deathAnimation.Clip.length
                : 1.5f;
            Invoke(nameof(CleanupAfterDeath), destroyDelay);
        }

        private void CleanupAfterDeath()
        {
            gameObject.SetActive(false);
        }

        private void ChangeState(NpcState newState, bool force = false)
        {
            if (!force && currentState == newState) return;
            if (currentState == NpcState.Death && !force)
                return;
            if (!force && _stateTimer < stateCooldown) return;

            switch (currentState)
            {
                case NpcState.ObservePlayer:
                    if (headBone) headBone.localRotation = _originalHeadRotation;
                    StopCoroutine(nameof(ObservePlayerState));
                    StopCoroutine(nameof(RotateToFace));
                    break;
                case NpcState.Flee:
                    speed = _originalSpeed;
                    StopCoroutine(nameof(FleeState));
                    break;
                case NpcState.Attacking:
                    _currentTarget = null;
                    StopCoroutine(nameof(AttackState));
                    break;
                case NpcState.Wander:
                    StopCoroutine(nameof(WanderState));
                    break;
                case NpcState.Idle:
                    StopCoroutine(nameof(IdleState));
                    break;
                case NpcState.Interact:
                    StopCoroutine(nameof(InteractState));
                    break;
                case NpcState.Follow:
                case NpcState.Death:
                case NpcState.Falling:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
                case NpcState.Attacking:
                    if (_currentTarget) StartCoroutine(AttackState());
                    else ChangeState(NpcState.Idle);
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

            UpdateAnimator(false);
        }


        private bool FindTarget()
        {
            if (!isEnemy || !player) return false;

            var distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= aggroRange)
            {
                _currentTarget = player;
                return true;
            }

            Transform? closestNpcTarget = null;
            var minDistanceSqr = aggroRange * aggroRange;

            Collider[] hits = Physics.OverlapSphere(transform.position, aggroRange, targetLayerMask);

            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;

                if (hit.TryGetComponent<NpcMovement>(out var otherNpc) && !otherNpc.isEnemy &&
                    hit.TryGetComponent<Health>(out _))
                {
                    var distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                    if (!(distSqr < minDistanceSqr)) continue;

                    minDistanceSqr = distSqr;
                    closestNpcTarget = hit.transform;
                }
                else if (hit.transform == player)
                {
                    var distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                    if (!(distSqr < minDistanceSqr)) continue;

                    minDistanceSqr = distSqr;
                    closestNpcTarget = hit.transform;
                }
            }

            _currentTarget = closestNpcTarget;
            return _currentTarget;
        }

        private bool HasLineOfSight(Vector3 targetPosition)
        {
            var direction =
                (targetPosition - (transform.position + Vector3.up * 0.5f))
                .normalized;
            var distance = Vector3.Distance(transform.position, targetPosition);

            LayerMask sightBlockingLayers =
                LayerMask.GetMask("Default", "PlanetSurface", "Water", "Rock", "Ship");

            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, out var hit, distance,
                    sightBlockingLayers)) return true;

            return !(Vector3.Distance(hit.point, targetPosition) > 1.0f);
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

                if (distance > 0 && distance < separationDistance)
                    separation -= directionToNeighbor.normalized / distance;

                neighborCount++;
            }

            if (neighborCount == 0) return Vector3.zero;

            cohesion = cohesion / neighborCount - transform.position;
            cohesion = Vector3.ProjectOnPlane(cohesion, _surfaceNormal).normalized * cohesionWeight;
            separation = Vector3.ProjectOnPlane(separation, _surfaceNormal).normalized * separationWeight;

            return cohesion + separation;
        }

        private Vector3 CalculateMovementDirection(Vector3 destination)
        {
            var direction = destination - transform.position;
            direction.y = 0;
            return Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;
        }

        private Vector3 CalculateAvoidanceDirection(Vector3 positionToAvoid)
        {
            var direction = transform.position - positionToAvoid;
            direction.y = 0;
            return Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;
        }


        private void UpdateGroundedState()
        {
            if (!_life)
            {
                Debug.LogError("Life component is null in UpdateGroundedState.", this);
                _isGrounded = false;
                if (currentState != NpcState.Falling) ChangeState(NpcState.Falling);
                return;
            }

            var currentlyGrounded = _life.isGrounded;
            _surfaceNormal = _life.surfaceNormal;

            switch (_isGrounded)
            {
                case false when currentlyGrounded:
                {
                    _isGrounded = true;
                    if (currentState == NpcState.Falling) ChangeState(NpcState.Idle);
                    break;
                }
                case true when !currentlyGrounded:
                {
                    _isGrounded = false;
                    if (currentState != NpcState.Death) ChangeState(NpcState.Falling);
                    break;
                }
            }
        }


        private bool IsWaterAhead(Vector3 moveDirection)
        {
            var origin = transform.position + Vector3.up * 0.1f;
            var checkDist = waterCheckDistance;

            if (Physics.Raycast(origin, moveDirection, checkDist, waterLayer)) return true;

            var leftRot = Quaternion.Euler(0, -waterAvoidanceAngle * 0.5f, 0);
            var rightRot = Quaternion.Euler(0, waterAvoidanceAngle * 0.5f, 0);

            return Physics.Raycast(origin, leftRot * moveDirection, checkDist, waterLayer) || Physics.Raycast(origin, rightRot * moveDirection, checkDist, waterLayer);
        }

        private Vector3 FindSafeDirection(Vector3 originalDirection)
        {
            const float checkAngle = 5f;
            const float maxCheck = 180f;

            for (var angle = checkAngle; angle <= maxCheck; angle += checkAngle)
            {
                var leftRot = Quaternion.Euler(0, -angle, 0);
                var leftDir = leftRot * originalDirection;
                if (!IsWaterAhead(leftDir)) return leftDir.normalized;

                var rightRot = Quaternion.Euler(0, angle, 0);
                var rightDir = rightRot * originalDirection;
                if (!IsWaterAhead(rightDir)) return rightDir.normalized;
            }

            return -originalDirection.normalized;
        }

        private void HandleMovement(Vector3 moveDirection)
        {
            if (moveDirection.magnitude < 0.01f)
            {
                _rb.linearVelocity =
                    Vector3.Lerp(_rb.linearVelocity, Vector3.zero,
                        Time.deltaTime * 10f);
                return;
            }

            var effectiveDirection = moveDirection.normalized;

            if (IsWaterAhead(effectiveDirection))
            {
                effectiveDirection = FindSafeDirection(effectiveDirection);
                if (effectiveDirection.magnitude < 0.01f)
                {
                    _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, Vector3.zero, Time.deltaTime * 10f);
                    return;
                }
            }

            var targetRotation = Quaternion.LookRotation(effectiveDirection, _surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            var targetVelocity = effectiveDirection * speed;
            var currentVelocity = _rb.linearVelocity;
            var velocityChange = targetVelocity - currentVelocity;
            velocityChange.y = 0;

            _rb.AddForce(velocityChange, ForceMode.VelocityChange);

            if (moveAudioSource.isPlaying || moveClips.Length <= 0) return;

            moveAudioSource.pitch = Random.Range(0.9f, 1.1f);
            AudioManager.Instance?.PlaySound(moveAudioSource, Utils.RandomElement(moveClips));
        }

        private void UpdateAnimator(bool isMoving)
        {
            switch (currentState)
            {
                case NpcState.Idle:
                case NpcState.ObservePlayer:
                    animancer.Play(idleAnimation);
                    break;
                case NpcState.Wander:
                case NpcState.Follow:
                    animancer.Play(isMoving
                        ? walkAnimation
                        : idleAnimation);
                    break;
                case NpcState.Attacking:
                    animancer.Play(attackAnimation);
                    break;
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
                    break;
                default:
                    animancer.Play(idleAnimation);
                    break;
            }
        }


        private IEnumerator IdleState()
        {
            while (currentState == NpcState.Idle)
            {
                if (isEnemy && FindTarget())
                {
                    ChangeState(NpcState.Attacking);
                    yield break;
                }

                switch (isEnemy)
                {
                    case false when player &&
                                    Vector3.Distance(transform.position, player.position) < followDistance:
                        ChangeState(NpcState.Follow);
                        yield break;
                    case false when ShouldObservePlayer():
                        ChangeState(NpcState.ObservePlayer);
                        yield break;
                }

                var waitTime = Random.Range(minIdleTime, maxIdleTime);
                var timer = 0f;
                while (timer < waitTime)
                {
                    if (isEnemy && FindTarget())
                    {
                        ChangeState(NpcState.Attacking);
                        yield break;
                    }

                    switch (isEnemy)
                    {
                        case false when player &&
                                        Vector3.Distance(transform.position, player.position) < followDistance:
                            ChangeState(NpcState.Follow);
                            yield break;
                        case false when ShouldObservePlayer():
                            ChangeState(NpcState.ObservePlayer);
                            yield break;
                        default:
                            timer += Time.deltaTime;
                            yield return null;
                            break;
                    }
                }


                var actionRoll = Random.value;
                switch (actionRoll)
                {
                    case < 0.6f:
                        ChangeState(NpcState.Wander);
                        yield break;
                    case < 0.8f:
                    {
                        var results = new Collider[5];
                        var size = Physics.OverlapSphereNonAlloc(transform.position, 20f, results, npcLayer);
                        for (var i = 0; i < size; i++)
                            if (results[i].transform != transform &&
                                results[i].TryGetComponent<NpcMovement>(out _))
                            {
                                ChangeState(NpcState.Interact);
                                yield break;
                            }

                        break;
                    }
                }

                if (Random.value < 0.1f && interactClips.Length > 0)
                    AudioManager.Instance?.PlaySound(interactAudioSource, Utils.RandomElement(interactClips));

                yield return null;
            }
        }

        private void StartWandering()
        {
            var randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0;
            var targetPos = transform.position +
                            randomDirection.normalized * Random.Range(wanderRadius * 0.5f, wanderRadius);

            targetPos = transform.position +
                        Vector3.ProjectOnPlane(targetPos - transform.position, _surfaceNormal).normalized *
                        Random.Range(wanderRadius * 0.5f, wanderRadius);


            var directionToTarget = (targetPos - transform.position).normalized;
            if (IsWaterAhead(directionToTarget))
            {
                var safeDirection = FindSafeDirection(directionToTarget);
                _wanderTarget = transform.position + safeDirection * Random.Range(wanderRadius * 0.5f, wanderRadius);
            }
            else
            {
                _wanderTarget = targetPos;
            }

            _wanderTimer = 0f;
            StartCoroutine(WanderState());
        }

        private IEnumerator WanderState()
        {
            const float maxWanderDuration = 15f;

            while (currentState == NpcState.Wander)
            {
                if (Vector3.Distance(transform.position, _wanderTarget) <= stopDistance)
                {
                    ChangeState(NpcState.Idle);
                    yield break;
                }

                _wanderTimer += Time.deltaTime;
                if (_wanderTimer > maxWanderDuration)
                {
                    ChangeState(NpcState.Idle);
                    yield break;
                }

                if (isEnemy && FindTarget())
                {
                    ChangeState(NpcState.Attacking);
                    yield break;
                }

                switch (isEnemy)
                {
                    case false when player &&
                                    Vector3.Distance(transform.position, player.position) < followDistance:
                        ChangeState(NpcState.Follow);
                        yield break;
                    case false when ShouldObservePlayer():
                        ChangeState(NpcState.ObservePlayer);
                        yield break;
                    default:
                        yield return null;
                        break;
                }
            }
        }

        private IEnumerator InteractState()
        {
            Transform? interactionTarget = null;
            var results = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position, 20f, results, npcLayer);
            for (var i = 0; i < size; i++)
                if (results[i].transform != transform &&
                    results[i].TryGetComponent<NpcMovement>(out _))
                {
                    interactionTarget = results[i].transform;
                    break;
                }

            if (!interactionTarget)
            {
                ChangeState(NpcState.Idle);
                yield break;
            }

            animancer.Play(interactAnimation);
            if (interactClips.Length > 0)
                AudioManager.Instance?.PlaySound(interactAudioSource, Utils.RandomElement(interactClips));


            var interactTime = Random.Range(3f, 8f);
            var timer = 0f;

            while (timer < interactTime && currentState == NpcState.Interact)
            {
                if (!interactionTarget || !interactionTarget.gameObject.activeInHierarchy) break;

                var direction = interactionTarget.position - transform.position;
                RotateTowards(direction);

                if (isEnemy && FindTarget() &&
                    _currentTarget != interactionTarget)
                {
                    ChangeState(NpcState.Attacking);
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (currentState == NpcState.Interact) ChangeState(NpcState.Idle);
        }

        private IEnumerator ObservePlayerState()
        {
            if (!player)
            {
                ChangeState(NpcState.Idle);
                yield break;
            }

            var observationTimer = 0f;
            var maxObserveDuration = Random.Range(minObservationTime, maxObservationTime);

            while (currentState == NpcState.ObservePlayer)
            {
                var distanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (distanceToPlayer > observationDistance * 1.2f ||
                    observationTimer > maxObserveDuration)
                {
                    ChangeState(NpcState.Idle);
                    yield break;
                }

                if (distanceToPlayer > observationDistance && distanceToPlayer < followDistance)
                {
                    ChangeState(NpcState.Follow);
                    yield break;
                }


                var directionToPlayer = player.position - transform.position;
                RotateTowards(directionToPlayer);


                if (headBone)
                {
                    var lookRot =
                        Quaternion.LookRotation(player.position - headBone.position,
                            transform.up);

                    headBone.rotation =
                        Quaternion.Slerp(headBone.rotation, lookRot, Time.deltaTime * headTrackingSpeed);
                }

                observationTimer += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator AttackState()
        {
            if (!_currentTarget)
            {
                ChangeState(NpcState.Idle);
                yield break;
            }

            var targetHealth = _currentTarget.GetComponent<Health>();
            if (!targetHealth)
            {
                Debug.LogWarning($"Target {_currentTarget.name} does not have a Health component. Cannot attack.",
                    this);
                ChangeState(NpcState.Idle);
                yield break;
            }

            _attackTimer = attackCooldown;

            while (currentState == NpcState.Attacking)
            {
                if (!_currentTarget ||
                    !_currentTarget.gameObject.activeInHierarchy)
                {
                    _currentTarget = null;
                    if (!FindTarget()) ChangeState(NpcState.Idle);
                    yield break;
                }

                var distanceToTarget = Vector3.Distance(transform.position, _currentTarget.position);

                RotateTowards(_currentTarget.position - transform.position);

                if (distanceToTarget <= attackRange)
                    if (_attackTimer >= attackCooldown)
                    {
                        _attackTimer = 0f;

                        if (attackClips.Length > 0)
                            AudioManager.Instance?.PlaySound(interactAudioSource, Utils.RandomElement(attackClips));


                        var damageDelay =
                            attackAnimation.Clip
                                ? attackAnimation.Clip.length * 0.5f
                                : 0.3f;
                        StartCoroutine(ApplyDamageAfterDelay(targetHealth, attackDamage, damageDelay));
                    }

                if (distanceToTarget > aggroRange * 1.2f)
                {
                    _currentTarget = null;
                    ChangeState(NpcState.Idle);
                    yield break;
                }

                yield return null;
            }
        }

        private static IEnumerator ApplyDamageAfterDelay(Health targetHealth, float damage, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (targetHealth && targetHealth.gameObject.activeInHierarchy) targetHealth.TakeDamage(damage);
        }

        private IEnumerator FleeState()
        {
            if (fleeClips.Length > 0)
                AudioManager.Instance.PlaySound(interactAudioSource, Utils.RandomElement(fleeClips));

            speed = fleeSpeed;
            _currentTarget = null;

            FindNewFleeTarget();

            var fleeTimer = 0f;

            while (currentState == NpcState.Flee && fleeTimer < fleeDuration)
            {
                if (_health.CurrentHealth / _health.MaxHealth > healthFleeThreshold * 1.5f)
                {
                    ChangeState(NpcState.Idle);
                    yield break;
                }

                if (player && Vector3.Distance(transform.position, player.position) < fleeDistance * 0.7f)
                    FindNewFleeTarget();

                if (Vector3.Distance(transform.position, _fleeTarget) <
                    stopDistance * 1.5f)
                    FindNewFleeTarget(true);

                fleeTimer += Time.deltaTime;
                yield return null;
            }

            if (currentState != NpcState.Flee) yield break;

            speed = _originalSpeed;
            ChangeState(NpcState.Idle);
        }

        private void FindNewFleeTarget(bool forceRandom = false)
        {
            Vector3 fleeDirection;

            if (!forceRandom && player)
            {
                fleeDirection = (transform.position - player.position).normalized;
            }
            else
            {
                fleeDirection = Random.insideUnitSphere;
                fleeDirection.y = 0;
                fleeDirection = Vector3.ProjectOnPlane(fleeDirection, _surfaceNormal).normalized;
            }


            if (IsWaterAhead(fleeDirection))
            {
                var safeDirection = FindSafeDirection(fleeDirection);
                if (safeDirection.magnitude > 0.1f) fleeDirection = safeDirection;
                else fleeDirection = -fleeDirection;
            }

            _fleeTarget = transform.position + fleeDirection * fleeDistance;
        }


        private IEnumerator RotateToFace(Quaternion targetRotation)
        {
            while (Quaternion.Angle(transform.rotation, targetRotation) > 1.0f)
            {
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }

            transform.rotation = targetRotation;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            var projectedDirection = Vector3.ProjectOnPlane(direction, _surfaceNormal).normalized;
            if (projectedDirection.sqrMagnitude < 0.01f) return;

            var targetRotation = Quaternion.LookRotation(projectedDirection, _surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }


        private bool ShouldObservePlayer()
        {
            if (!player) return false;
            var distanceToPlayer = Vector3.Distance(transform.position, player.position);
            return distanceToPlayer <= observationDistance &&
                   distanceToPlayer > minLookDistance;
        }
    }
}