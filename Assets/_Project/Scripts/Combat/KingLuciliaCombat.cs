using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using KingLucilia.Core;
using KingLucilia.Audio;

namespace KingLucilia.Combat
{
    public class KingLuciliaCombat : MonoBehaviour
    {
        [Header("Stamina & Spore Stats (GDD 5.1)")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float currentStamina = 100f;
        [SerializeField] private float staminaRegenRate = 20f;
        [SerializeField] private float staminaRegenDelay = 1f;
        [SerializeField] private float dashStaminaCost = 25f;

        [Space(5)]
        [SerializeField] private float maxSporeCharge = 100f;
        [SerializeField] private float currentSporeCharge = 60f;

        [Header("Light Combo Settings")]
        [SerializeField] private int[] comboDamages = { 15, 20, 35 };
        [SerializeField] private float attackStepDuration = 0.45f;
        [SerializeField] private float comboResetTime = 1.0f;
        [SerializeField] private float lightAttackRadius = 2.0f;

        [Header("Heavy Attack Settings")]
        [SerializeField] private int heavyDamage = 50;
        [SerializeField] private float heavyStaminaCost = 30f;
        [SerializeField] private float heavyAttackRadius = 2.5f;
        [SerializeField] private float heavyChargeDuration = 0.4f;

        [Header("Spore Shot Settings (Tecla Q - Mano Izquierda)")]
        [SerializeField] private GameObject sporeProjectilePrefab;
        [SerializeField] private Transform leftHandFirePoint;
        [SerializeField] private float sporeShotCost = 20f;
        [SerializeField] private float sporeSpawnDelay = 0.25f;
        [SerializeField] private float sporeShotRecovery = 0.35f;

        [Header("Adjustable Slash VFX")]
        [SerializeField] private GameObject slashVFXPrefab;
        [SerializeField] private Transform slashSpawnPoint;
        [SerializeField] private float slashVFXLifetime = 0.6f;

        [Header("Detection Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("References")]
        [SerializeField] private Animator animator;

        private int comboIndex = 0;
        private float lastAttackTime;
        private float lastStaminaConsumeTime;
        private bool isAttacking;
        private bool isInvulnerable;

        private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
        private static readonly int HeavyAttackTriggerHash = Animator.StringToHash("HeavyAttack");
        private static readonly int SporeShotTriggerHash = Animator.StringToHash("SporeShot");
        private static readonly int ComboStepHash = Animator.StringToHash("ComboStep");

        public float CurrentStamina => currentStamina;
        public float CurrentSporeCharge => currentSporeCharge;
        public bool IsAttacking => isAttacking;
        public bool IsInvulnerable => isInvulnerable;

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

            if (!isAttacking)
            {
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

                if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
                {
                    PerformSporeShot();
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
            if (value.isPressed) PerformLightAttack();
        }

        public void OnHeavyAttack(InputValue value)
        {
            if (value.isPressed) PerformHeavyAttack();
        }

        public void OnSporeShot(InputValue value)
        {
            if (value.isPressed) PerformSporeShot();
        }

        public void PerformLightAttack()
        {
            if (isAttacking || currentStamina < 10f) return;
            StartCoroutine(LightAttackRoutine());
        }

        public void PerformHeavyAttack()
        {
            if (isAttacking) return;
            if (currentStamina < heavyStaminaCost) return;
            StartCoroutine(HeavyAttackRoutine());
        }

        public void PerformSporeShot()
        {
            if (isAttacking || currentSporeCharge < sporeShotCost) return;
            StartCoroutine(SporeShotRoutine());
        }

        public void AddSporeCharge(float amount)
        {
            currentSporeCharge = Mathf.Min(maxSporeCharge, currentSporeCharge + amount);
        }

        public bool TryExecuteDash(float invulnerabilityDuration = 0.3f)
        {
            if (currentStamina < dashStaminaCost) return false;

            currentStamina -= dashStaminaCost;
            lastStaminaConsumeTime = Time.time;
            StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));
            return true;
        }

        private IEnumerator InvulnerabilityRoutine(float duration)
        {
            isInvulnerable = true;
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
        }

        private IEnumerator LightAttackRoutine()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            currentStamina = Mathf.Max(0f, currentStamina - 10f);
            lastStaminaConsumeTime = Time.time;

            if (animator != null)
            {
                animator.SetInteger(ComboStepHash, comboIndex);
                animator.SetTrigger(AttackTriggerHash);
            }

            // Dispara el SFX de corte de espada con variacion aleatoria
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySlash();

            SpawnSlashVFX(comboIndex);

            int damage = (comboIndex < comboDamages.Length) ? comboDamages[comboIndex] : 15;
            DealDamage(lightAttackRadius, damage);

            comboIndex = (comboIndex + 1) % comboDamages.Length;

            yield return new WaitForSeconds(attackStepDuration);
            isAttacking = false;
        }

        private IEnumerator HeavyAttackRoutine()
        {
            isAttacking = true;
            currentStamina = Mathf.Max(0f, currentStamina - heavyStaminaCost);
            lastStaminaConsumeTime = Time.time;

            if (animator != null)
            {
                animator.ResetTrigger(AttackTriggerHash);
                animator.SetTrigger(HeavyAttackTriggerHash);
            }

            yield return new WaitForSeconds(heavyChargeDuration);

            // Dispara el SFX de corte al ejecutar la estocada pesada
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySlash();

            SpawnSlashVFX(99);
            DealDamage(heavyAttackRadius, heavyDamage);

            yield return new WaitForSeconds(0.3f);
            isAttacking = false;
        }

        private IEnumerator SporeShotRoutine()
        {
            isAttacking = true;
            currentSporeCharge -= sporeShotCost;

            if (animator != null)
            {
                animator.ResetTrigger(AttackTriggerHash);
                animator.SetTrigger(SporeShotTriggerHash);
            }

            yield return new WaitForSeconds(sporeSpawnDelay);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySporeShot();
            SpawnSporeProjectileFromLeftHand();

            yield return new WaitForSeconds(sporeShotRecovery);
            isAttacking = false;
        }

        public void SpawnSporeProjectileFromLeftHand()
        {
            Vector3 spawnPos = leftHandFirePoint != null
                ? leftHandFirePoint.position
                : transform.position + transform.forward * 0.8f + Vector3.up * 1.2f - transform.right * 0.3f;

            Quaternion spawnRot = Quaternion.LookRotation(transform.forward, Vector3.up);

            if (sporeProjectilePrefab != null)
            {
                GameObject projObj = Instantiate(sporeProjectilePrefab, spawnPos, spawnRot);
                if (projObj.TryGetComponent<SporeProjectile>(out var sporeProj))
                {
                    sporeProj.Initialize(gameObject);
                }
            }
        }

        private void SpawnSlashVFX(int attackType)
        {
            if (slashVFXPrefab == null) return;

            Vector3 spawnPos = slashSpawnPoint != null ? slashSpawnPoint.position : (transform.position + transform.forward * 1.2f + Vector3.up * 1.1f);
            Quaternion spawnRot = transform.rotation;

            if (attackType == 0) spawnRot *= Quaternion.Euler(20f, 0f, -30f);
            else if (attackType == 1) spawnRot *= Quaternion.Euler(-10f, 0f, 35f);
            else if (attackType == 99) spawnRot *= Quaternion.Euler(0f, 0f, 90f);

            GameObject instance = Instantiate(slashVFXPrefab, spawnPos, spawnRot);
            Destroy(instance, slashVFXLifetime);
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

            if (leftHandFirePoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(leftHandFirePoint.position, 0.15f);
                Gizmos.DrawRay(leftHandFirePoint.position, transform.forward * 1.5f);
            }
        }
    }
}
