using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [RequireComponent(typeof(NpcMovement), typeof(Rigidbody))]
    public class NpcMovementManager : MonoBehaviour
    {
    [Header("Behavior Control")]
    [SerializeField] private float activationDistance = 50f;
    [SerializeField] private float behaviorUpdateInterval = 0.5f;

    [Header("Idle Behavior")]
    [SerializeField] private float idleWanderRadius = 10f;
    [SerializeField] private float pointOfInterestRadius = 15f;
    [SerializeField] private float lookAtDuration = 2f;
    [SerializeField] private LayerMask pointsOfInterestLayers;

    [Header("Flocking")]
    [SerializeField] private float neighborRadius = 5f;
    [SerializeField] private float separationWeight = 1.5f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleDetectionRange = 3f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidanceWeight = 2f;

    private Transform _planet;

    private Transform _player;
    private NpcMovement _movement;
    private Vector3 _currentTarget;
    private float _nextBehaviorUpdate;
    private float _currentLookAtTime;
    private bool _isActive;
    private Rigidbody _rb;

    private static readonly List<NpcMovementManager> ActiveNpCs = new();

    private void Awake()
    {
        _movement = GetComponent<NpcMovement>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _planet = _movement.planet;
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ActiveNpCs.Add(this);
    }

    private void OnDisable()
    {
        ActiveNpCs.Remove(this);
    }

    private void Update()
    {
        UpdateActivationState();

        if (!_isActive) return;

        if (!(Time.time >= _nextBehaviorUpdate)) return;

        UpdateBehavior();
        _nextBehaviorUpdate = Time.time + behaviorUpdateInterval;
    }

    private void UpdateActivationState()
    {
        var distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        _isActive = distanceToPlayer <= activationDistance;

        enabled = _isActive;
    }

    private void UpdateBehavior()
    {
        var flockingForce = CalculateFlockingForce();
        var avoidanceForce = CalculateObstacleAvoidance();

        if (flockingForce.magnitude < 0.1f && avoidanceForce.magnitude < 0.1f)
        {
            HandleIdleBehavior();
        }
        else
        {
            var combinedForce = (flockingForce + avoidanceForce).normalized;
            _currentTarget = ProjectOnPlanetSurface(transform.position + combinedForce * 5f);
        }

        // _movement.SetTarget(_currentTarget);
    }

    private Vector3 ProjectOnPlanetSurface(Vector3 point)
    {
        var directionToPlanet = (_planet.position - point).normalized;
        var distanceToPlanetSurface = Vector3.Distance(_planet.position, transform.position);
        return _planet.position - directionToPlanet * distanceToPlanetSurface;
    }

    private Vector3 CalculateFlockingForce()
    {
        var separation = Vector3.zero;
        var alignment = Vector3.zero;
        var cohesion = Vector3.zero;
        var neighborCount = 0;

        foreach (var npc in ActiveNpCs)
        {
            if (npc == this) continue;

            var distance = Vector3.Distance(transform.position, npc.transform.position);
            if (!(distance <= neighborRadius)) continue;

            var diff = transform.position - npc.transform.position;
            var projectedDiff = Vector3.ProjectOnPlane(diff, (transform.position - _planet.position).normalized);
            separation += projectedDiff.normalized / distance;

            var projectedVelocity = Vector3.ProjectOnPlane(_rb.linearVelocity, (transform.position - _planet.position).normalized);

            alignment += projectedVelocity;

            cohesion += ProjectOnPlanetSurface(npc.transform.position);

            neighborCount++;
        }

        if (neighborCount <= 0) return Vector3.zero;

        alignment /= neighborCount;
        cohesion /= neighborCount;
        cohesion = (cohesion - transform.position).normalized;

        return (separation * separationWeight +
                alignment.normalized * alignmentWeight +
                cohesion * cohesionWeight) / 3f;

    }

    private Vector3 CalculateObstacleAvoidance()
    {
        var avoidanceForce = Vector3.zero;
        var surfaceNormal = (transform.position - _planet.position).normalized;

        var forward = Vector3.ProjectOnPlane(transform.forward, surfaceNormal).normalized;

        var rayDirections = new[]
        {
            forward,
            Quaternion.AngleAxis(20, surfaceNormal) * forward,
            Quaternion.AngleAxis(-20, surfaceNormal) * forward
        };

        foreach (var direction in rayDirections)
        {
            if (!Physics.Raycast(transform.position, direction, out var hit, obstacleDetectionRange,
                    obstacleLayer)) continue;

            var avoidDir = Vector3.ProjectOnPlane(transform.position - hit.point, surfaceNormal).normalized;
            avoidanceForce += avoidDir * (1f - hit.distance / obstacleDetectionRange);
        }

        return avoidanceForce * avoidanceWeight;
    }

    private void HandleIdleBehavior()
    {
        if (_currentLookAtTime <= 0)
        {
            if (Random.value > 0.5f)
            {
                FindPointOfInterest();
            }
            else
            {
                GenerateWanderPoint();
            }
        }
        else
        {
            _currentLookAtTime -= Time.deltaTime;
        }
    }

    private void FindPointOfInterest()
    {
        if (Vector3.Distance(transform.position, _player.position) < pointOfInterestRadius)
        {
            _currentTarget = _player.position;
            _currentLookAtTime = lookAtDuration;
            return;
        }

        var collides = new Collider[10];
        var size = Physics.OverlapSphereNonAlloc(transform.position, pointOfInterestRadius, collides, pointsOfInterestLayers);
        if (size > 0)
        {
            var pointOfInterest = collides[Random.Range(0, size)];
            _currentTarget = pointOfInterest.transform.position;
            _currentLookAtTime = lookAtDuration;
        }
        else
        {
            GenerateWanderPoint();
        }
    }

    private void GenerateWanderPoint()
    {
        var surfaceNormal = (transform.position - _planet.position).normalized;
        var randomPoint = Vector3.ProjectOnPlane(Random.insideUnitSphere, surfaceNormal).normalized;
        _currentTarget = transform.position + randomPoint * idleWanderRadius;
        _currentTarget = ProjectOnPlanetSurface(_currentTarget);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationDistance);

        if (!_isActive) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, idleWanderRadius);
    }
    }
}