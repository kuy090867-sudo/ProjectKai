using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    /// <summary>
    /// 잠긴 문. 해당 구역의 모든 적을 처치하면 열림.
    /// 스테이지 내 구간 분리용.
    /// </summary>
    public class LockedDoor : MonoBehaviour
    {
        [SerializeField] private string _doorName = "문";
        [SerializeField] private int _requiredKills = 0;

        private BoxCollider2D _collider;
        private SpriteRenderer _sr;
        private int _currentKills;
        private bool _opened;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _sr = GetComponent<SpriteRenderer>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider2D>();
                _collider.size = new Vector2(1f, 3f);
            }
        }

        private void Start()
        {
            if (_requiredKills <= 0)
            {
                // 자동으로 씬의 현재 적 수 감지
                _requiredKills = GameObject.FindGameObjectsWithTag("Enemy").Length;
            }

            // 모든 적의 사망 이벤트 구독
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                var dr = e.GetComponent<DamageReceiver>();
                if (dr != null)
                    dr.OnDeath += OnEnemyKilled;
            }
        }

        private void OnEnemyKilled()
        {
            _currentKills++;
            if (_currentKills >= _requiredKills && !_opened)
            {
                Open();
            }
        }

        private void Open()
        {
            _opened = true;
            if (_collider != null) _collider.enabled = false;
            if (_sr != null) _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 0.3f);
            AudioManager.Instance?.PlaySFX("jump", 0.6f);
            Debug.Log($"[LockedDoor] {_doorName} 열림!");
        }
    }
}
