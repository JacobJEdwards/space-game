using UnityEngine;

namespace AstronautPlayer
{
    public class AstronautPlayer : MonoBehaviour
    {
        private static readonly int AnimationPar = Animator.StringToHash("AnimationPar");

        public float speed = 600.0f;
        public float turnSpeed = 400.0f;
        public float gravity = 20.0f;

        private Animator anim;
        private CharacterController controller;
        private Vector3 moveDirection = Vector3.zero;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            anim = gameObject.GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            anim.SetInteger(AnimationPar, Input.GetKey("w") ? 1 : 0);

            if (controller.isGrounded) moveDirection = transform.forward * (Input.GetAxis("Vertical") * speed);

            var turn = Input.GetAxis("Horizontal");
            transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
            controller.Move(moveDirection * Time.deltaTime);
            moveDirection.y -= gravity * Time.deltaTime;
        }
    }
}