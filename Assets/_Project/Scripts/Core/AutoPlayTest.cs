using UnityEngine;
using ProjectKai.Player;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    /// <summary>
    /// QA 자동 전투 테스트. Play 시 자동 실행.
    /// 녹화 연동 (QARecorder).
    /// 테스트 후 삭제할 것.
    /// </summary>
    public class AutoPlayTest : MonoBehaviour
    {
        private PlayerController _player;
        private float _attackTimer;
        private int _killCount;
        private bool _testComplete;
        private float _testTime;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
            if (_player == null) { enabled = false; return; }

            // Play 누른 직후 → 녹화 시작
            QARecorder.Instance?.StartRecording();
            Debug.Log("[QA] === 자동 전투 테스트 시작 (녹화 중) ===");
        }

        private void Update()
        {
            if (_testComplete || _player == null || !_player.IsAlive) return;
            _testTime += Time.deltaTime;
            if (_testTime > 30f) { EndTest("타임아웃"); return; }

            Transform nearest = FindNearest();
            if (nearest == null) { EndTest($"전체 처치! {_killCount}마리 {_testTime:F1}초"); return; }

            float dist = nearest.position.x - _player.transform.position.x;
            float dir = Mathf.Sign(dist);
            var fp = typeof(PlayerController).GetProperty("FacingDirection");
            fp?.SetValue(_player, dir > 0 ? 1 : -1);
            if (_player.SpriteRenderer != null) _player.SpriteRenderer.flipX = dir < 0;

            if (Mathf.Abs(dist) > 1.5f)
            {
                _player.Rb.linearVelocity = new Vector2(dir * _player.MoveSpeed, _player.Rb.linearVelocity.y);
                _player.SpriteAnim?.Play("run");
            }
            else
            {
                _player.Rb.linearVelocity = new Vector2(0, _player.Rb.linearVelocity.y);
                _attackTimer += Time.deltaTime;
                if (_attackTimer >= 0.45f)
                {
                    _attackTimer = 0f;
                    Attack(nearest);
                }
            }
        }

        private void Attack(Transform target)
        {
            float dir = Mathf.Sign(target.position.x - _player.transform.position.x);
            Vector2 origin = (Vector2)_player.transform.position + new Vector2(0.8f * dir, 0f);
            _player.SpriteAnim?.ForcePlay("attack");
            AudioManager.Instance?.PlaySFX("sword_swing", 0.5f);

            var hits = Physics2D.OverlapBoxAll(origin, new Vector2(1.5f, 1f), 0f);
            foreach (var hit in hits)
            {
                var d = hit.GetComponent<IDamageable>();
                if (d != null && d.IsAlive && hit.transform != _player.transform)
                {
                    d.TakeDamage(25f, new Vector2(dir, 0.3f).normalized, 5f);
                    if (!d.IsAlive) { _killCount++; Debug.Log($"[QA] {hit.name} 처치! ({_killCount}마리)"); }
                }
            }
        }

        private Transform FindNearest()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform n = null; float min = float.MaxValue;
            foreach (var e in enemies)
            {
                var dr = e.GetComponent<DamageReceiver>();
                if (dr != null && dr.IsAlive)
                {
                    float d = Vector2.Distance(_player.transform.position, e.transform.position);
                    if (d < min) { min = d; n = e.transform; }
                }
            }
            return n;
        }

        private void EndTest(string result)
        {
            _testComplete = true;
            Debug.Log($"[QA] === 테스트 완료: {result} ===");
            QARecorder.Instance?.StopRecording();
        }
    }
}
