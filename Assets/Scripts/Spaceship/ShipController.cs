#nullable enable

using System;
using System.Collections.Generic;
using HUDIndicator;
using Interfaces;
using Managers;
using Movement;
using Player;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;

namespace Spaceship
{
    public enum ShipState
    {
        Landed,
        Landing,
        Flying,
        SpaceIdle
    }

    [RequireComponent(typeof(Rigidbody), typeof(SpaceMovement))]
    public class ShipController : MonoBehaviour, IUpgradeable
    {
        [SerializeField] private CameraSettings cameraSettings = null!;
        [SerializeField] private LandingSettings landingSettings = null!;
        [SerializeField] private Indicator[] indicators = null!;

        [SerializeField] private ParticleSystem landingParticles = null!;

        [SerializeField] private ParticleSystem damageParticles = null!;

        [SerializeField] private Transform[] damagePoints = null!;

        private readonly List<ShipUpgrade> _upgrades = new();

        private CameraController _cameraController = null!;
        private PlayerController? _currentPlayer;
        private bool _hasValidLandingPoint;
        private Health _health = null!;
        private Vector3 _landingNormal;

        private Vector3 _landingPoint;
        private Collider? _nearestLandingZone;
        private int _numDamageParticles;

        private Rigidbody _rb = null!;
        private ShipShooting _shipShooting = null!;
        private SpaceMovement _spaceMovement = null!;
        private UiManager _uiManager = null!;

        public bool IsOccupied => _currentPlayer;
        public ShipState CurrentState { get; private set; } = ShipState.SpaceIdle;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            _uiManager = UiManager.Instance;
            _cameraController = CameraController.Instance;
            _shipShooting = GetComponent<ShipShooting>();
            landingSettings.landingLayers = LayerMask.GetMask("PlanetSurface", "Water");

            var inputManager = InputManager.Instance;
            inputManager.SetOnLandingPressed(HandleLandingOrTakeoff);

            _health.onHealthChanged.AddListener(OnHealthChanged);

            SetCurrentState(ShipState.SpaceIdle, true);
        }

        private void FixedUpdate()
        {
            UpdateShipState();
        }

        private void OnEnable()
        {
            RegisterCameras();
        }

        private void OnDisable()
        {
            UnregisterCameras();
        }

        private void OnDrawGizmosSelected()
        {
            DrawLandingZoneGizmos();
            DrawShipStateGizmos();
        }

        public bool CanApplyUpgrade(BaseUpgrade upgrade)
        {
            return upgrade is ShipUpgrade;
        }

        public void ApplyUpgrade(BaseUpgrade upgrade)
        {
            switch (upgrade)
            {
                case ShipLaserUpgrade shipLaserUpgrade:
                    _shipShooting.ApplyUpgrade(shipLaserUpgrade);
                    break;
                case ShipEngineUpgrade shipEngineUpgrade:
                    _spaceMovement.ApplyUpgrade(shipEngineUpgrade);
                    break;
            }

            if (upgrade is ShipUpgrade shipUpgrade) _upgrades.Add(shipUpgrade);
        }

        public UpgradeType GetUpgradeType()
        {
            return UpgradeType.Ship;
        }

        private void OnHealthChanged(float health)
        {
            var thresholds = new[] { 0.25f, 0.5f, 0.75f };

            for (var i = _numDamageParticles; i < thresholds.Length; i++)
            {
                var threshold = thresholds[i];
                if (!(health < _health.MaxHealth * threshold)) continue;

                var particles = Instantiate(damageParticles, transform.position, Quaternion.identity);
                particles.transform.parent = transform;
                particles.transform.localPosition = damagePoints[i].localPosition;
                particles.transform.localRotation = damagePoints[i].localRotation;
                particles.Play();
                _numDamageParticles++;
                break;
            }
        }

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _spaceMovement = GetComponent<SpaceMovement>();
            _health = GetComponent<Health>();


            Assert.IsNotNull(_rb, "Rigidbody is not set!");
            Assert.IsNotNull(_spaceMovement, "Space movement is not set!");
            Assert.IsNotNull(cameraSettings.thirdPersonCamera, "Third person camera is not set!");
            Assert.IsNotNull(cameraSettings.firstPersonCamera, "First person camera is not set!");
            Assert.IsNotNull(landingSettings, "Landing settings are not set!");
        }

        private void UpdateShipState()
        {
            switch (CurrentState)
            {
                case ShipState.Landed:
                    HandleLandedState();
                    break;
                case ShipState.Landing:
                    HandleLandingState();
                    break;
                case ShipState.Flying:
                    HandleFlyingState();
                    break;
                case ShipState.SpaceIdle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleLandedState()
        {
            SetKinematic(true);
        }

        private void HandleLandingState()
        {
            if (!_nearestLandingZone) return;

            if (!_hasValidLandingPoint)
            {
                _uiManager.SetInfo("Finding Landing Zone...");
                FindLandingPoint();
                if (!_hasValidLandingPoint) return;
            }

            ExecuteLandingSequence();
        }

        private void HandleFlyingState()
        {
            DetectLandingZones();
        }

        private void SetKinematic(bool isKinematic)
        {
            _rb.isKinematic = isKinematic;
        }

        private void ExecuteLandingSequence()
        {
            _spaceMovement.enabled = false;
            _uiManager.SetInfo("Landing...");

            var desiredPosition = _landingPoint + _landingNormal * landingSettings.hoverDistance;
            var desiredRotation = CalculateLandingRotation();

            var distanceToLanding = Vector3.Distance(transform.position, desiredPosition);

            UpdateLandingPositionAndRotation(desiredPosition, desiredRotation, distanceToLanding);

            if (IsLandingComplete(distanceToLanding, desiredRotation)) CompleteLanding();
        }

        private Quaternion CalculateLandingRotation()
        {
            return Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, _landingNormal),
                _landingNormal
            );
        }

        private void UpdateLandingPositionAndRotation(Vector3 desiredPosition, Quaternion desiredRotation,
            float distanceToLanding)
        {
            if (distanceToLanding < landingSettings.approachDistance)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    desiredRotation,
                    landingSettings.rotationSpeed * Time.deltaTime
                );

            transform.position = Vector3.MoveTowards(
                transform.position,
                desiredPosition,
                landingSettings.approachSpeed * Time.deltaTime
            );
        }

        private bool IsLandingComplete(float distanceToLanding, Quaternion desiredRotation)
        {
            return distanceToLanding < landingSettings.hoverDistance &&
                   Quaternion.Angle(transform.rotation, desiredRotation) < 5f;
        }

        private void DetectLandingZones()
        {
            var landingZones = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(
                transform.position,
                landingSettings.detectionRadius,
                landingZones,
                landingSettings.landingLayer
            );

            if (size == 0)
            {
                _nearestLandingZone = null;
                UpdateLandingUI();
                return;
            }

            _nearestLandingZone = GetNearestLandingZone(landingZones);

            UpdateLandingUI();
        }

        private Collider? GetNearestLandingZone(Collider[] landingZones)
        {
            var nearestDistance = float.MaxValue;
            Collider? nearestLandingZone = null;

            foreach (var landingZone in landingZones)
            {
                if (!landingZone) continue;

                var distance = Vector3.Distance(transform.position, landingZone.transform.position);
                if (distance >= nearestDistance) continue;

                nearestDistance = distance;
                nearestLandingZone = landingZone;
            }

            return nearestLandingZone;
        }

        private void UpdateLandingUI()
        {
            if (!_nearestLandingZone)
            {
                _uiManager.ClearHint();
                return;
            }

            var closestPoint = _nearestLandingZone.ClosestPoint(transform.position);
            var distance = Vector3.Distance(transform.position, closestPoint);

            if (distance < landingSettings.landingThreshold)
                _uiManager.SetHint("Press L to land");
        }

        private void FindLandingPoint()
        {
            if (!_nearestLandingZone) return;

            _uiManager.SetInfo("Finding Landing Point...");

            var center = _nearestLandingZone.ClosestPoint(transform.position);
            var closestDistance = float.MaxValue;
            var foundPoint = false;

            for (var i = 0; i < landingSettings.landingRayCount; i++)
                if (TryFindLandingPointInDirection(i, center, ref closestDistance))
                    foundPoint = true;

            _hasValidLandingPoint = foundPoint;
        }

        private bool TryFindLandingPointInDirection(int index, Vector3 center, ref float closestDistance)
        {
            if (!_nearestLandingZone) return false;

            var direction = Quaternion.AngleAxis(
                index * (360f / landingSettings.landingRayCount),
                _nearestLandingZone.transform.up
            ) * _nearestLandingZone.transform.forward;

            Debug.DrawRay(center, direction, Color.red);

            if (!Physics.Raycast(center, direction, out var hit, landingSettings.landingRayRadius,
                    landingSettings.landingLayers))
                return false;

            if (hit.transform.gameObject.layer != (int)Layers.PlanetSurface) return false;

            var distance = Vector3.Distance(transform.position, hit.point);
            if (distance >= closestDistance) return false;

            _landingPoint = hit.point;
            _landingNormal = hit.normal;
            closestDistance = distance;
            return true;
        }

        private void RegisterCameras()
        {
            if (cameraSettings.thirdPersonCamera)
                _cameraController?.Register(cameraSettings.thirdPersonCamera);
            if (cameraSettings.firstPersonCamera)
                _cameraController?.Register(cameraSettings.firstPersonCamera);
        }

        private void UnregisterCameras()
        {
            if (cameraSettings.thirdPersonCamera)
                _cameraController?.Unregister(cameraSettings.thirdPersonCamera);
            if (cameraSettings.firstPersonCamera)
                _cameraController?.Unregister(cameraSettings.firstPersonCamera);
        }

        public void PlayerEnteredShip(PlayerController player)
        {
            _currentPlayer = player;
            _cameraController.SetActiveCamera(cameraSettings.thirdPersonCamera);

            foreach (var indicator in indicators) indicator.enabled = false;

            if (CurrentState == ShipState.SpaceIdle) SetCurrentState(ShipState.Flying);
        }

        public void PlayerExitShip()
        {
            if (!_currentPlayer) return;

            foreach (var indicator in indicators) indicator.enabled = true;

            _currentPlayer.ExitShip();
            _currentPlayer = null;
            _uiManager.ClearHint();
            _uiManager.TransitionToState(UIState.ZeroG);

            if (CurrentState == ShipState.Flying) SetCurrentState(ShipState.SpaceIdle);
        }

        private void SetCurrentState(ShipState state, bool force = false)
        {
            if (state == CurrentState && !force) return;

            switch (state)
            {
                case ShipState.Flying:
                {
                    SetKinematic(false);
                    _spaceMovement.enabled = true;
                }
                    break;
                case ShipState.Landed:
                case ShipState.SpaceIdle:
                {
                    SetKinematic(true);
                    _spaceMovement.enabled = false;
                }
                    break;
                case ShipState.Landing:
                {
                    _spaceMovement.enabled = false;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            CurrentState = state;
        }

        public void OnInteract()
        {
            if (IsOccupied) PlayerExitShip();
        }

        public void OnSwitchCamera()
        {
            if (!IsOccupied) return;

            var newCamera = _cameraController.IsActive(cameraSettings.thirdPersonCamera)
                ? cameraSettings.firstPersonCamera
                : cameraSettings.thirdPersonCamera;

            _cameraController.SetActiveCamera(newCamera);
        }

        private void HandleLandingOrTakeoff()
        {
            switch (CurrentState)
            {
                case ShipState.Landed:
                    InitiateTakeoff();
                    break;
                case ShipState.Flying:
                    _uiManager.ClearHint();
                    InitiateLanding();
                    break;
                case ShipState.Landing:
                case ShipState.SpaceIdle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InitiateTakeoff()
        {
            SetCurrentState(ShipState.Flying);
            SetKinematic(false);
            _uiManager.SetInfo("Ship Launched", 5);
        }

        private void InitiateLanding()
        {
            if (!_nearestLandingZone) return;

            _hasValidLandingPoint = false;
            SetCurrentState(ShipState.Landing);

            Invoke(nameof(MaybeFailLanding), 5);
        }

        private void MaybeFailLanding()
        {
            if (CurrentState != ShipState.Landing) return;

            _hasValidLandingPoint = false;
            _uiManager.SetInfo("Landing Failed", 5);
            SetCurrentState(ShipState.Flying);
        }

        private void CompleteLanding()
        {
            SetCurrentState(ShipState.Landed);
            SetKinematic(true);
            _uiManager.SetInfo("Ship Landed", 5);
        }

        private void DrawLandingZoneGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, landingSettings.detectionRadius);

            if (!_nearestLandingZone) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_nearestLandingZone.transform.position, 5f);
        }

        private void DrawShipStateGizmos()
        {
            switch (CurrentState)
            {
                case ShipState.Landed:
                    DrawLandedGizmos();
                    break;
                case ShipState.Landing:
                    DrawLandingGizmos();
                    break;
                case ShipState.Flying:
                case ShipState.SpaceIdle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawLandedGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }

        private void DrawLandingGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 5f);

            if (!_hasValidLandingPoint) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_landingPoint, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_landingPoint, _landingNormal * 2f);

            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(_landingPoint, landingSettings.approachDistance);
        }

        [Serializable]
        private class CameraSettings
        {
            public CinemachineCamera thirdPersonCamera = null!;
            public CinemachineCamera firstPersonCamera = null!;
        }

        [Serializable]
        private class LandingSettings
        {
            public LayerMask landingLayer;
            public float detectionRadius = 100f;
            public float landingThreshold = 500f;
            public float approachDistance = 20f;
            public float hoverDistance = 2f;
            public float rotationSpeed = 2f;
            public float approachSpeed = 5f;
            public LayerMask landingLayers;
            public int landingRayCount = 16;
            public float landingRayRadius = 50f;
        }
    }
}