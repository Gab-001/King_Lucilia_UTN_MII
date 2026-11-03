using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using KingLucilia.Core;
using KingLucilia.Player;

namespace KingLucilia.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class ColossusBoss : MonoBehaviour, IDamageable
    {
        public enum BossState { Dormant, Chase, StompAoE, SlamAttack, Defeated }

        [Header("Stats")]
        [SerializeField] private int maxHealth = 250;
        [SerializeField] private int currentHealth;
        [SerializeField] private float attackRange = 3.5f;
        [SerializeField] private float stompCooldown = 6.0f;
        [SerializeField] private float regularAttackCooldown = 3.0f;

        [Header("AoE Stomp Settings")]
        [SerializeField] private float stompRadius = 5.0f;
        [SerializeField] private int stompDamage = 25;
        [SerializeField] private GameObject stompVFXPrefab;

        [Header("Slam Hitbox Settings")]
        [SerializeField] private Transform slamSocket;
        [SerializeField] private float slamRadius = 1.5f;
        [SerializeField] private int slamDamage = 20;

        [Header("Targeting")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private LayerMask playerLayer = ~0;

        [Header("Debug & Visual Feedback")]
        [SerializeField] private SkinnedMeshRenderer bossRenderer;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.15f;

        private NavMeshAgent agent;
        private Animator animator;
        private CapsuleCollider col;
        private BossState currentState = BossState.Dormant;
        private float nextStompTime;
        private float nextAttackTime;
        private bool isActing = false;

        private Material[] originalMaterials;
        private Coroutine flashRoutine;
        private Vector3 lastHitPoint;
        private Vector3 lastHitDirection;

        public bool IsDead => currentState == BossState.Defeated;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            col = GetComponent<CapsuleCollider>();
            currentHealth = maxHealth;

            if (bossRenderer == null)
            {
                bossRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (bossRenderer != null)
            {
                originalMaterials = bossRenderer.materials;
            }

            if (agent != null)
            {
                agent.stoppingDistance = attackRange - 0.5f;
                agent.autoBraking = true;
            }

            if (playerTarget == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTarget = player.transform;
            }
        }

        private void Start()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
        }

        public void WakeUp()
        {
            if (currentState != BossState.Dormant) return;

            currentState = BossState.Chase;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }

            nextStompTime = Time.time + 3.0f;
            Debug.Log("<color=red>[Coloso]</color> El Coloso Hinchado se ha despertado y comienza la persecución.");
        }

        private void Update()
        {
            if (currentState == BossState.Dormant || currentState == BossState.Defeated)
            {
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
                return;
            }

            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
            if (playerTarget == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (!isActing)
            {
                if (Time.time >= nextStompTime && distanceToPlayer <= stompRadius + 1f)
                {
                    StartCoroutine(ExecuteStompRoutine());
                }
                else if (distanceToPlayer <= attackRange)
                {
                    if (Time.time >= nextAttackTime)
                    {
                        StartCoroutine(ExecuteSlamRoutine());
                    }
                    else
                    {
                        FrenarYMirarAlJugador();
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(playerTarget.position);
                    if (animator != null) animator.SetFloat("Speed", agent.velocity.magnitude);
                }
            }
            else
            {
                FrenarYMirarAlJugador();
            }
        }

        private void FrenarYMirarAlJugador()
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                if (agent.hasPath) agent.ResetPath();
            }
            if (animator != null) animator.SetFloat("Speed", 0f);

            Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 5f * Time.deltaTime);
            }
        }

        private IEnumerator ExecuteStompRoutine()
        {
            isActing = true;
            currentState = BossState.StompAoE;
            FrenarYMirarAlJugador();

            if (animator != null) animator.SetTrigger("Stomp");

            yield return new WaitForSeconds(0.6f);

            if (stompVFXPrefab != null)
            {
                Instantiate(stompVFXPrefab, transform.position, Quaternion.identity);
            }

            Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, stompRadius, playerLayer);
            foreach (var target in targetsInRadius)
            {
                if (target.CompareTag("Player") || target.GetComponent<KingLuciliaController>() != null)
                {
                    if (target.TryGetComponent<IDamageable>(out var damageable))
                    {
                        damageable.TakeDamage(stompDamage, transform.position, (target.transform.position - transform.position).normalized);
                        Debug.Log($"<color=red>[Coloso AoE]</color> Pisoteo impactó en Lucilia por {stompDamage} de daño.");
                    }
                }
            }

            yield return new WaitForSeconds(0.8f);
            nextStompTime = Time.time + stompCooldown;
            currentState = BossState.Chase;
            isActing = false;
        }

        private IEnumerator ExecuteSlamRoutine()
        {
            isActing = true;
            currentState = BossState.SlamAttack;
            FrenarYMirarAlJugador();

            if (animator != null) animator.SetTrigger("Attack");

            yield return new WaitForSeconds(0.5f);

            Vector3 hitCenter = slamSocket != null ? slamSocket.position : transform.position + transform.forward * 2.0f + Vector3.up * 1f;
            Collider[] hits = Physics.OverlapSphere(hitCenter, slamRadius, playerLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") || hit.GetComponent<KingLuciliaController>() != null)
                {
                    if (hit.TryGetComponent<IDamageable>(out var victim))
                    {
                        victim.TakeDamage(slamDamage, hitCenter, transform.forward);
                        Debug.Log($"<color=red>[Coloso Golpe]</color> Machaque conectó en Lucilia por {slamDamage} de daño.");
                    }
                }
            }

            yield return new WaitForSeconds(0.7f);
            nextAttackTime = Time.time + regularAttackCooldown;
            currentState = BossState.Chase;
            isActing = false;
        }

        // --- SISTEMA DE DAÑO Y AUDITORÍA DEBUG ---
        public void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead)
            {
                Debug.LogWarning($"<color=yellow>[Boss Debug]</color> Se intentó dañar al Coloso pero ya está derrotado (HP: {currentHealth}).");
                return;
            }

            if (currentState == BossState.Dormant)
            {
                Debug.Log("<color=yellow>[Boss Debug]</color> Daño recibido en reposo: Despertando jefe automáticamente.");
                WakeUp();
            }

            currentHealth = Mathf.Max(0, currentHealth - damageAmount);
            lastHitPoint = hitPoint;
            lastHitDirection = hitDirection;

            float healthPercentage = ((float)currentHealth / maxHealth) * 100f;

            Debug.Log($"<color=orange>[Boss Damage Hit]</color> Coloso recibió <b>{damageAmount}</b> de daño en {hitPoint}. " +
                      $"Salud restante: <b>{currentHealth}/{maxHealth}</b> ({healthPercentage:F1}%).");

            Debug.DrawRay(hitPoint, hitDirection * 1.5f, Color.magenta, 1.0f);

            // Flash visual reactivo
            if (bossRenderer != null && gameObject.activeInHierarchy)
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(DamageFlashRoutine());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (bossRenderer == null) yield break;

            foreach (var mat in bossRenderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", damageFlashColor);
                else if (mat.HasProperty("_Color"))
                    mat.color = damageFlashColor;
            }

            yield return new WaitForSeconds(flashDuration);

            // Restaurar colores originales
            if (originalMaterials != null && bossRenderer != null)
            {
                bossRenderer.materials = originalMaterials;
            }
        }

        private void Die()
        {
            currentState = BossState.Defeated;
            StopAllCoroutines();

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetTrigger("Die");
            }

            Debug.Log("<color=green>[Victoria]</color> ¡El Coloso Hinchado ha caído!");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stompRadius);

            Gizmos.color = Color.red;
            Vector3 hitCenter = slamSocket != null ? slamSocket.position : transform.position + transform.forward * 2.0f + Vector3.up * 1f;
            Gizmos.DrawWireSphere(hitCenter, slamRadius);

            if (lastHitPoint != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(lastHitPoint, 0.2f);
            }
        }
    }
}
