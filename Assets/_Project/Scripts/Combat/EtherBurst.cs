using UnityEngine;
using ProjectKai.Core;

namespace ProjectKai.Combat
{
    /// <summary>
    /// 에테르 폭발 스킬. K키로 발동. MP 20 소모.
    /// 플레이어 주변 원형 범위에 데미지.
    /// </summary>
    public class EtherBurst : MonoBehaviour
    {
        [SerializeField] private float _damage = 30f;
        [SerializeField] private float _radius = 3f;
        [SerializeField] private float _manaCost = 20f;
        [SerializeField] private float _cooldown = 3f;
        [SerializeField] private float _knockback = 10f;

        private float _lastUseTime = -999f;

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.kKey.wasPressedThisFrame == true)
            {
                TryUse();
            }
        }

        public void TryUse()
        {
            if (Time.time < _lastUseTime + _cooldown) return;
            if (ManaSystem.Instance == null || !ManaSystem.Instance.UseMana(_manaCost)) return;

            _lastUseTime = Time.time;
            Explode();
        }

        private void Explode()
        {
            // 이펙트
            GameFeel.Instance?.CameraShake(0.15f, 0.2f);
            GameFeel.Instance?.HitStop(0.06f);
            AudioManager.Instance?.PlaySFX("sword_hit", 0.8f);

            // STR 보너스
            float bonusDmg = 0f;
            if (ProgressionSystem.Instance != null)
                bonusDmg = ProgressionSystem.Instance.STR * 1.5f;

            // 범위 내 적에게 데미지
            var hits = Physics2D.OverlapCircleAll(transform.position, _radius);
            int hitCount = 0;

            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;

                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    damageable.TakeDamage(_damage + bonusDmg, dir, _knockback);
                    hitCount++;
                    UI.ComboCounter.Instance?.AddHit();
                }
            }

            if (hitCount > 0)
                Debug.Log($"[EtherBurst] {hitCount}마리에게 {_damage + bonusDmg:F0} 데미지!");
        }
    }
}
