using UnityEngine;
using KingLucilia.Core;

namespace KingLucilia.Environment
{
    [RequireComponent(typeof(Collider))]
    public class KillzoneTrigger : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int lethalDamage = 999;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Detectar directamente por IDamageable
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                if (!damageable.IsDead)
                {
                    Debug.Log($"<color=red>[Killzone]</color> {other.gameObject.name} cayó a la fosa/abismo.");
                    damageable.TakeDamage(lethalDamage, other.transform.position, Vector3.up);
                }
            }
            // Respaldo si el collider está en un objeto hijo
            else
            {
                var parentDamageable = other.GetComponentInParent<IDamageable>();
                if (parentDamageable != null && !parentDamageable.IsDead)
                {
                    Debug.Log($"<color=red>[Killzone]</color> {other.transform.root.name} cayó a la fosa/abismo.");
                    parentDamageable.TakeDamage(lethalDamage, other.transform.position, Vector3.up);
                }
            }
        }
    }
}
