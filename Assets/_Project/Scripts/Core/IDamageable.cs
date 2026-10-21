using UnityEngine;

namespace KingLucilia.Core
{
    public interface IDamageable
    {
        void TakeDamage(int damageAmount, Vector3 hitPoint, Vector3 hitDirection);
        bool IsDead { get; }
    }
}
