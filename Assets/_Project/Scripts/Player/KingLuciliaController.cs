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

        [Header("Health Settings")]
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int maxHealth = 100;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController characterController;
        private Vector2 moveInput;
        private Vector3 verticalVelocity;

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

        private void Update()
        {
            HandleMovement();
            ApplyGravity();
        }

        private void HandleMovement()
        {
            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                if (cameraTransform != null)
                {
                    targetAngle += cameraTransform.eulerAngles.y;
                }

                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                Vector3 moveDirection = targetRotation * Vector3.forward;
                characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            }
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move(verticalVelocity * Time.deltaTime);
        }

        public void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damageAmount);
            Debug.Log($"<color=red>[Player Daño]</color> Lucilia recibió {damageAmount} de daño. Vida actual: {currentHealth}/{maxHealth}");
        }
    }
}
