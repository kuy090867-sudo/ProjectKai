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

        [Header("Death")]
        [SerializeField] private float _deathFadeDuration = 0.6f;

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
        private Transform _spriteTransform;
        private Vector3 _baseSpriteScale;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null)
            {
                _originalColor = _sr.color;
                _originalMaterial = _sr.material;
                _spriteTransform = _sr.transform;
                _baseSpriteScale = _spriteTransform.localScale;
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

            // 시각 2: 스퀴시 효과 (피격 시 찌그러짐)
            StartCoroutine(HitSquashCoroutine());

            // 시각 3: 넉백
            if (knockbackForce > 0f)
                ApplyKnockback(knockbackDirection, knockbackForce);

            // 청각: 타격음
            Core.AudioManager.Instance?.PlaySFX("hit", 0.6f);

            // VFX: 타격 스파크
            Core.VFXManager.Instance?.HitEffect(transform.position);

            // 촉각: 히트스톱 + 카메라쉐이크
            if (!IsAlive)
            {
                // VFX: 사망 파편 폭발
                Color deathColor = _sr != null ? _originalColor : Color.white;
                Core.VFXManager.Instance?.EnemyDeathEffect(transform.position, deathColor);

                // === 사망 연출 (강화된 3채널) ===
                Core.AudioManager.Instance?.PlaySFX("enemy_death", 0.8f);
                Core.GameFeel.Instance?.CameraShake(0.25f, 0.3f);
                Core.GameFeel.Instance?.HitStop(0.15f);
                Core.GameFeel.Instance?.KillSlowMotion(0.5f, 0.15f);
                Core.GameFeel.Instance?.KillFlash(0.25f);

                // 사망 시각 연출: 페이드 아웃 + 콜라이더 비활성화
                StartCoroutine(DeathSequenceCoroutine());
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
            yield return new WaitForSecondsRealtime(_flashDuration);

            if (_sr != null && IsAlive)
                _sr.color = _originalColor;
        }

        /// <summary>
        /// 피격 시 스퀴시 효과 (넉백 방향으로 찌그러짐 → 복원)
        /// </summary>
        private IEnumerator HitSquashCoroutine()
        {
            if (_spriteTransform == null) yield break;

            float duration = 0.12f;
            float elapsed = 0f;
            Vector3 squash = new Vector3(
                _baseSpriteScale.x * 1.3f,
                _baseSpriteScale.y * 0.7f,
                _baseSpriteScale.z);

            // 찌그러짐
            while (elapsed < duration * 0.3f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration * 0.3f);
                _spriteTransform.localScale = Vector3.Lerp(_baseSpriteScale, squash, t);
                yield return null;
            }

            // 복원 (바운스)
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = (elapsed - duration * 0.3f) / (duration * 0.7f);
                _spriteTransform.localScale = Vector3.Lerp(squash, _baseSpriteScale, t);
                yield return null;
            }

            _spriteTransform.localScale = _baseSpriteScale;
        }

        /// <summary>
        /// 사망 시: 붉은 플래시 → 알파 페이드 → 콜라이더 비활성화 → Destroy
        /// </summary>
        private IEnumerator DeathSequenceCoroutine()
        {
            // 콜라이더 즉시 비활성화 (추가 피격 방지)
            var cols = GetComponents<Collider2D>();
            foreach (var c in cols) c.enabled = false;

            if (_sr == null) yield break;

            // 붉은 플래시
            _sr.color = new Color(1f, 0.2f, 0.2f, 1f);
            yield return new WaitForSecondsRealtime(0.1f);

            // 알파 페이드 아웃
            float elapsed = 0f;
            Color startColor = _sr.color;
            while (elapsed < _deathFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _deathFadeDuration;
                // 이징: 천천히 시작 → 빠르게 사라짐
                t = t * t;
                if (_sr != null)
                    _sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

                // 스프라이트 약간 위로 떠오름 (영혼 이탈 느낌)
                if (_spriteTransform != null)
                    _spriteTransform.position += Vector3.up * Time.unscaledDeltaTime * 0.5f;

                yield return null;
            }

            Destroy(gameObject);
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
                StartCoroutine(KinematicKnockback(direction.normalized, force * 0.12f));
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
            float duration = 0.18f;
            Vector2 start = _rb.position;
            Vector2 end = start + dir * distance;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
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
