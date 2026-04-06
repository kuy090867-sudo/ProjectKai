using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    /// <summary>
    /// 바닥 가시 함정. 접촉 시 데미지.
    /// DungeonTilesetII의 floor_spikes 스프라이트 사용.
    /// </summary>
    public class FloorSpikes : MonoBehaviour
    {
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _cooldown = 1f;
        private float _lastHitTime = -999f;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Time.time < _lastHitTime + _cooldown) return;

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(_damage, Vector2.up, 3f);
                _lastHitTime = Time.time;
                AudioManager.Instance?.PlaySFX("hit", 0.5f);
            }
        }
    }
}
