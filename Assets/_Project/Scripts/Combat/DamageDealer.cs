using UnityEngine;

namespace ProjectKai.Combat
{
    public class DamageDealer : MonoBehaviour
    {
        private float _damage;
        private float _knockbackForce;
        private Vector2 _direction;
        private LayerMask _targetLayer;
        private bool _isActive;

        public void Activate(float damage, float knockbackForce, Vector2 direction, LayerMask targetLayer)
        {
            _damage = damage;
            _knockbackForce = knockbackForce;
            _direction = direction;
            _targetLayer = targetLayer;
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        /// <summary>
        /// 지정된 영역에서 히트 판정을 수행
        /// </summary>
        public int PerformHitCheck(Vector2 origin, Vector2 size)
        {
            if (!_isActive) return 0;

            int hitCount = 0;
            var hits = Physics2D.OverlapBoxAll(origin, size, 0f, _targetLayer);

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    damageable.TakeDamage(_damage, _direction, _knockbackForce);
                    hitCount++;
                    UI.ComboCounter.Instance?.AddHit();
                }
            }

            return hitCount;
        }
    }
}
