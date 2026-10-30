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
        [SerializeField] private float comboResetTime = 0.8f;
        [SerializeField] private float lightAttackRadius = 2.0f;

        [Header("Heavy Attack Settings")]
        [SerializeField] private int heavyDamage = 50;
        [SerializeField] private float heavyStaminaCost = 30f;
        [SerializeField] private float heavyAttackRadius = 2.5f;
        [SerializeField] private float heavyChargeDuration = 0.5f;

        [Header("Detection Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask hitLayers;

        private int comboIndex = 0;
        private float lastAttackTime;
        private float lastStaminaConsumeTime;
        private bool isAttacking;

        public float CurrentStamina => currentStamina;

        private void Update()
        {
            RegenerateStamina();

            if (comboIndex > 0 && Time.time - lastAttackTime > comboResetTime && !isAttacking)
            {
                comboIndex = 0;
            }
        }

        public void OnAttack(InputValue value)
        {
            if (value.isPressed && !isAttacking)
            {
                ExecuteLightAttack();
            }
        }

        public void OnHeavyAttack(InputValue value)
        {
            if (value.isPressed && !isAttacking && currentStamina >= heavyStaminaCost)
            {
                StartCoroutine(ExecuteHeavyAttackRoutine());
            }
        }

        private void ExecuteLightAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            int damage = comboDamages[comboIndex];
            Debug.Log($"<color=green>[Combate]</color> Ataque Ligero Paso {comboIndex + 1} - Daño: {damage}");

            DealDamage(lightAttackRadius, damage);

            comboIndex = (comboIndex + 1) % comboDamages.Length;
            isAttacking = false;
        }

        private IEnumerator ExecuteHeavyAttackRoutine()
        {
            isAttacking = true;
            currentStamina -= heavyStaminaCost;
            lastStaminaConsumeTime = Time.time;

            Debug.Log("<color=yellow>[Combate]</color> Cargando Estocada Pesada...");
            yield return new WaitForSeconds(heavyChargeDuration);

            Debug.Log($"<color=red>[Combate]</color> ¡Estocada Pesada ejecutada! - Daño: {heavyDamage}");
            DealDamage(heavyAttackRadius, heavyDamage);

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
