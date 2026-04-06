using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Core;
using ProjectKai.Data;

namespace ProjectKai.Enemy
{
    public enum BossPhase { Phase1, Phase2, Phase3 }

    /// <summary>
    /// 수정왕 고블린 (1장 보스)
    /// Phase 1 (100~50%): 돌진 → 3연타
    /// Phase 2 (50~20%): 점프 공격, 속도 1.3배
    /// Phase 3 (20~0%): 분노 모드, 졸개 소환
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class BossGoblin : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private float _chargeSpeed = 12f;
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _jumpAttackDamage = 25f;
        [SerializeField] private float _detectionRange = 15f;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Attack")]
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private float _chargeDuration = 0.5f;
        [SerializeField] private Vector2 _hitboxSize = new Vector2(2f, 1.5f);

        private Rigidbody2D _rb;
        private DamageReceiver _dr;
        private SpriteRenderer _sr;
        private SpriteAnimator _anim;
        private Transform _player;

        private BossPhase _phase = BossPhase.Phase1;
        private float _fixedY;
        private int _facingDir = -1;
        private float _actionTimer;
        private float _actionCooldown;
        private int _comboCount;
        private bool _isActing;

        private enum BossAction { Idle, Chase, Charge, ComboAttack, JumpAttack, Roar, Summon }
        private BossAction _currentAction = BossAction.Idle;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _dr = GetComponent<DamageReceiver>();
            _sr = GetComponentInChildren<SpriteRenderer>();
            _anim = GetComponentInChildren<SpriteAnimator>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            _fixedY = transform.position.y;
            _dr.OnDamaged += OnDamaged;
            _dr.OnDeath += OnDeath;
            _dr.OnHealthChanged += CheckPhaseTransition;

            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;

            _actionCooldown = _attackCooldown;
        }

        private void Update()
        {
            if (_player == null || !_dr.IsAlive) return;

            UpdateFacing();
            _actionTimer += Time.deltaTime;

            if (!_isActing && _actionTimer >= _actionCooldown)
            {
                ChooseAction();
            }
        }

        private void ChooseAction()
        {
            float dist = Vector2.Distance(transform.position, _player.position);

            switch (_phase)
            {
                case BossPhase.Phase1:
                    if (dist > 5f)
                        StartAction(BossAction.Charge);
                    else
                        StartAction(BossAction.ComboAttack);
                    break;

                case BossPhase.Phase2:
                    int r = Random.Range(0, 3);
                    if (r == 0) StartAction(BossAction.JumpAttack);
                    else if (dist > 4f) StartAction(BossAction.Charge);
                    else StartAction(BossAction.ComboAttack);
                    break;

                case BossPhase.Phase3:
                    int r3 = Random.Range(0, 4);
                    if (r3 == 0) StartAction(BossAction.Summon);
                    else if (r3 == 1) StartAction(BossAction.JumpAttack);
                    else if (dist > 3f) StartAction(BossAction.Charge);
                    else StartAction(BossAction.ComboAttack);
                    break;
            }
        }

        private void StartAction(BossAction action)
        {
            _currentAction = action;
            _isActing = true;
            _actionTimer = 0f;

            switch (action)
            {
                case BossAction.Charge:
                    StartCoroutine(ChargeAttack());
                    break;
                case BossAction.ComboAttack:
                    StartCoroutine(ComboAttack());
                    break;
                case BossAction.JumpAttack:
                    StartCoroutine(JumpAttack());
                    break;
                case BossAction.Summon:
                    StartCoroutine(SummonMinions());
                    break;
            }
        }

        private System.Collections.IEnumerator ChargeAttack()
        {
            // 텔레그래프: 붉은 빛 0.5초
            _sr.color = new Color(1f, 0.3f, 0.3f);
            _anim?.Play("run");
            yield return new WaitForSeconds(0.5f);

            // 돌진
            _sr.color = Color.white;
            float timer = 0f;
            float speed = _phase == BossPhase.Phase3 ? _chargeSpeed * 1.5f : _chargeSpeed;

            while (timer < _chargeDuration)
            {
                timer += Time.deltaTime;
                _rb.MovePosition(new Vector2(
                    transform.position.x + _facingDir * speed * Time.deltaTime,
                    _fixedY));
                yield return null;
            }

            // 돌진 끝에서 타격
            PerformHit(_attackDamage, 8f);

            _isActing = false;
            _actionCooldown = _attackCooldown * (_phase == BossPhase.Phase2 ? 0.7f : _phase == BossPhase.Phase3 ? 0.5f : 1f);
        }

        private System.Collections.IEnumerator ComboAttack()
        {
            int comboMax = _phase == BossPhase.Phase3 ? 4 : 3;

            for (int i = 0; i < comboMax; i++)
            {
                _sr.color = new Color(1f, 0.5f, 0.5f);
                yield return new WaitForSeconds(0.2f);
                _sr.color = Color.white;

                PerformHit(_attackDamage * (1f + i * 0.2f), 5f + i * 2f);
                AudioManager.Instance?.PlaySFX("sword_swing", 0.6f);

                yield return new WaitForSeconds(0.3f);
            }

            _isActing = false;
            _actionCooldown = _attackCooldown;
        }

        private System.Collections.IEnumerator JumpAttack()
        {
            _anim?.Play("jump");
            _sr.color = new Color(1f, 0.3f, 0f);

            // 점프
            Vector2 startPos = transform.position;
            Vector2 targetPos = new Vector2(_player.position.x, _fixedY);
            float jumpHeight = 3f;
            float duration = 0.6f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = timer / duration;
                float x = Mathf.Lerp(startPos.x, targetPos.x, t);
                float y = _fixedY + jumpHeight * Mathf.Sin(Mathf.PI * t);
                _rb.MovePosition(new Vector2(x, y));
                yield return null;
            }

            // 착지 충격
            _rb.MovePosition(new Vector2(targetPos.x, _fixedY));
            _sr.color = Color.white;
            GameFeel.Instance?.CameraShake(0.2f, 0.2f);
            GameFeel.Instance?.HitStop(0.08f);
            AudioManager.Instance?.PlaySFX("hit", 0.8f);

            // 넓은 범위 타격
            PerformHit(_jumpAttackDamage, 10f, new Vector2(3f, 2f));

            _anim?.Play("idle");
            _isActing = false;
            _actionCooldown = _attackCooldown * 1.5f;
        }

        private System.Collections.IEnumerator SummonMinions()
        {
            // 포효
            _sr.color = Color.red;
            GameFeel.Instance?.CameraShake(0.15f, 0.3f);
            AudioManager.Instance?.PlaySFX("enemy_death", 0.5f);
            yield return new WaitForSeconds(0.8f);
            _sr.color = Color.white;

            // 졸개 2마리 소환 (간단한 고블린)
            for (int i = 0; i < 2; i++)
            {
                float offsetX = (i == 0 ? -3f : 3f);
                var minionObj = new GameObject("Minion");
                minionObj.transform.position = new Vector3(transform.position.x + offsetX, _fixedY, 0);
                minionObj.tag = "Enemy";

                var sr = minionObj.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.5f, 1f, 0.5f);

                // 플레이스홀더 스프라이트
                var tex = new Texture2D(4, 4);
                var pix = new Color[16];
                for (int j = 0; j < 16; j++) pix[j] = Color.white;
                tex.SetPixels(pix);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

                var rb = minionObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;

                var col = minionObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.5f, 0.8f);

                var dmgRecv = minionObj.AddComponent<DamageReceiver>();

                minionObj.AddComponent<EnemyBase>();

                // StageManager에 적 추가
                if (StageManager.Instance != null)
                {
                    dmgRecv.OnDeath += () =>
                    {
                        // 사망 시 StageManager 카운트 업데이트는 태그 기반으로 자동 처리
                    };
                }
            }

            Debug.Log("[BossGoblin] 졸개 2마리 소환!");
            _isActing = false;
            _actionCooldown = _attackCooldown * 2f;
        }

        private void PerformHit(float damage, float knockback, Vector2? customSize = null)
        {
            Vector2 size = customSize ?? _hitboxSize;
            Vector2 origin = (Vector2)transform.position + new Vector2(_facingDir * 1f, 0f);

            var hits = Physics2D.OverlapBoxAll(origin, size, 0f);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    Vector2 knockDir = new Vector2(_facingDir, 0.3f).normalized;
                    damageable.TakeDamage(damage, knockDir, knockback);
                }
            }
        }

        private void CheckPhaseTransition(float current, float max)
        {
            float ratio = current / max;

            if (ratio <= 0.2f && _phase != BossPhase.Phase3)
            {
                _phase = BossPhase.Phase3;
                Debug.Log("[BossGoblin] Phase 3 - 분노 모드!");
                _sr.color = Color.red;
                GameFeel.Instance?.CameraShake(0.2f, 0.5f);
                StartCoroutine(PhaseTransitionRoar());
            }
            else if (ratio <= 0.5f && _phase == BossPhase.Phase1)
            {
                _phase = BossPhase.Phase2;
                Debug.Log("[BossGoblin] Phase 2 - 점프 공격 해금!");
                GameFeel.Instance?.CameraShake(0.15f, 0.3f);
                StartCoroutine(PhaseTransitionRoar());
            }
        }

        private System.Collections.IEnumerator PhaseTransitionRoar()
        {
            _isActing = true;
            yield return new WaitForSeconds(1f);
            _sr.color = Color.white;
            _isActing = false;
        }

        private void OnDamaged(float damage, Vector2 dir)
        {
            _anim?.ForcePlay("hit");
        }

        private void OnDeath()
        {
            Debug.Log("[BossGoblin] 수정왕 고블린 처치!");
            _isActing = true;
            _rb.MovePosition(new Vector2(transform.position.x, _fixedY));
            GameFeel.Instance?.KillSlowMotion(0.8f, 0.1f);
            Destroy(gameObject, 2f);
        }

        private void UpdateFacing()
        {
            if (_player == null) return;
            _facingDir = _player.position.x > transform.position.x ? 1 : -1;
            if (_sr != null) _sr.flipX = _facingDir < 0;
        }
    }
}
