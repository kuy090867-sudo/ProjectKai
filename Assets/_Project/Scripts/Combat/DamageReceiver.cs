using UnityEngine;
using System;
using System.Collections;

namespace ProjectKai.Combat
{
    public class DamageReceiver : MonoBehaviour, IDamageable, IKnockbackable
    {
        [SerializeField] private float _maxHealth = 100f;

        [Header("Hit Reaction")]
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private float _iFrameDuration = 0.5f;
        [SerializeField] private float _iFrameBlinkInterval = 0.05f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<float, Vector2> OnDamaged;

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Color _originalColor;
        private bool _isInvincible;
        private Material _originalMaterial;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null)
            {
                _originalColor = _sr.color;
                _originalMaterial = _sr.material;
            }
        }

        public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce)
        {
            if (!IsAlive || _isInvincible) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(CurrentHealth, 0f);

            OnDamaged?.Invoke(damage, knockbackDirection);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            // === 피격 리액션 (3채널: 시각+청각+촉각) ===

            // 시각 1: 흰색 플래시
            StartCoroutine(HitFlashCoroutine());

            // 시각 2: 넉백
            if (knockbackForce > 0f)
                ApplyKnockback(knockbackDirection, knockbackForce);

            // 청각: 타격음
            Core.AudioManager.Instance?.PlaySFX("hit", 0.6f);

            // 촉각: 히트스톱 + 카메라쉐이크
            if (!IsAlive)
            {
                // === 사망 연출 (강화된 3채널) ===
                Core.AudioManager.Instance?.PlaySFX("enemy_death", 0.8f);
                Core.GameFeel.Instance?.CameraShake(0.2f, 0.25f);
                Core.GameFeel.Instance?.HitStop(0.15f);
                Core.GameFeel.Instance?.KillSlowMotion(0.4f, 0.2f);
                OnDeath?.Invoke();
            }
            else
            {
                // 일반 피격
                Core.GameFeel.Instance?.CameraShake(0.08f, 0.1f);
                Core.GameFeel.Instance?.HitStop(0.03f);

                // 무적 프레임
                StartCoroutine(InvincibilityCoroutine());
            }
        }

        /// <summary>
        /// 피격 시 흰색 플래시 (스프라이트 색상 전환)
        /// </summary>
        private IEnumerator HitFlashCoroutine()
        {
            if (_sr == null) yield break;

            _sr.color = Color.white;
            yield return new WaitForSeconds(_flashDuration);

            if (_sr != null && IsAlive)
                _sr.color = _originalColor;
        }

        /// <summary>
        /// 무적 프레임 + 스프라이트 깜빡임
        /// </summary>
        private IEnumerator InvincibilityCoroutine()
        {
            _isInvincible = true;
            float timer = 0f;

            while (timer < _iFrameDuration)
            {
                if (_sr != null)
                {
                    // 깜빡임: 보였다 안 보였다
                    _sr.enabled = !_sr.enabled;
                }
                yield return new WaitForSeconds(_iFrameBlinkInterval);
                timer += _iFrameBlinkInterval;
            }

            if (_sr != null)
            {
                _sr.enabled = true;
                _sr.color = _originalColor;
            }
            _isInvincible = false;
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            if (_rb == null) return;

            if (_rb.bodyType == RigidbodyType2D.Kinematic)
            {
                // Kinematic 적: MovePosition으로 넉백
                StartCoroutine(KinematicKnockback(direction.normalized, force * 0.1f));
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            }
        }

        private IEnumerator KinematicKnockback(Vector2 dir, float distance)
        {
            float elapsed = 0f;
            float duration = 0.15f;
            Vector2 start = _rb.position;
            Vector2 end = start + dir * distance;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 빠르게 밀린 후 감속 (EaseOut)
                t = 1f - (1f - t) * (1f - t);
                _rb.MovePosition(Vector2.Lerp(start, end, t));
                yield return null;
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }
    }
}
