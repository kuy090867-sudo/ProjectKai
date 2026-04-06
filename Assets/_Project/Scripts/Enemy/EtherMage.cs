using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Core;

namespace ProjectKai.Enemy
{
    /// <summary>
    /// 에테르 마법사 — 원거리 적 (2장).
    /// AI: 순간이동 → 마법 투사체 3발 → 순간이동.
    /// HP 60, 데미지 15.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class EtherMage : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _detectionRange = 12f;
        [SerializeField] private float _teleportRange = 5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private float _projectileDamage = 15f;
        [SerializeField] private float _projectileSpeed = 10f;
        [SerializeField] private int _burstCount = 3;

        [Header("Glow")]
        [SerializeField] private float _glowSpeed = 2f;
        [SerializeField] private Color _glowColorA = new Color(0.3f, 0.6f, 1f);   // 파란색
        [SerializeField] private Color _glowColorB = new Color(0.85f, 0.9f, 1f);   // 백색(약간 파랑)

        private Rigidbody2D _rb;
        private DamageReceiver _dr;
        private SpriteRenderer _sr;
        private Transform _player;
        private float _fixedY;
        private float _actionTimer;
        private bool _isActing;
        private bool _isGlowing = true;
        private int _facingDir = -1;
        private Vector2 _spawnPosition;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _dr = GetComponent<DamageReceiver>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            _fixedY = transform.position.y;
            _spawnPosition = transform.position;
            _dr.OnDamaged += (d, dir) => { if (_dr.IsAlive) StartCoroutine(TeleportAway()); };
            _dr.OnDeath += () => Destroy(gameObject, 1f);

            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void Update()
        {
            if (_player == null || !_dr.IsAlive) return;

            // 에테르 발광: 텔레그래프(_isActing) 중이 아닐 때만 사인파 색상 펄스
            if (_isGlowing && !_isActing && _sr != null)
            {
                float t = (Mathf.Sin(Time.time * _glowSpeed) + 1f) * 0.5f;   // 0~1 반복
                _sr.color = Color.Lerp(_glowColorA, _glowColorB, t);
            }

            if (_isActing) return;

            UpdateFacing();
            float dist = Vector2.Distance(transform.position, _player.position);

            if (dist > _detectionRange) return;

            _actionTimer += Time.deltaTime;
            if (_actionTimer >= _attackCooldown)
            {
                _actionTimer = 0f;
                StartCoroutine(AttackSequence());
            }
        }

        private System.Collections.IEnumerator AttackSequence()
        {
            _isActing = true;
            _isGlowing = false;   // 텔레그래프 중 발광 비활성화

            // 텔레그래프: 보라색 빛
            if (_sr != null) _sr.color = new Color(0.6f, 0.2f, 1f);
            yield return new WaitForSeconds(0.4f);

            // 투사체 3발 연속
            for (int i = 0; i < _burstCount; i++)
            {
                FireProjectile();
                AudioManager.Instance?.PlaySFX("sword_swing", 0.3f);
                yield return new WaitForSeconds(0.25f);
            }

            if (_sr != null) _sr.color = Color.white;
            yield return new WaitForSeconds(0.3f);

            // 순간이동
            yield return StartCoroutine(TeleportAway());

            _isGlowing = true;    // 발광 재활성화
            _isActing = false;
        }

        private void FireProjectile()
        {
            if (_player == null) return;

            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;

            var projObj = new GameObject("EtherBolt");
            projObj.transform.position = transform.position + new Vector3(dir.x * 0.5f, 0f, 0f);

            var sr = projObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.6f, 0.3f, 1f);
            var tex = new Texture2D(4, 4);
            var pix = new Color[16];
            for (int i = 0; i < 16; i++) pix[i] = Color.white;
            tex.SetPixels(pix); tex.filterMode = FilterMode.Point; tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

            var rb = projObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = dir * _projectileSpeed;

            var col = projObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.2f, 0.2f);

            projObj.AddComponent<Projectile>().Initialize(dir, _projectileSpeed, _projectileDamage, 3f);
            projObj.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

            Destroy(projObj, 4f);
        }

        private System.Collections.IEnumerator TeleportAway()
        {
            if (_sr != null) _sr.color = new Color(0.4f, 0.1f, 0.8f, 0.5f);
            yield return new WaitForSeconds(0.15f);

            // 랜덤 위치로 순간이동 (스폰 지점 기준 범위 제한)
            float offsetX = Random.Range(-_teleportRange, _teleportRange);
            float newX = Mathf.Clamp(
                transform.position.x + offsetX,
                _spawnPosition.x - _teleportRange * 2f,
                _spawnPosition.x + _teleportRange * 2f);
            _rb.MovePosition(new Vector2(newX, _fixedY));

            if (_sr != null) _sr.color = Color.white;
        }

        private void UpdateFacing()
        {
            if (_player == null) return;
            _facingDir = _player.position.x > transform.position.x ? 1 : -1;
            if (_sr != null) _sr.flipX = _facingDir < 0;
        }
    }
}
