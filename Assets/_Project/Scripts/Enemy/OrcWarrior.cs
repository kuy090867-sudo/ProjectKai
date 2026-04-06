using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Core;

namespace ProjectKai.Enemy
{
    /// <summary>
    /// 오크 워리어 — 탱크형 적.
    /// AI: 느린 전진 → 방패 차기 (강넉백) → 강공격.
    /// HP 120, 데미지 20.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class OrcWarrior : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _chaseSpeed = 3f;
        [SerializeField] private float _detectionRange = 7f;
        [SerializeField] private float _attackRange = 1.8f;

        [Header("Attack")]
        [SerializeField] private float _attackDamage = 20f;
        [SerializeField] private float _shieldKnockback = 12f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private Vector2 _hitboxSize = new Vector2(1.5f, 1.2f);

        private Rigidbody2D _rb;
        private DamageReceiver _dr;
        private SpriteRenderer _sr;
        private Transform _player;
        private float _fixedY;
        private float _lastAttackTime = -999f;
        private int _facingDir = -1;

        private enum State { Patrol, Chase, ShieldBash, HeavyAttack, Hit, Dead }
        private State _state = State.Patrol;
        private float _stateTimer;
        private Vector2 _patrolOrigin;

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
            _dr.OnDeath += () => { _state = State.Dead; Destroy(gameObject, 1.5f); };

            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void Update()
        {
            if (_player == null || !_dr.IsAlive || _state == State.Dead) return;

            _stateTimer += Time.deltaTime;
            UpdateFacing();
            float dist = Vector2.Distance(transform.position, _player.position);

            switch (_state)
            {
                case State.Patrol:
                    if (dist <= _detectionRange)
                        _state = State.Chase;
                    else
                        Patrol();
                    break;

                case State.Chase:
                    if (dist > _detectionRange * 1.5f)
                    {
                        _state = State.Patrol;
                        break;
                    }
                    if (dist <= _attackRange && Time.time >= _lastAttackTime + _attackCooldown)
                    {
                        _state = Random.value > 0.5f ? State.ShieldBash : State.HeavyAttack;
                        _stateTimer = 0f;
                        break;
                    }
                    float dir = Mathf.Sign(_player.position.x - transform.position.x);
                    _rb.MovePosition(new Vector2(
                        transform.position.x + dir * _chaseSpeed * Time.deltaTime,
                        _fixedY));
                    break;

                case State.ShieldBash:
                    if (_stateTimer < 0.4f)
                    {
                        // 텔레그래프
                        if (_sr != null) _sr.color = Color.Lerp(Color.white, Color.yellow, _stateTimer / 0.4f);
                    }
                    else if (_stateTimer < 0.5f)
                    {
                        // 돌진 + 타격
                        _rb.MovePosition(new Vector2(
                            transform.position.x + _facingDir * 4f * Time.deltaTime,
                            _fixedY));
                        PerformHit(_attackDamage * 0.5f, _shieldKnockback);
                        AudioManager.Instance?.PlaySFX("hit", 0.7f);
                        if (_sr != null) _sr.color = Color.white;
                    }
                    else
                    {
                        _lastAttackTime = Time.time;
                        _state = State.Chase;
                    }
                    break;

                case State.HeavyAttack:
                    if (_stateTimer < 0.5f)
                    {
                        if (_sr != null) _sr.color = Color.Lerp(Color.white, Color.red, _stateTimer / 0.5f);
                    }
                    else if (_stateTimer < 0.6f)
                    {
                        PerformHit(_attackDamage, 8f);
                        GameFeel.Instance?.CameraShake(0.12f, 0.15f);
                        AudioManager.Instance?.PlaySFX("sword_hit", 0.7f);
                        if (_sr != null) _sr.color = Color.white;
                    }
                    else
                    {
                        _lastAttackTime = Time.time;
                        _state = State.Chase;
                    }
                    break;

                case State.Hit:
                    if (_stateTimer >= 0.2f)
                        _state = State.Chase;
                    break;
            }
        }

        private void Patrol()
        {
            float d = transform.position.x - _patrolOrigin.x;
            if (Mathf.Abs(d) >= 3f) _facingDir = d > 0 ? -1 : 1;
            _rb.MovePosition(new Vector2(
                transform.position.x + _facingDir * _moveSpeed * Time.deltaTime,
                _fixedY));
        }

        private void PerformHit(float damage, float knockback)
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(_facingDir * 1f, 0f);
            var hits = Physics2D.OverlapBoxAll(origin, _hitboxSize, 0f);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                var dmg = hit.GetComponent<IDamageable>();
                if (dmg != null && dmg.IsAlive)
                    dmg.TakeDamage(damage, new Vector2(_facingDir, 0.2f).normalized, knockback);
            }
        }

        private void UpdateFacing()
        {
            if (_player == null) return;
            _facingDir = _player.position.x > transform.position.x ? 1 : -1;
            if (_sr != null) _sr.flipX = _facingDir < 0;
        }
    }
}
