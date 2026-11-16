using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using KingLucilia.Core;
using KingLucilia.Player;

namespace KingLucilia.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class EnemyBase : MonoBehaviour, IDamageable, IInteractable
    {
        public enum EnemyState { Chase, Attack, HuskDormant }

        [Header("Stats")]
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private int currentHealth;
        [SerializeField] private float attackRange = 2.0f;
        [SerializeField] private float attackCooldown = 2.2f;
        [SerializeField] private int attackDamage = 15;

        [Header("Hand Hitbox Settings")]
        [SerializeField] private Transform handHitPoint;
        [SerializeField] private float hitRadius = 0.6f;
        [SerializeField] private LayerMask playerLayer = ~0;

        [Header("Charred / Husk Effect")]
        [SerializeField] private SkinnedMeshRenderer meshRenderer;
        [SerializeField] private Material charredMaterial;
        [SerializeField] private GameObject sporeDissolveVFXPrefab;
        [SerializeField] private float destroyDelay = 0.2f;

        [Header("Player Tracking")]
        [SerializeField] private Transform playerTarget;

        private NavMeshAgent agent;
        private Animator animator;
        private CapsuleCollider col;
        private EnemyState currentState = EnemyState.Chase;
        private float nextAttackTime;
        private bool isHarvested = false;
        private bool isAttacking = false;

        public bool IsDead => currentState == EnemyState.HuskDormant;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            col = GetComponent<CapsuleCollider>();
            currentHealth = maxHealth;

            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (playerTarget == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTarget = player.transform;
            }

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.stoppingDistance = Mathf.Max(0.5f, attackRange - 0.2f);
                agent.autoBraking = true;
            }
        }

        private void Update()
        {
            if (currentState == EnemyState.HuskDormant) return;
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
            if (playerTarget == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            switch (currentState)
            {
                case EnemyState.Chase:
                    HandleChase(distanceToPlayer);
                    break;
                case EnemyState.Attack:
                    HandleAttack(distanceToPlayer);
                    break;
            }
        }

        private void HandleChase(float distance)
        {
            if (distance <= attackRange)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                if (agent.hasPath) agent.ResetPath();
                if (animator != null) animator.SetFloat("Speed", 0f);

                currentState = EnemyState.Attack;
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);

            if (animator != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
        }

        private void HandleAttack(float distance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            if (agent.hasPath) agent.ResetPath();

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }

            Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), 8f * Time.deltaTime);
            }

            if (Time.time >= nextAttackTime && !isAttacking)
            {
                StartCoroutine(AttackCycleRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }

            if (distance > attackRange + 0.8f && !isAttacking)
            {
                currentState = EnemyState.Chase;
            }
        }

        private IEnumerator AttackCycleRoutine()
        {
            isAttacking = true;

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                int attackType = Random.Range(0, 2);
                animator.SetInteger("AttackIndex", attackType);
                animator.SetTrigger("Attack");
            }

            yield return new WaitForSeconds(1.1f);
            isAttacking = false;
        }

        public void TriggerHandHit()
        {
            if (currentState == EnemyState.HuskDormant) return;

            Vector3 hitCenter = handHitPoint != null ? handHitPoint.position : transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
            Collider[] hits = Physics.OverlapSphere(hitCenter, hitRadius, playerLayer);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") || hit.GetComponent<KingLuciliaController>() != null)
                {
                    if (hit.TryGetComponent<IDamageable>(out var victim))
                    {
                        victim.TakeDamage(attackDamage, hitCenter, transform.forward);
                        Debug.Log($"<color=red>[Famélico Impacto]</color> Zarpazo conectó en Lucilia por {attackDamage} de daño.");
                        break;
                    }
                }
            }
        }

        public virtual void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (IsDead) return;

            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                TurnIntoCharredHusk();
            }
        }

        private void TurnIntoCharredHusk()
        {
            currentState = EnemyState.HuskDormant;
            StopAllCoroutines();

            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                agent.enabled = false;
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetTrigger("Die");
            }

            if (meshRenderer != null && charredMaterial != null)
            {
                meshRenderer.material = charredMaterial;
            }

            int interactableLayerIndex = LayerMask.NameToLayer("Interactable");
            if (interactableLayerIndex != -1)
            {
                SetLayerRecursively(gameObject, interactableLayerIndex);
            }

            try
            {
                tag = "Interactable";
            }
            catch (UnityException) { }

            if (col != null)
            {
                col.direction = 1;
                col.height = 1.8f;
                col.radius = 0.4f;
                col.center = new Vector3(0, 0.9f, 0);
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public string GetInteractionPrompt() => "Presiona [E] para liberar con esporas";

        public virtual void Interact(KingLuciliaController player)
        {
            if (!IsDead || isHarvested) return;

            isHarvested = true;

            if (sporeDissolveVFXPrefab != null)
            {
                Instantiate(sporeDissolveVFXPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            }

            Destroy(gameObject, destroyDelay);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Vector3 hitCenter = handHitPoint != null ? handHitPoint.position : transform.position + transform.forward * 1.0f + Vector3.up * 1.0f;
            Gizmos.DrawWireSphere(hitCenter, hitRadius);
        }
    }
}
