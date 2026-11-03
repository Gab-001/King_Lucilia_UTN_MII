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

        [Header("Light Combo Settings")]
        [SerializeField] private int[] comboDamages = { 15, 20, 35 };
        [SerializeField] private float attackStepDuration = 0.55f; // Duración activa de cada tajo
        [SerializeField] private float comboResetTime = 1.0f;
        [SerializeField] private float lightAttackRadius = 2.0f;

        [Header("Heavy Attack Settings")]
        [SerializeField] private int heavyDamage = 50;
        [SerializeField] private float heavyStaminaCost = 30f;
        [SerializeField] private float heavyAttackRadius = 2.5f;
        [SerializeField] private float heavyChargeDuration = 0.5f;

        [Header("Detection Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask hitLayers;

        [Header("References")]
        [SerializeField] private Animator animator;

        private int comboIndex = 0;
        private float lastAttackTime;
        private float lastStaminaConsumeTime;
        private bool isAttacking;
        private bool inputBuffered;

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
        }

        private void Update()
        {
            RegenerateStamina();

            // Si se vence la ventana de combo sin nuevos ataques, resetea la secuencia
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
            if (!value.isPressed) return;

            if (!isAttacking)
            {
                StartCoroutine(ExecuteAttackStep());
            }
            else
            {
                // Guarda el clic en cola (buffer) si está cerca del final del golpe actual
                inputBuffered = true;
            }
        }

        public void OnHeavyAttack(InputValue value)
        {
            if (value.isPressed && !isAttacking && currentStamina >= heavyStaminaCost)
            {
                StartCoroutine(ExecuteHeavyAttackRoutine());
            }
        }

        private IEnumerator ExecuteAttackStep()
        {
            isAttacking = true;
            inputBuffered = false;
            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.SetInteger(ComboStepHash, comboIndex);
                animator.SetTrigger(AttackTriggerHash);
            }

            int damage = comboDamages[comboIndex];
            Debug.Log($"<color=green>[Combate Sucesivo]</color> Paso {comboIndex + 1} - Daño: {damage}");
            DealDamage(lightAttackRadius, damage);

            comboIndex = (comboIndex + 1) % comboDamages.Length;

            // Espera a que termine la porción activa de la animación
            yield return new WaitForSeconds(attackStepDuration);

            isAttacking = false;

            // Si el jugador presionó clic durante el golpe previo, encadena de inmediato el siguiente
            if (inputBuffered)
            {
                StartCoroutine(ExecuteAttackStep());
            }
        }

        private IEnumerator ExecuteHeavyAttackRoutine()
        {
            isAttacking = true;
            inputBuffered = false;
            currentStamina -= heavyStaminaCost;
            lastStaminaConsumeTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger(HeavyAttackTriggerHash);
            }

            Debug.Log("<color=yellow>[Combate]</color> Cargando Estocada Pesada...");
            yield return new WaitForSeconds(heavyChargeDuration);

            Debug.Log($"<color=red>[Combate]</color> ¡Estocada Pesada! - Daño: {heavyDamage}");
            DealDamage(heavyAttackRadius, heavyDamage);

            yield return new WaitForSeconds(0.4f);
            isAttacking = false;
        }

        private void DealDamage(float radius, int damageAmount)
        {
            Vector3 origin = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
            Collider[] hits = Physics.OverlapSphere(origin, radius, hitLayers);

            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject && hit.TryGetComponent<IDamageable>(out var victim))
                {
                    victim.TakeDamage(damageAmount, hit.ClosestPoint(origin), transform.forward);
                }
            }
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
            Vector3 origin = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, lightAttackRadius);
        }
    }
}
