using UnityEngine;

namespace ProjectKai.Combat
{
    public interface IDamageable
    {
        void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce);
        bool IsAlive { get; }
    }

    public interface IKnockbackable
    {
        void ApplyKnockback(Vector2 direction, float force);
    }
}
