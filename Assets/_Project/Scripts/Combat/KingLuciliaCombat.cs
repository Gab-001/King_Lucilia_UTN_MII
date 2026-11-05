using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;

namespace KingLucilia.Combat
{
    public class KingLuciliaCombat : MonoBehaviour
    {
        [Header("Stamina & Stats")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private float staminaRegenRate = 20f;
        [SerializeField] private float staminaRegenDelay = 1f;
        [SerializeField] private float dashStaminaCost = 25f;

        [Header("Light Combo Settings")]
        [SerializeField] private int[] comboDamages = { 15, 20, 35 };
        [SerializeField] private float attackStepDuration = 0.5f;
        [SerializeField] private float comboResetTime = 1.0f;
        [SerializeField] private float lightAttackRadius = 2.0f;

        [Header("Heavy Attack Settings")]
        [SerializeField] private int heavyDamage = 50;
        [SerializeField] private float heavyStaminaCost = 30f;
        [SerializeField] private float heavyAttackRadius = 2.5f;
        [SerializeField] private float heavyChargeDuration = 0.4f;

        [Header("Detection Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("References")]
        [SerializeField] private Animator animator;

        private int comboIndex = 0;
        private float lastAttackTime;
        private float lastStaminaConsumeTime;
        private bool isAttacking;
        public bool IsInvulnerable { get; private set; }

        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int HeavyAttackTriggerHash = Animator.StringToHash("HeavyAttack");
        private static readonly int ComboStepHash = Animator.StringToHash("ComboStep");

        public float CurrentStamina => currentStamina;
        public bool IsAttacking => isAttacking;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            currentStamina = maxStamina;
        }

        private void Update()
        {
            RegenerateStamina();

            // Respaldo directo de mouse por si el PlayerInput no rutea la acción
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    PerformLightAttack();
                }
                else if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    PerformHeavyAttack();
                }
            }

            if (comboIndex > 0 && Time.time - lastAttackTime > comboResetTime && !isAttacking)
            {
                comboIndex = 0;
                if (animator != null)
                {
                    animator.SetInteger(ComboStepHash, 0);
                }
            }
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                PerformLightAttack();
            }
        }

        public void OnHeavyAttack(InputValue value)
        {
            if (value.isPressed)
            {
                PerformHeavyAttack();
            }
        }

        public void PerformLightAttack()
        {
            if (isAttacking) return;
            if (currentStamina < 10f) return;

            ConsumeStamina(10f);
            StartCoroutine(LightAttackRoutine());
        }

        public void PerformHeavyAttack()
        {
            if (isAttacking) return;
            if (currentStamina < heavyStaminaCost) return;

            StartCoroutine(HeavyAttackRoutine());
        }

        private IEnumerator LightAttackRoutine()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.SetInteger(ComboStepHash, comboIndex);
                animator.SetTrigger(AttackTriggerHash);
            }

            int damage = (comboIndex < comboDamages.Length) ? comboDamages[comboIndex] : 15;
            Debug.Log($"<color=green>[Ataque Ligero]</color> Paso {comboIndex + 1} - Daño: {damage}");
            DealDamage(lightAttackRadius, damage);

            comboIndex = (comboIndex + 1) % comboDamages.Length;

            yield return new WaitForSeconds(attackStepDuration);
            isAttacking = false;
        }

        private IEnumerator HeavyAttackRoutine()
        {
            isAttacking = true;
            ConsumeStamina(heavyStaminaCost);

            if (animator != null)
            {
                animator.SetTrigger(HeavyAttackTriggerHash);
            }

            Debug.Log("<color=yellow>[Ataque Pesado]</color> Cargando estocada...");
            yield return new WaitForSeconds(heavyChargeDuration);

            Debug.Log($"<color=red>[Ataque Pesado]</color> ¡Impacto! - Daño: {heavyDamage}");
            DealDamage(heavyAttackRadius, heavyDamage);

            yield return new WaitForSeconds(0.35f);
            isAttacking = false;
        }

        private void DealDamage(float radius, int damageAmount)
        {
            Vector3 origin = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1.2f + Vector3.up * 0.9f;
            Collider[] hits = Physics.OverlapSphere(origin, radius, hitLayers);

            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject && hit.TryGetComponent<IDamageable>(out var victim))
                {
                    victim.TakeDamage(damageAmount, hit.ClosestPoint(origin), transform.forward);
                }
            }
        }

        public bool TryExecuteDash(float invulnerabilityDuration = 0.3f)
        {
            if (currentStamina < dashStaminaCost) return false;

            ConsumeStamina(dashStaminaCost);
            StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));
            return true;
        }

        private IEnumerator InvulnerabilityRoutine(float duration)
        {
            IsInvulnerable = true;
            yield return new WaitForSeconds(duration);
            IsInvulnerable = false;
        }

        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0f, currentStamina - amount);
            lastStaminaConsumeTime = Time.time;
        }

        private void RegenerateStamina()
        {
            if (Time.time - lastStaminaConsumeTime >= staminaRegenDelay && currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = attackPoint != null ? attackPoint.position : transform.position + transform.forward * 1.2f + Vector3.up * 0.9f;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, lightAttackRadius);
        }
    }
}
