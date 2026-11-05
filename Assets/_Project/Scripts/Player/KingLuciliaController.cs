using System.Collections;
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

        [Header("Dash Settings")]
        [SerializeField] private float dashDistance = 6.0f;
        [SerializeField] private float dashDuration = 0.25f;

        [Header("Health Settings")]
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int maxHealth = 100;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private KingLuciliaCombat combat;

        private CharacterController characterController;
        private Vector2 moveInput;
        private Vector3 verticalVelocity;
        private bool jumpRequested;
        private bool isDashing;
        private Vector3 dashDirection;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int DieHash = Animator.StringToHash("Die");

        public bool IsDead => currentHealth <= 0;
        public int CurrentHealth => currentHealth;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (combat == null)
            {
                combat = GetComponent<KingLuciliaCombat>();
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        public void OnMove(InputValue value)
        {
            if (IsDead) return;
            moveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (!value.isPressed || IsDead || isDashing) return;

            // Si está atacando, no saltar ni hacer dash
            if (combat != null && combat.IsAttacking) return;

            if (moveInput.sqrMagnitude > 0.05f)
            {
                if (combat != null && combat.TryExecuteDash(0.3f))
                {
                    StartCoroutine(PerformDashRoutine());
                    return;
                }
            }

            if (characterController.isGrounded)
            {
                jumpRequested = true;
            }
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed && combat != null && !IsDead)
            {
                combat.PerformLightAttack();
            }
        }

        public void OnHeavyAttack(InputValue value)
        {
            if (value.isPressed && combat != null && !IsDead)
            {
                combat.PerformHeavyAttack();
            }
        }

        private IEnumerator PerformDashRoutine()
        {
            isDashing = true;
            float elapsed = 0f;
            float speed = dashDistance / dashDuration;

            dashDirection = GetMovementDirection();
            if (dashDirection.sqrMagnitude < 0.01f)
            {
                dashDirection = transform.forward;
            }

            while (elapsed < dashDuration)
            {
                characterController.Move(dashDirection * speed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            isDashing = false;
        }

        private void Update()
        {
            if (IsDead) return;

            if (isDashing)
            {
                ApplyGravityOnly();
                UpdateAnimator(moveSpeed);
                return;
            }

            Vector3 horizontalMove = CalculateHorizontalMovement();
            Vector3 verticalMove = CalculateVerticalForces();

            characterController.Move((horizontalMove + verticalMove) * Time.deltaTime);

            UpdateAnimator(horizontalMove.magnitude);
        }

        private Vector3 GetMovementDirection()
        {
            if (moveInput.sqrMagnitude < 0.01f) return Vector3.zero;

            Vector3 moveDir;
            if (cameraTransform != null)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            }
            else
            {
                moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            }

            return moveDir;
        }

        private Vector3 CalculateHorizontalMovement()
        {
            // Bloquea movimiento si está ejecutando un ataque
            if (combat != null && combat.IsAttacking)
            {
                return Vector3.zero;
            }

            Vector3 moveDirection = GetMovementDirection();

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
                if (verticalVelocity.y < 0f) verticalVelocity.y = -2f;

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

        private void ApplyGravityOnly()
        {
            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }
            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move(verticalVelocity * Time.deltaTime);
        }

        private void UpdateAnimator(float currentSpeed)
        {
            if (animator == null) return;
            float speedToSend = (combat != null && combat.IsAttacking) ? 0f : currentSpeed;
            animator.SetFloat(SpeedHash, speedToSend, 0.1f, Time.deltaTime);
            animator.SetBool(IsGroundedHash, characterController.isGrounded);
        }

        public void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead) return;

            if (combat != null && combat.IsInvulnerable)
            {
                Debug.Log("<color=cyan>[Dash i-frames]</color> ¡Daño evadido exitosamente!");
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damageAmount);
            Debug.Log($"<color=red>[Player Daño]</color> Lucilia recibió {damageAmount} de daño. Vida actual: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                if (animator != null)
                {
                    animator.SetFloat(SpeedHash, 0f);
                    animator.SetTrigger(DieHash);
                }

                if (combat != null) combat.enabled = false;
                if (characterController != null) characterController.enabled = false;
                enabled = false;
            }
        }
    }
}
