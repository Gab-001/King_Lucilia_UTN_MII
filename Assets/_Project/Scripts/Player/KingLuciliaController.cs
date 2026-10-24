using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;

namespace KingLucilia.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class KingLuciliaController : MonoBehaviour, IDamageable
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 6.0f;
        [SerializeField] private float rotationSpeed = 15.0f;
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float jumpHeight = 1.5f;

        [Header("Health Settings")]
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int maxHealth = 100;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController characterController;
        private Vector2 moveInput;
        private Vector3 verticalVelocity;
        private bool jumpRequested;

        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed && characterController.isGrounded)
            {
                jumpRequested = true;
            }
        }

        private void Update()
        {
            Vector3 horizontalMove = CalculateHorizontalMovement();
            Vector3 verticalMove = CalculateVerticalForces();

            // Unificación en un único desplazamiento físico por frame
            characterController.Move((horizontalMove + verticalMove) * Time.deltaTime);
        }

        private Vector3 CalculateHorizontalMovement()
        {
            if (moveInput.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 moveDirection;

            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            }
            else
            {
                moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            }

            if (moveDirection.sqrMagnitude >= 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            return moveDirection * moveSpeed;
        }

        private Vector3 CalculateVerticalForces()
        {
            if (characterController.isGrounded)
            {
                if (verticalVelocity.y < 0f)
                {
                    verticalVelocity.y = -2f;
                }

                if (jumpRequested)
                {
                    verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    jumpRequested = false;
                }
            }
            else
            {
                jumpRequested = false;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            return verticalVelocity;
        }

        public void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damageAmount);
            Debug.Log($"<color=red>[Player Daño]</color> Lucilia recibió {damageAmount} de daño. Vida actual: {currentHealth}/{maxHealth}");
        }
    }
}
