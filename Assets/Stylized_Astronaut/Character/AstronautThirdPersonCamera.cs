using UnityEngine;

namespace AstronautThirdPersonCamera
{
    public class AstronautThirdPersonCamera : MonoBehaviour
    {
        private const float Y_ANGLE_MIN = 0.0f;
        private const float Y_ANGLE_MAX = 50.0f;

        public Transform lookAt;
        public Transform camTransform;
        public float distance = 5.0f;

        private float currentX;
        private float currentY = 45.0f;
        private float sensitivityX = 20.0f;
        private float sensitivityY = 20.0f;

        private void Start()
        {
            camTransform = transform;
        }

        private void Update()
        {
            currentX += Input.GetAxis("Mouse X");
            currentY += Input.GetAxis("Mouse Y");

            currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
        }

        private void LateUpdate()
        {
            var dir = new Vector3(0, 0, -distance);
            var rotation = Quaternion.Euler(currentY, currentX, 0);
            camTransform.position = lookAt.position + rotation * dir;
            camTransform.LookAt(lookAt.position);
        }
    }
}