using System.Collections;
using UnityEngine;
using KingLucilia.Core;

namespace KingLucilia.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class SporeProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 16f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private int impactDamage = 5;

        [Header("Poison DoT Settings (GDD: 10 DoT / 5s)")]
        [SerializeField] private int dotDamagePerTick = 2;
        [SerializeField] private float dotDuration = 5f;
        [SerializeField] private float tickInterval = 1f;

        [Header("VFX")]
        [SerializeField] private GameObject hitVFXPrefab;

        private GameObject owner;
        private bool hasHit = false;

        public void Initialize(GameObject shooter)
        {
            owner = shooter;
            Destroy(gameObject, lifetime);
        }

        private void Awake()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            SphereCollider col = GetComponent<SphereCollider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            if (!hasHit)
            {
                transform.position += transform.forward * speed * Time.deltaTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;
            if (other.gameObject == owner || other.CompareTag("Player")) return;
            if (other.isTrigger) return;

            if (other.TryGetComponent<IDamageable>(out var victim))
            {
                hasHit = true;
                victim.TakeDamage(impactDamage, transform.position, transform.forward);
                Debug.Log($"<color=green>[Disparo de Esporas]</color> Impacto directo en {other.name}. Aplicando DoT fúngico...");

                if (hitVFXPrefab != null)
                {
                    Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
                }

                // Ocultar malla y colisionador mientras corre la corrutina de veneno (DoT)
                foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
                GetComponent<Collider>().enabled = false;

                StartCoroutine(ApplyPoisonDoT(victim));
            }
            else
            {
                // Chocó contra pared u obstáculo del escenario
                hasHit = true;
                if (hitVFXPrefab != null)
                {
                    Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }

        private IEnumerator ApplyPoisonDoT(IDamageable victim)
        {
            float elapsed = 0f;
            while (elapsed < dotDuration && victim != null && !victim.IsDead)
            {
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;

                if (victim != null && !victim.IsDead)
                {
                    victim.TakeDamage(dotDamagePerTick, transform.position, Vector3.zero);
                    Debug.Log($"<color=lime>[Veneno de Esporas DoT]</color> +{dotDamagePerTick} daño fúngico ({elapsed}s/{dotDuration}s)");
                }
            }
            Destroy(gameObject);
        }
    }
}
