using UnityEngine;
using ProjectKai.StateMachine;
using ProjectKai.Player.States;
using ProjectKai.Combat;
using ProjectKai.Core;
using ProjectKai.Data;

namespace ProjectKai.Player
{
    [DefaultExecutionOrder(-50)] // GameSetup(-100) 이후에 실행
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour, IDamageable, IKnockbackable
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _acceleration = 50f;
        [SerializeField] private float _deceleration = 50f;
        [SerializeField] private float _airAcceleration = 30f;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 14f;
        [SerializeField] private float _jumpCutMultiplier = 0.5f;
        [SerializeField] private float _fallGravityMultiplier = 2.5f;
        [SerializeField] private float _maxFallSpeed = 20f;
        [SerializeField] private int _maxAirJumps = 1;

        [Header("Dash")]
        [SerializeField] private float _dashSpeed = 20f;
        [SerializeField] private float _dashDuration = 0.15f;
        [SerializeField] private float _dashCooldown = 0.5f;

        [Header("Combat")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _hitStunDuration = 0.3f;

        [Header("References")]
        [SerializeField] private GroundCheck _groundCheck;
        [SerializeField] private DamageDealer _damageDealer;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private LayerMask _enemyLayer;

        [Header("Weapon Data")]
        [SerializeField] private ComboDataSO _meleeComboData;
        [SerializeField] private WeaponDataSO _rangedWeaponData;

        // Components
        public Rigidbody2D Rb { get; private set; }
        public PlayerInputHandler Input { get; private set; }
        public InputBuffer InputBuffer { get; private set; }
        public Animator Animator { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }
        public SpriteAnimator SpriteAnim { get; private set; }
        public GroundCheck GroundCheck => _groundCheck;

        // State Machine
        public StateMachine.StateMachine StateMachine { get; private set; }
        public IdleState IdleState { get; private set; }
        public RunState RunState { get; private set; }
        public JumpState JumpState { get; private set; }
        public FallState FallState { get; private set; }
        public DashState DashState { get; private set; }
        public MeleeAttackState MeleeAttackState { get; private set; }
        public RangedAttackState RangedAttackState { get; private set; }
        public WallSlideState WallSlideState { get; private set; }

        // Combat
        public ComboSystem ComboSystem { get; private set; }

        // Properties
        public float MoveSpeed => _moveSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float AirAcceleration => _airAcceleration;
        public float JumpForce => _jumpForce;
        public float JumpCutMultiplier => _jumpCutMultiplier;
        public float FallGravityMultiplier => _fallGravityMultiplier;
        public float MaxFallSpeed => _maxFallSpeed;
        public int MaxAirJumps => _maxAirJumps;
        public float DashSpeed => _dashSpeed;
        public float DashDuration => _dashDuration;
        public float DashCooldown => _dashCooldown;
        public float HitStunDuration => _hitStunDuration;

        // Runtime State
        public int FacingDirection { get; private set; } = 1;
        public int AirJumpsRemaining { get; set; }
        public float LastDashTime { get; set; } = -999f;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        public bool IsMelee { get; private set; } = true;
        public bool IsInvincible { get; set; }

        private float _defaultGravityScale;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Input = GetComponent<PlayerInputHandler>();
            Animator = GetComponentInChildren<Animator>();
            SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            SpriteAnim = GetComponentInChildren<SpriteAnimator>();
            InputBuffer = new InputBuffer(0.15f);

            // 자동 참조 연결 — Inspector에서 안 넣어도 자동 탐색
            if (_groundCheck == null)
                _groundCheck = GetComponentInChildren<GroundCheck>();
            if (_damageDealer == null)
                _damageDealer = GetComponent<DamageDealer>();
            if (_firePoint == null)
            {
                var fp = transform.Find("FirePoint");
                if (fp != null) _firePoint = fp;
            }

            // Enemy 레이어 자동 설정
            if (_enemyLayer == 0)
            {
                int enemyLayerIdx = LayerMask.NameToLayer("Enemy");
                if (enemyLayerIdx >= 0)
                    _enemyLayer = 1 << enemyLayerIdx;
            }

            // 콤보/무기 데이터 자동 로드 (Resources → RuntimeCache 순서)
            if (_meleeComboData == null)
                _meleeComboData = Resources.Load<ComboDataSO>("Data/BasicSwordCombo");
            if (_meleeComboData == null)
                _meleeComboData = RuntimeDataCache.SwordCombo;
            if (_rangedWeaponData == null)
                _rangedWeaponData = Resources.Load<WeaponDataSO>("Data/MagicPistol");
            if (_rangedWeaponData == null)
                _rangedWeaponData = RuntimeDataCache.MagicPistol;

            // 에테르 폭발 스킬 자동 추가
            if (GetComponent<Combat.EtherBurst>() == null)
                gameObject.AddComponent<Combat.EtherBurst>();

            _defaultGravityScale = Rb.gravityScale;
            CurrentHealth = _maxHealth;

            StateMachine = new StateMachine.StateMachine();
            IdleState = new IdleState(this);
            RunState = new RunState(this);
            JumpState = new JumpState(this);
            FallState = new FallState(this);
            DashState = new DashState(this);

            ComboSystem = new ComboSystem();
            if (_meleeComboData != null)
                ComboSystem.SetComboData(_meleeComboData);

            WallSlideState = new WallSlideState(this);
            MeleeAttackState = new MeleeAttackState(this, ComboSystem, _damageDealer, _enemyLayer);
            RangedAttackState = new RangedAttackState(this, _firePoint);
            if (_rangedWeaponData != null)
                RangedAttackState.SetWeaponData(_rangedWeaponData);
        }

        private void Start()
        {
            StateMachine.Initialize(IdleState);

            Input.OnAttackPressed += () => InputBuffer.Buffer(BufferedInput.Attack);
            Input.OnShootPressed += () => InputBuffer.Buffer(BufferedInput.Shoot);
            Input.OnDashPressed += () => InputBuffer.Buffer(BufferedInput.Dash);
            Input.OnJumpPressed += () => InputBuffer.Buffer(BufferedInput.Jump);
            Input.OnWeaponSwitchPressed += () =>
            {
                IsMelee = !IsMelee;
                Core.AudioManager.Instance?.PlaySFX("dash", 0.3f);
                Debug.Log($"[Player] 무기 전환: {(IsMelee ? "검" : "총")}");
            };
        }

        private void Update()
        {
            if (StateMachine == null) return;
            StateMachine.Update();
            InputBuffer.Update();
            ComboSystem.Update();
            UpdateFacing();
            Input.ConsumeAllPresses();
        }

        private void FixedUpdate()
        {
            if (StateMachine == null) return;
            StateMachine.FixedUpdate();
            ClampFallSpeed();
        }

        public void SetVelocity(float x, float y)
        {
            Rb.linearVelocity = new Vector2(x, y);
        }

        public void SetVelocityX(float x)
        {
            Rb.linearVelocity = new Vector2(x, Rb.linearVelocity.y);
        }

        public void SetVelocityY(float y)
        {
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, y);
        }

        public void SetGravityScale(float scale)
        {
            Rb.gravityScale = scale;
        }

        public void ResetGravity()
        {
            Rb.gravityScale = _defaultGravityScale;
        }

        public bool CanDash()
        {
            return Time.time >= LastDashTime + _dashCooldown;
        }

        private void UpdateFacing()
        {
            float moveX = Input.MoveInput.x;
            if (moveX != 0f)
            {
                FacingDirection = moveX > 0f ? 1 : -1;
                if (SpriteRenderer != null)
                {
                    SpriteRenderer.flipX = FacingDirection < 0;
                }
            }
        }

        private void ClampFallSpeed()
        {
            if (Rb.linearVelocity.y < -_maxFallSpeed)
            {
                SetVelocityY(-_maxFallSpeed);
            }
        }

        public void TakeDamage(float damage, Vector2 knockbackDir, float knockbackForce)
        {
            if (!IsAlive || IsInvincible) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(CurrentHealth, 0f);

            ApplyKnockback(knockbackDir, knockbackForce);

            // 피격 리액션 (3채널)
            Core.AudioManager.Instance?.PlaySFX("hit", 0.7f);
            Core.GameFeel.Instance?.CameraShake(0.1f, 0.12f);
            Core.GameFeel.Instance?.HitStop(0.04f);
            SpriteAnim?.ForcePlay("hit");

            if (!IsAlive)
            {
                OnDeath();
            }
            else
            {
                // 무적 프레임
                StartCoroutine(InvincibilityCoroutine());
            }
        }

        private System.Collections.IEnumerator InvincibilityCoroutine()
        {
            IsInvincible = true;
            float timer = 0f;
            float duration = 0.5f;

            while (timer < duration)
            {
                if (SpriteRenderer != null)
                    SpriteRenderer.enabled = !SpriteRenderer.enabled;
                yield return new WaitForSeconds(0.05f);
                timer += 0.05f;
            }

            if (SpriteRenderer != null)
                SpriteRenderer.enabled = true;
            IsInvincible = false;
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            Rb.linearVelocity = Vector2.zero;
            Rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
        }

        private void OnDeath()
        {
            Debug.Log("Player died!");
            SpriteAnim?.Play("death");
            Rb.linearVelocity = Vector2.zero;
            Rb.bodyType = RigidbodyType2D.Static;

            // 사망 화면 표시 (1초 후)
            StartCoroutine(DeathSequence());
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            yield return new WaitForSeconds(1f);
            UI.DeathScreen.Instance?.Show();
        }
    }
}
