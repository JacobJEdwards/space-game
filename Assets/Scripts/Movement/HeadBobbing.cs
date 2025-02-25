#nullable enable

using System;
using Unity.Assertions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Movement
{
    public class HeadBobbing : MonoBehaviour
    {
        [SerializeField] private BobbingSettings settings = null!;
        [SerializeField] private Transform target = null!;

        private InputManager _inputManager = null!;

        private float _defaultPosY;
        private bool _isSprinting;
        private Vector3 _targetPos;
        private float _timer;

        private void Start()
        {
            _inputManager = InputManager.Instance;
            Assert.IsNotNull(settings, "Settings is not assigned");
            Assert.IsNotNull(_inputManager, "InputManager is not assigned");
            Assert.IsNotNull(target, "Target is not assigned");

            _defaultPosY = target.localPosition.y;
        }

        private void Update()
        {
            var horizontal = _inputManager.GetStrafe();
            var vertical = _inputManager.GetForward();
            var isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

            if (isMoving)
            {
                var bobbingSpeed = _isSprinting ? settings.runningBobbingSpeed : settings.walkingBobbingSpeed;
                var bobbingAmount = _isSprinting ? settings.sprintBobbingAmount : settings.bobbingAmount;

                _timer += Time.deltaTime * bobbingSpeed;
                _targetPos = target.localPosition;
                _targetPos.y = _defaultPosY + Mathf.Sin(_timer) * bobbingAmount;
            }
            else
            {
                _timer = 0f;
                _targetPos = new Vector3(target.localPosition.x, _defaultPosY, target.localPosition.z);
            }

            target.localPosition = Vector3.Lerp(
                target.localPosition,
                _targetPos,
                Time.deltaTime * settings.smoothing
            );
        }

        public void SetSprinting(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        [Serializable]
        public class BobbingSettings
        {
            public float walkingBobbingSpeed = 14f;
            public float runningBobbingSpeed = 18f;
            public float bobbingAmount = 0.05f;
            public float sprintBobbingAmount = 0.075f;
            public float smoothing = 16f;
        }
    }
}