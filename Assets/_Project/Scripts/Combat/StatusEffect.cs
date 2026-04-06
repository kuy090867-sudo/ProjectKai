using UnityEngine;
using System.Collections.Generic;

namespace ProjectKai.Combat
{
    public enum StatusType { Poison, Burn, Freeze, Stun }

    [System.Serializable]
    public class StatusEffect
    {
        public StatusType type;
        public float duration;
        public float damagePerTick;
        public float tickInterval;
        public float speedMultiplier = 1f;
    }

    /// <summary>
    /// 상태이상 핸들러. 독/화상/빙결/스턴 처리.
    /// </summary>
    public class StatusEffectHandler : MonoBehaviour
    {
        private List<ActiveStatus> _activeEffects = new List<ActiveStatus>();
        private SpriteRenderer _sr;
        private Color _originalColor;

        public float SpeedMultiplier { get; private set; } = 1f;
        public bool IsStunned { get; private set; }

        private class ActiveStatus
        {
            public StatusEffect effect;
            public float remaining;
            public float tickTimer;
        }

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null) _originalColor = _sr.color;
        }

        public void ApplyStatus(StatusEffect effect)
        {
            _activeEffects.Add(new ActiveStatus
            {
                effect = effect,
                remaining = effect.duration,
                tickTimer = 0f
            });
        }

        private void Update()
        {
            SpeedMultiplier = 1f;
            IsStunned = false;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var active = _activeEffects[i];
                active.remaining -= Time.deltaTime;

                if (active.remaining <= 0f)
                {
                    _activeEffects.RemoveAt(i);
                    continue;
                }

                // 틱 데미지
                if (active.effect.damagePerTick > 0f)
                {
                    active.tickTimer += Time.deltaTime;
                    if (active.tickTimer >= active.effect.tickInterval)
                    {
                        active.tickTimer -= active.effect.tickInterval;
                        var dmg = GetComponent<IDamageable>();
                        if (dmg != null && dmg.IsAlive)
                            dmg.TakeDamage(active.effect.damagePerTick, Vector2.zero, 0f);
                    }
                }

                // 속도 변경
                SpeedMultiplier *= active.effect.speedMultiplier;

                // 스턴/빙결
                if (active.effect.type == StatusType.Stun || active.effect.type == StatusType.Freeze)
                    IsStunned = true;
            }

            // 시각 효과
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_sr == null) return;

            if (_activeEffects.Count == 0)
            {
                _sr.color = _originalColor;
                return;
            }

            var latest = _activeEffects[_activeEffects.Count - 1];
            switch (latest.effect.type)
            {
                case StatusType.Poison: _sr.color = Color.Lerp(_originalColor, Color.green, 0.3f); break;
                case StatusType.Burn: _sr.color = Color.Lerp(_originalColor, new Color(1f, 0.5f, 0f), 0.4f); break;
                case StatusType.Freeze: _sr.color = Color.Lerp(_originalColor, Color.cyan, 0.5f); break;
                case StatusType.Stun: _sr.color = Color.Lerp(_originalColor, Color.yellow, 0.3f); break;
            }
        }
    }
}
