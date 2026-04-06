using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Core;

namespace ProjectKai.Enemy
{
    /// <summary>
    /// 스켈레톤 궁수 — 원거리 적.
    /// AI: 감지 → 후퇴하며 화살 발사 → 거리 유지.
    /// HP 40, 데미지 12.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class SkeletonArcher : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _retreatSpeed = 4f;
        [SerializeField] private float _detectionRange = 10f;
        [SerializeField] private float _preferredRange = 6f;
        [SerializeField] private float _tooCloseRange = 3f;

        [Header("Attack")]
        [SerializeField] private float _attackCooldown = 1.5f;
        [SerializeField] private float _arrowSpeed = 12f;
        [SerializeField] private float _arrowDamage = 12f;

        private Rigidbody2D _rb;
        private DamageReceiver _dr;
        private SpriteRenderer _sr;
        private Transform _player;
        private float _fixedY;
        private float _lastAttackTime = -999f;
        private int _facingDir = -1;

        private enum State { Patrol, Retreat, Attack, Hit, Dead }
        private State _state = State.Patrol;
        private Vector2 _patrolOrigin;
        private float _stateTimer;

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
            _patrolOrigin = transform.position;
            _dr.OnDamaged += (d, dir) => { _state = State.Hit; _stateTimer = 0f; };
            _dr.OnDeath += () => { _state = State.Dead; StopAllCoroutines(); Destroy(gameObject, 1f); };

            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void Update()
        {
            if (_player == null || !_dr.IsAlive) return;

            _stateTimer += Time.deltaTime;
            UpdateFacing();

            float dist = Vector2.Distance(transform.position, _player.position);

            switch (_state)
            {
                case State.Patrol:
                    if (dist <= _detectionRange)
                    {
                        if (dist < _tooCloseRange)
                            _state = State.Retreat;
                        else if (Time.time >= _lastAttackTime + _attackCooldown)
                            _state = State.Attack;
                    }
                    else
                    {
                        Patrol();
                    }
                    break;

                case State.Retreat:
                    float retreatDir = Mathf.Sign(transform.position.x - _player.position.x);
                    _rb.MovePosition(new Vector2(
                        transform.position.x + retreatDir * _retreatSpeed * Time.deltaTime,
                        _fixedY));
                    if (dist >= _preferredRange)
                        _state = State.Attack;
                    break;

                case State.Attack:
                    if (_stateTimer >= 0.3f)
                    {
                        ShootArrow();
                        _lastAttackTime = Time.time;
                        _state = State.Patrol;
                        _stateTimer = 0f;
                    }
                    else
                    {
                        // 텔레그래프: 붉은 빛
                        if (_sr != null)
                            _sr.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f), _stateTimer / 0.3f);
                    }
                    break;

                case State.Hit:
                    if (_stateTimer >= 0.3f)
                        _state = State.Patrol;
                    break;
            }
        }

        private void Patrol()
        {
            float distFromOrigin = transform.position.x - _patrolOrigin.x;
            if (Mathf.Abs(distFromOrigin) >= 3f)
                _facingDir = distFromOrigin > 0 ? -1 : 1;

            _rb.MovePosition(new Vector2(
                transform.position.x + _facingDir * _moveSpeed * Time.deltaTime,
                _fixedY));
        }

        private void ShootArrow()
        {
            if (_sr != null) _sr.color = Color.white;
            AudioManager.Instance?.PlaySFX("sword_swing", 0.4f);

            // 화살 생성
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.position = transform.position + new Vector3(_facingDir * 0.5f, 0f, 0f);

            var arrowSr = arrowObj.AddComponent<SpriteRenderer>();
            arrowSr.color = new Color(0.8f, 0.8f, 0.6f);
            var tex = new Texture2D(4, 2);
            for (int i = 0; i < 8; i++) tex.SetPixel(i % 4, i / 4, Color.white);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            arrowSr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 2), new Vector2(0.5f, 0.5f), 16f);

            var arrowRb = arrowObj.AddComponent<Rigidbody2D>();
            arrowRb.gravityScale = 0.3f;
            arrowRb.linearVelocity = new Vector2(_facingDir * _arrowSpeed, 1f);

            var arrowCol = arrowObj.AddComponent<BoxCollider2D>();
            arrowCol.isTrigger = true;
            arrowCol.size = new Vector2(0.3f, 0.1f);

            var proj = arrowObj.AddComponent<Projectile>();
            proj.Initialize(new Vector2(_facingDir, 0f), _arrowSpeed, _arrowDamage, 3f);

            Destroy(arrowObj, 5f);
        }

        private void UpdateFacing()
        {
            if (_player == null) return;
            _facingDir = _player.position.x > transform.position.x ? 1 : -1;
            if (_sr != null) _sr.flipX = _facingDir < 0;
        }
    }
}
