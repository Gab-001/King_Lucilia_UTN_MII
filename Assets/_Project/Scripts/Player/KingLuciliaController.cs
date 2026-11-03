using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;
using KingLucilia.Combat;

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
        [SerializeField] private Animator animator;

        private CharacterController characterController;
        private Vector2 moveInput;
        private Vector3 verticalVelocity;
        private bool jumpRequested;
        private bool isDead = false;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int DieHash = Animator.StringToHash("Die");

        public bool IsDead => isDead;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        public void OnMove(InputValue value)
        {
            if (isDead) return;
            moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (isDead) return;
            if (value.isPressed && characterController.isGrounded)
            {
                jumpRequested = true;
            }
        }

        private void Update()
        {
            if (isDead) return;

            Vector3 horizontalMove = CalculateHorizontalMovement();
            Vector3 verticalMove = CalculateVerticalForces();

            characterController.Move((horizontalMove + verticalMove) * Time.deltaTime);

            if (animator != null)
            {
                float currentSpeed = new Vector2(horizontalMove.x, horizontalMove.z).magnitude;
                animator.SetFloat(SpeedHash, currentSpeed, 0.1f, Time.deltaTime);
                animator.SetBool(IsGroundedHash, characterController.isGrounded);
            }
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
            if (isDead) return;

            currentHealth = Mathf.Max(0, currentHealth - damageAmount);
            Debug.Log($"<color=red>[Player Daño]</color> Lucilia recibió {damageAmount} de daño. Vida actual: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            moveInput = Vector2.zero;
            verticalVelocity = Vector3.zero;

            if (animator != null)
            {
                animator.SetFloat(SpeedHash, 0f);
                animator.SetTrigger(DieHash);
            }

            // Deshabilitar combate si el componente existe
            if (TryGetComponent<KingLuciliaCombat>(out var combat))
            {
                combat.enabled = false;
            }

            // Deshabilitar interacciones si el componente existe
            if (TryGetComponent<KingLuciliaInteraction>(out var interaction))
            {
                interaction.enabled = false;
            }

            // Desactivar CharacterController para liberar colisiones
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            Debug.Log("<color=black>[Player Derrotado]</color> King Lucilia ha caído en combate.");
        }
    }
}
