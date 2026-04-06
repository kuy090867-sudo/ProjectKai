using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    /// <summary>
    /// 적 스폰 포인트. 플레이어가 범위 내에 들어오면 적 생성.
    /// 한번만 스폰 (일회성).
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private float _triggerRange = 8f;
        [SerializeField] private int _enemyCount = 2;

        private bool _spawned;
        private Transform _player;

        private void Update()
        {
            if (_spawned) return;

            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _player = p.transform;
                return;
            }

            if (Vector2.Distance(transform.position, _player.position) < _triggerRange)
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            _spawned = true;
            AudioManager.Instance?.PlaySFX("enemy_death", 0.3f);
            GameFeel.Instance?.CameraShake(0.05f, 0.1f);

            for (int i = 0; i < _enemyCount; i++)
            {
                float offsetX = Random.Range(-2f, 2f);
                var enemyObj = new GameObject($"SpawnedEnemy_{i}");
                enemyObj.transform.position = transform.position + new Vector3(offsetX, 0f, 0f);
                enemyObj.tag = "Enemy";

                // 기본 고블린 적 생성
                var sr = new GameObject("Sprite");
                sr.transform.SetParent(enemyObj.transform);
                sr.transform.localPosition = Vector3.zero;
                var renderer = sr.AddComponent<SpriteRenderer>();
                renderer.color = new Color(0.5f, 0.8f, 0.4f);

                var rb = enemyObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;

                var col = enemyObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.5f, 0.8f);

                var dr = enemyObj.AddComponent<DamageReceiver>();
                enemyObj.AddComponent<Enemy.EnemyBase>();
                enemyObj.AddComponent<EnemyReward>();

                // StageManager에 스폰된 적 등록
                if (StageManager.Instance != null)
                    StageManager.Instance.RegisterSpawnedEnemy(dr);

                // 체력바
                UI.HealthBarUI.CreateHealthBar(
                    enemyObj.transform, 50f,
                    new Vector3(0f, 1f, 0f),
                    new Color(0.9f, 0.2f, 0.2f),
                    new Vector2(60f, 8f));

                var anim = sr.AddComponent<SpriteAnimator>();
                // SpriteAnimator가 Awake에서 자동 로드
            }

            Debug.Log($"[SpawnPoint] {_enemyCount}마리 스폰!");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _triggerRange);
        }
    }
}
