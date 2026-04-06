using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Enemy
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hit,
        Dead
    }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _chaseSpeed = 5f;

        [Header("Detection")]
        [SerializeField] private float _detectionRange = 8f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Patrol")]
        [SerializeField] private float _patrolDistance = 4f;
        [SerializeField] private float _patrolWaitTime = 1f;

        [Header("Attack")]
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private float _attackDuration = 0.4f;
        [SerializeField] private float _knockbackForce = 5f;
        [SerializeField] private Vector2 _attackHitboxSize = new Vector2(1f, 1f);
        [SerializeField] private Vector2 _attackHitboxOffset = new Vector2(1f, 0f);

        [Header("Hit")]
        [SerializeField] private float _hitStunDuration = 0.3f;

        private Rigidbody2D _rb;
        private DamageReceiver _damageReceiver;
        private SpriteRenderer _spriteRenderer;

        private EnemyState _currentState = EnemyState.Idle;
        private int _facingDirection = 1;
        private Vector2 _patrolOrigin;
        private float _stateTimer;
        private float _lastAttackTime = -999f;
        private Transform _playerTransform;
        private float _fixedY; // Kinematic이므로 Y좌표 고정

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _damageReceiver = GetComponent<DamageReceiver>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _patrolOrigin = transform.position;

            if (_playerLayer == 0)
            {
                int playerLayerIdx = LayerMask.NameToLayer("Player");
                if (playerLayerIdx >= 0)
                    _playerLayer = 1 << playerLayerIdx;
            }
        }

        private void Start()
        {
            _damageReceiver.OnDamaged += OnDamaged;
            _damageReceiver.OnDeath += OnDeath;
            _fixedY = transform.position.y;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            SetState(EnemyState.Patrol);
        }

        private void Update()
        {
            _stateTimer += Time.deltaTime;

            switch (_currentState)
            {
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Chase: UpdateChase(); break;
                case EnemyState.Attack: UpdateAttack(); break;
                case EnemyState.Hit: UpdateHit(); break;
                case EnemyState.Dead: break;
            }

            UpdateFacing();
        }

        private void SetState(EnemyState newState)
        {
            _currentState = newState;
            _stateTimer = 0f;
        }

        private void UpdatePatrol()
        {
            if (DetectPlayer())
            {
                SetState(EnemyState.Chase);
                return;
            }

            float distFromOrigin = transform.position.x - _patrolOrigin.x;

            if (Mathf.Abs(distFromOrigin) >= _patrolDistance)
            {
                _facingDirection = distFromOrigin > 0 ? -1 : 1;
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

                if (_stateTimer < _patrolWaitTime) return;
                _stateTimer = 0f;
            }

            _rb.MovePosition(new Vector2(
                transform.position.x + _facingDirection * _moveSpeed * Time.deltaTime,
                _fixedY));
        }

        private void UpdateChase()
        {
            if (_playerTransform == null || !DetectPlayer())
            {
                SetState(EnemyState.Patrol);
                return;
            }

            float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distToPlayer <= _attackRange && Time.time >= _lastAttackTime + _attackCooldown)
            {
                SetState(EnemyState.Attack);
                return;
            }

            float dirToPlayer = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            _facingDirection = dirToPlayer > 0 ? 1 : -1;
            _rb.MovePosition(new Vector2(
                transform.position.x + dirToPlayer * _chaseSpeed * Time.deltaTime,
                _fixedY));
        }

        private void UpdateAttack()
        {
            // 텔레그래프 (공격 전 0.3초: 붉은 빛 + 살짝 뒤로)
            float telegraphTime = 0.3f;

            if (_stateTimer < telegraphTime)
            {
                // 공격 예고: 스프라이트 붉은 빛
                if (_spriteRenderer != null)
                    _spriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.3f, 0.3f), _stateTimer / telegraphTime);

                // 살짝 뒤로 물러남 (힘 모으는 동작)
                float pullBack = -_facingDirection * 0.5f * Time.deltaTime;
                _rb.MovePosition(new Vector2(transform.position.x + pullBack, _fixedY));
                return;
            }

            // 텔레그래프 끝 → 원래 색 복원
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.white;

            float attackTime = _stateTimer - telegraphTime;

            // 실제 타격
            if (attackTime >= _attackDuration * 0.3f && attackTime < _attackDuration * 0.3f + Time.deltaTime)
            {
                // 돌진 공격
                _rb.MovePosition(new Vector2(
                    transform.position.x + _facingDirection * 2f,
                    _fixedY));
                PerformAttackHit();
                Core.AudioManager.Instance?.PlaySFX("sword_swing", 0.4f);
            }

            if (attackTime >= _attackDuration)
            {
                _lastAttackTime = Time.time;
                SetState(EnemyState.Chase);
            }
        }

        private void UpdateHit()
        {
            if (_stateTimer >= _hitStunDuration)
            {
                SetState(EnemyState.Chase);
            }
        }

        private void PerformAttackHit()
        {
            Vector2 hitOrigin = (Vector2)transform.position +
                new Vector2(_attackHitboxOffset.x * _facingDirection, _attackHitboxOffset.y);

            var hits = Physics2D.OverlapBoxAll(hitOrigin, _attackHitboxSize, 0f, _playerLayer);

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    Vector2 knockDir = new Vector2(_facingDirection, 0.3f).normalized;
                    damageable.TakeDamage(_attackDamage, knockDir, _knockbackForce);
                }
            }
        }

        private bool DetectPlayer()
        {
            // 레이어 기반 감지
            var hit = Physics2D.OverlapCircle(transform.position, _detectionRange, _playerLayer);
            if (hit != null)
            {
                _playerTransform = hit.transform;
                return true;
            }

            // 폴백: 태그 기반 거리 감지 (레이어 할당 타이밍 문제 대응)
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }

            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                return dist <= _detectionRange;
            }

            return false;
        }

        private void OnDamaged(float damage, Vector2 direction)
        {
            if (_currentState == EnemyState.Dead) return;
            SetState(EnemyState.Hit);
        }

        private void OnDeath()
        {
            SetState(EnemyState.Dead);
            Destroy(gameObject, 1f);
        }

        private void UpdateFacing()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = _facingDirection < 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);

            Gizmos.color = Color.magenta;
            Vector2 hitOrigin = (Vector2)transform.position +
                new Vector2(_attackHitboxOffset.x * _facingDirection, _attackHitboxOffset.y);
            Gizmos.DrawWireCube(hitOrigin, _attackHitboxSize);
        }
    }
}
