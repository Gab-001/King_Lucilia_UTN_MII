using System.Collections;
using UnityEngine;
using KingLucilia.Core;
using KingLucilia.Player;

namespace KingLucilia.Enemies
{
    public class EnemyExploder : EnemyBase
    {
        [Header("Explosion Mechanics")]
        [SerializeField] private float fuseTime = 3.5f;
        [SerializeField] private float explosionRadius = 4.5f;
        [SerializeField] private int explosionDamage = 35;
        [SerializeField] private GameObject explosionGasVFX;
        [SerializeField] private Color fuseBlinkWarningColor = new Color(0.9f, 0.2f, 0.0f);

        private bool hasDetonated = false;
        private Coroutine explosionCountdownRoutine;

        public override void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection)
        {
            base.TakeDamage(damageAmount, hitPoint, hitDirection);

            if (IsDead && !hasDetonated && explosionCountdownRoutine == null)
            {
                explosionCountdownRoutine = StartCoroutine(ExplosionCountdownRoutine());
            }
        }

        private IEnumerator ExplosionCountdownRoutine()
        {
            float elapsed = 0f;
            var skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            Material mat = skinnedMesh != null ? skinnedMesh.material : null;
            Color originalEmission = mat != null && mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

            while (elapsed < fuseTime)
            {
                if (mat != null && mat.HasProperty("_EmissionColor"))
                {
                    float pingPong = Mathf.PingPong(elapsed * 6f, 1f);
                    mat.SetColor("_EmissionColor", Color.Lerp(originalEmission, fuseBlinkWarningColor * 2.5f, pingPong));
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            DetonateGasBomb();
        }

        private void DetonateGasBomb()
        {
            if (hasDetonated) return;
            hasDetonated = true;

            Debug.Log("<color=red>[Explosión Cáustica]</color> El vientre del Famélico ha detonado en gas tóxico.");

            if (explosionGasVFX != null)
            {
                Instantiate(explosionGasVFX, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player") || hit.GetComponent<KingLuciliaController>() != null)
                {
                    if (hit.TryGetComponent<IDamageable>(out var target))
                    {
                        Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                        target.TakeDamage(explosionDamage, hit.transform.position, pushDir);
                    }
                }
            }

            Destroy(gameObject, 0.1f);
        }

        public override void Interact(KingLuciliaController player)
        {
            if (explosionCountdownRoutine != null)
            {
                StopCoroutine(explosionCountdownRoutine);
            }

            hasDetonated = true;
            base.Interact(player);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
