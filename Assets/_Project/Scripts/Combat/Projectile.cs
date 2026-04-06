using UnityEngine;

namespace ProjectKai.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private LayerMask _hitLayer;

        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _knockbackForce;
        private Rigidbody2D _rb;

        public void Initialize(Vector2 direction, float speed, float damage, float knockbackForce)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _knockbackForce = knockbackForce;

            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.linearVelocity = _direction * _speed;

            // 방향에 따라 스프라이트 뒤집기
            if (_direction.x < 0f)
            {
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.flipX = true;
            }

            Destroy(gameObject, _lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _hitLayer) == 0) return;

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(_damage, _direction, _knockbackForce);
            }

            Destroy(gameObject);
        }
    }
}
