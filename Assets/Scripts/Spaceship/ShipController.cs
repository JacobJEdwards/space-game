using UnityEngine;
using System;
using JetBrains.Annotations;
using Unity.Cinemachine;
using Player;
using Managers;
using Movement;

namespace Spaceship
{
    public enum ShipState
    {
        Landed,
        Landing,
        Flying,
        SpaceIdle
    }

    [RequireComponent(typeof(Rigidbody))]
    public class ShipController : MonoBehaviour
    {
        [Serializable]
        private class CameraSettings
        {
            public CinemachineCamera thirdPersonCamera;
            public CinemachineCamera firstPersonCamera;
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
            public LayerMask surfaceLayer;
            public int landingRayCount = 16;
            public float landingRayRadius = 50f;
        }

        [SerializeField] private CameraSettings cameraSettings;
        [SerializeField] private LandingSettings landingSettings;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private UiManager uiManager;

        private Vector3 _landingPoint;
        private Vector3 _landingNormal;
        private bool _hasValidLandingPoint;
        [CanBeNull] private Collider _nearestLandingZone;
        private SpaceMovement _spaceMovement;
        private Rigidbody _rb;
        private PlayerController _currentPlayer;

        public bool IsOccupied => _currentPlayer != null;
        public ShipState CurrentState { get; private set; } = ShipState.SpaceIdle;

        private void Awake()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _rb = GetComponent<Rigidbody>();
            _spaceMovement = GetComponentInChildren<SpaceMovement>();

            var inputManager = FindFirstObjectByType<InputManager>();

            inputManager.SetOnLandingPressed(HandleLandingOrTakeoff);
        }

        private void FixedUpdate()
        {
            UpdateShipState();
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
                    HandleSpaceIdleState();
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
                uiManager.SetInfo("Finding Landing Zone...");
                FindLandingPoint();
                if (!_hasValidLandingPoint) return;
            }

            ExecuteLandingSequence();
        }

        private void HandleFlyingState()
        {
            SetKinematic(false);
            DetectLandingZones();
        }

        private void HandleSpaceIdleState()
        {
            SetKinematic(true);
        }

        private void SetKinematic(bool isKinematic)
        {
            if (_rb) _rb.isKinematic = isKinematic;
        }

        private void ExecuteLandingSequence()
        {
            _spaceMovement.enabled = false;
            uiManager.SetInfo("Landing...");

            var desiredPosition = _landingPoint + _landingNormal * landingSettings.hoverDistance;
            var desiredRotation = CalculateLandingRotation();

            var distanceToLanding = Vector3.Distance(transform.position, desiredPosition);

            UpdateLandingPositionAndRotation(desiredPosition, desiredRotation, distanceToLanding);

            if (IsLandingComplete(distanceToLanding, desiredRotation))
            {
                CompleteLanding();
            }
        }

        private Quaternion CalculateLandingRotation()
        {
            return Quaternion.LookRotation(
                Vector3.ProjectOnPlane(transform.forward, _landingNormal),
                _landingNormal
            );
        }

        private void UpdateLandingPositionAndRotation(Vector3 desiredPosition, Quaternion desiredRotation, float distanceToLanding)
        {
            if (distanceToLanding < landingSettings.approachDistance)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    desiredRotation,
                    landingSettings.rotationSpeed * Time.deltaTime
                );
            }

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

        private Collider GetNearestLandingZone(Collider[] landingZones)
        {
            var nearestDistance = float.MaxValue;
            Collider nearestLandingZone = null;

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
                uiManager.ClearHint();
                return;
            }

            var closestPoint = _nearestLandingZone.ClosestPoint(transform.position);
            var distance = Vector3.Distance(transform.position, closestPoint);

            uiManager.SetHint(distance < landingSettings.landingThreshold
                ? "Press L to land"
                : "");
        }

        private void FindLandingPoint()
        {
            if (!_nearestLandingZone) return;

            uiManager.SetInfo("Finding Landing Point...");

            var center = _nearestLandingZone.ClosestPoint(transform.position);
            var closestDistance = float.MaxValue;
            var foundPoint = false;

            for (var i = 0; i < landingSettings.landingRayCount; i++)
            {
                if (TryFindLandingPointInDirection(i, center, ref closestDistance))
                {
                    foundPoint = true;
                }
            }

            _hasValidLandingPoint = foundPoint;
        }

        private bool TryFindLandingPointInDirection(int index, Vector3 center, ref float closestDistance)
        {
            if (!_nearestLandingZone) return false;

            var direction = Quaternion.AngleAxis(
                index * (360f / landingSettings.landingRayCount),
                _nearestLandingZone.transform.up
            ) * _nearestLandingZone.transform.forward;

            if (!Physics.Raycast(center, direction, out var hit, landingSettings.landingRayRadius,
                    landingSettings.surfaceLayer))
            {
                return false;
            }

            var distance = Vector3.Distance(transform.position, hit.point);
            if (distance >= closestDistance) return false;

            _landingPoint = hit.point;
            _landingNormal = hit.normal;
            closestDistance = distance;
            return true;
        }

        private void OnEnable()
        {
            RegisterCameras();
        }

        private void OnDisable()
        {
            UnregisterCameras();
        }

        private void RegisterCameras()
        {
            if (cameraSettings.thirdPersonCamera)
                CameraController.Register(cameraSettings.thirdPersonCamera);
            if (cameraSettings.firstPersonCamera)
                CameraController.Register(cameraSettings.firstPersonCamera);
        }

        private void UnregisterCameras()
        {
            if (cameraSettings.thirdPersonCamera)
                CameraController.Unregister(cameraSettings.thirdPersonCamera);
            if (cameraSettings.firstPersonCamera)
                CameraController.Unregister(cameraSettings.firstPersonCamera);
        }

        public void PlayerEnteredShip(PlayerController player)
        {
            _currentPlayer = player;
            CameraController.SetActiveCamera(cameraSettings.thirdPersonCamera);

            if (CurrentState == ShipState.SpaceIdle)
            {
                CurrentState = ShipState.Flying;
            }
        }

        public void PlayerExitShip()
        {
            if (!_currentPlayer) return;

            _currentPlayer.ExitShip();
            _currentPlayer = null;
            uiManager.ClearHint();
            uiManager.TransitionToState(UIState.ZeroG);

            if (CurrentState == ShipState.Flying)
            {
                CurrentState = ShipState.SpaceIdle;
            }
        }

        public void OnInteract()
        {
            if (IsOccupied)
            {
                PlayerExitShip();
            }
        }

        public void OnSwitchCamera()
        {
            if (!IsOccupied) return;

            var newCamera = CameraController.IsActive(cameraSettings.thirdPersonCamera)
                ? cameraSettings.firstPersonCamera
                : cameraSettings.thirdPersonCamera;

            CameraController.SetActiveCamera(newCamera);
        }

        private void HandleLandingOrTakeoff()
        {
            switch (CurrentState)
            {
                case ShipState.Landed:
                    InitiateTakeoff();
                    break;
                case ShipState.Flying:
                    uiManager.ClearHint();
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
            CurrentState = ShipState.Flying;
            SetKinematic(false);
            _spaceMovement.enabled = true;
            uiManager.SetInfo("Ship Launched", 5);
        }

        private void InitiateLanding()
        {
            if (!_nearestLandingZone) return;

            _hasValidLandingPoint = false;
            CurrentState = ShipState.Landing;

            // run coroutine, lasts 5 seconds if landing not completed
            Invoke(nameof(MaybeFailLanding), 5);
        }

        private void MaybeFailLanding()
        {
            if (CurrentState != ShipState.Landing) return;

            _hasValidLandingPoint = false;
            uiManager.SetInfo("Landing Failed", 5);
            _spaceMovement.enabled = true;

            CurrentState = ShipState.Flying;
        }

        private void CompleteLanding()
        {
            CurrentState = ShipState.Landed;
            SetKinematic(true);
            _spaceMovement.enabled = false;
            uiManager.SetInfo("Ship Landed", 5);
        }

        private void OnDrawGizmosSelected()
        {
            DrawLandingZoneGizmos();
            DrawShipStateGizmos();
        }

        private void DrawLandingZoneGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, landingSettings.detectionRadius);

            if (_nearestLandingZone)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_nearestLandingZone.transform.position, 5f);
            }
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
    }
}