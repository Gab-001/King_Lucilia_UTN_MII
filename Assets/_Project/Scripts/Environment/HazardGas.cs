using UnityEngine;
using KingLucilia.Core;

namespace KingLucilia.Environment
{
    [RequireComponent(typeof(Collider))]
    public class HazardGas : MonoBehaviour
    {
        [Header("Hazard Settings")]
        [SerializeField] private int damagePerTick = 5;
        [SerializeField] private float damageInterval = 0.5f;

        private float nextDamageTime;

        private void Reset()
        {
            // Asegura que el collider actúe siempre como trigger
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time >= nextDamageTime)
            {
                if (other.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damagePerTick, other.transform.position, Vector3.up);
                    Debug.Log($"<color=orange>[Hazard]</color> {other.gameObject.name} recibió {damagePerTick} de daño por gas cáustico.");
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}
