using System;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyShipAI : MonoBehaviour
    {
        [Header("Target Settings")] [SerializeField]
        private Transform target;

        [SerializeField] private float detectionRange = 10000f;
        [SerializeField] private float optimalCombatRange = 20f;
        [SerializeField] private float minCombatRange = 10f;

        [Header("Movement Settings")] [SerializeField]
        private SpaceMovementConfig movementConfig;

        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float strafeForce = 10f;
        [SerializeField] private float evasionForce = 15f;
        [SerializeField] private float brakingForce = 0.8f;
        [SerializeField] private float maxVelocity = 20f;
        [SerializeField] private float aimingThreshold = 30f;
        [SerializeField] private float minAimingSpeed = 2f;

        [Header("Combat Settings")] [SerializeField]
        private float firingRange = 30f;

        [SerializeField] private float firingCooldown = 1f;
        private AIState _currentState = AIState.Patrol;

        private Health _health; // add to controller script
        private bool _isAiming;
        private float _nextDirectionChangeTime;
        private float _nextFireTime;
        private Vector3 _patrolPoint;
        private float _patrolWaitTime;
        private Vector3 _randomStrafeDirection;
        private Rigidbody _rb;

        private IFireable[] _weaponMounts;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            SetNewPatrolPoint();
            _weaponMounts = GetComponentsInChildren<IFireable>(true);

            _rb.useGravity = false;

            _health = GetComponentInChildren<Health>(); // add to controller script
            _health.onDeath.AddListener(OnDeath); // add to controller script
            _health.onHealthChanged.AddListener(OnHealthChanged); // add to controller script
        }

        private void FixedUpdate()
        {
            if (!target)
            {
                FindTarget();
                return;
            }

            UpdateAIState();
            ExecuteCurrentState();
            ApplySpeedLimits();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, firingRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, optimalCombatRange);

            if (!_isAiming) return;

            Gizmos.color = Color.yellow;
            var aimDirection = transform.forward * firingRange;
            var rightAimBound = Quaternion.Euler(0, aimingThreshold, 0) * aimDirection;
            var leftAimBound = Quaternion.Euler(0, -aimingThreshold, 0) * aimDirection;

            Gizmos.DrawRay(transform.position, rightAimBound);
            Gizmos.DrawRay(transform.position, leftAimBound);
        }

        private void OnHealthChanged(float health) // add to controller script
        {
        }

        private void OnDeath() // add to controller script
        {
            Destroy(gameObject); // add to controller script
        }

        private void FindTarget()
        {
            var possibleTarget = GameObject.FindGameObjectWithTag("Asteroid");

            if (possibleTarget)
            {
                target = possibleTarget.transform;
            }
            else
            {
                var possibleAsteroid = GameObject.FindGameObjectWithTag("Player");

                if (possibleAsteroid) target = possibleAsteroid.transform;
            }
        }

        private void UpdateAIState()
        {
            var distanceToTarget = Vector3.Distance(transform.position, target.position);

            switch (_currentState)
            {
                case AIState.Patrol:
                    if (distanceToTarget <= detectionRange)
                    {
                        StopWeapons();
                        _currentState = AIState.Chase;
                    }

                    break;

                case AIState.Chase:
                    if (distanceToTarget <= firingRange)
                    {
                        StopWeapons();
                        _currentState = AIState.Combat;
                    }
                    else if (distanceToTarget > detectionRange)
                    {
                        StopWeapons();
                        _currentState = AIState.Patrol;
                    }

                    break;

                case AIState.Combat:
                    if (ShouldEvade())
                    {
                        StopWeapons();
                        _currentState = AIState.Evade;
                    }
                    else if (distanceToTarget > firingRange)
                    {
                        StopWeapons();
                        _currentState = AIState.Chase;
                    }

                    break;

                case AIState.Evade:
                    if (!ShouldEvade() && distanceToTarget <= firingRange)
                    {
                        StopWeapons();
                        _currentState = AIState.Combat;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StopWeapons()
        {
            foreach (var weaponMount in _weaponMounts) weaponMount.StopFire();
        }

        private void ExecuteCurrentState()
        {
            switch (_currentState)
            {
                case AIState.Patrol:
                    ExecutePatrolBehavior();
                    break;
                case AIState.Chase:
                    ExecuteChaseBehavior();
                    break;
                case AIState.Combat:
                    ExecuteCombatBehavior();
                    break;
                case AIState.Evade:
                    ExecuteEvadeBehavior();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ExecutePatrolBehavior()
        {
            if (Vector3.Distance(transform.position, _patrolPoint) < 5f)
            {
                if (_patrolWaitTime <= 0)
                    SetNewPatrolPoint();
                else
                    _patrolWaitTime -= Time.fixedDeltaTime;
                return;
            }

            RotateTowards(_patrolPoint);
            ApplyThrust(0.5f);
        }

        private void ExecuteChaseBehavior()
        {
            RotateTowards(target.position);
            ApplyThrust(1f);
        }

        private void ExecuteCombatBehavior()
        {
            var targetDirection = target.position - transform.position;
            var angleToTarget = Vector3.Angle(transform.forward, targetDirection);
            var distanceToTarget = targetDirection.magnitude;

            _isAiming = angleToTarget > aimingThreshold;

            if (_isAiming)
            {
                ApplyBraking();
                StopWeapons();
            }

            RotateTowards(target.position);

            if (!_isAiming)
            {
                var targetSpeed = Mathf.Clamp((distanceToTarget - optimalCombatRange) / 10f, -1f, 1f);
                ApplyThrust(targetSpeed);

                if (angleToTarget < aimingThreshold) ApplyStrafe();
            }

            if (angleToTarget <= aimingThreshold)
                foreach (var weaponMount in _weaponMounts)
                    weaponMount.Fire();
            else
                StopWeapons();
        }

        private void ExecuteEvadeBehavior()
        {
            var evadeDirection = transform.position - target.position;
            RotateTowards(transform.position + evadeDirection);

            _rb.AddForce(evadeDirection.normalized * evasionForce, ForceMode.Acceleration);
            ApplyStrafe();
        }

        private void RotateTowards(Vector3 targetPosition)
        {
            var targetDirection = targetPosition - transform.position;
            var targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        private void ApplyThrust(float multiplier)
        {
            var forwardVelocity = Vector3.Project(_rb.linearVelocity, transform.forward);
            if (forwardVelocity.magnitude < maxVelocity)
                _rb.AddRelativeForce(Vector3.forward * (movementConfig.Thrust * multiplier * Time.fixedDeltaTime));
        }

        private void ApplyBraking()
        {
            var brakeForce = -_rb.linearVelocity * brakingForce;

            if (_rb.linearVelocity.magnitude < minAimingSpeed)
                brakeForce = transform.forward * minAimingSpeed - _rb.linearVelocity;

            _rb.AddForce(brakeForce, ForceMode.Acceleration);
        }

        private void ApplySpeedLimits()
        {
            if (_rb.linearVelocity.magnitude > maxVelocity)
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxVelocity;
        }

        private void ApplyStrafe()
        {
            if (Time.time >= _nextDirectionChangeTime)
            {
                var randomPerp = Vector3.Cross(transform.forward, Random.insideUnitSphere).normalized;
                _randomStrafeDirection = randomPerp;
                _nextDirectionChangeTime = Time.time + Random.Range(1f, 3f);
            }

            if (_rb.linearVelocity.magnitude < maxVelocity) _rb.AddRelativeForce(_randomStrafeDirection * strafeForce);
        }

        private void SetNewPatrolPoint()
        {
            _patrolPoint = transform.position + Random.insideUnitSphere * 50f;
            _patrolPoint.y = transform.position.y;
            _patrolWaitTime = Random.Range(1f, 3f);
        }

        private bool ShouldEvade()
        {
            return Vector3.Distance(transform.position, target.position) < minCombatRange;
        }

        private enum AIState
        {
            Patrol,
            Chase,
            Combat,
            Evade
        }
    }
}