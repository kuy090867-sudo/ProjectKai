using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Core;
using ProjectKai.Data;
using ProjectKai.UI;
using System.Collections;

namespace ProjectKai.Enemy
{
    /// <summary>
    /// 기사단장 — 2장 보스.
    /// 첫 만남에서는 이길 수 없다 (HP 30% 이하로 안 내려감).
    /// "거울 앞에 선 기사"
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class KnightCommander : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float _chargeSpeed = 14f;
        [SerializeField] private float _attackDamage = 25f;
        [SerializeField] private float _attackCooldown = 1.5f;
        [SerializeField] private bool _isUnwinnable = true;
        [SerializeField] private float _minHealthPercent = 0.3f;

        private Rigidbody2D _rb;
        private DamageReceiver _dr;
        private SpriteRenderer _sr;
        private Transform _player;
        private float _fixedY;
        private int _facingDir = -1;
        private bool _defeated;

        private enum Phase { Phase1, Phase2, Phase3 }
        private Phase _phase = Phase.Phase1;
        private float _actionTimer;
        private bool _isActing;

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
            _dr.OnDamaged += OnDamaged;
            _dr.OnHealthChanged += OnHealthChanged;

            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void Update()
        {
            if (_player == null || _defeated) return;

            UpdateFacing();
            _actionTimer += Time.deltaTime;

            if (!_isActing && _actionTimer >= _attackCooldown)
            {
                _actionTimer = 0f;
                ChooseAttack();
            }
        }

        private void ChooseAttack()
        {
            float dist = Vector2.Distance(transform.position, _player.position);
            int r = Random.Range(0, 3);

            if (r == 0 || dist > 5f)
                StartCoroutine(ChargeSlash());
            else if (r == 1)
                StartCoroutine(ComboAttack());
            else
                StartCoroutine(CounterStance());
        }

        private IEnumerator ChargeSlash()
        {
            _isActing = true;
            _sr.color = new Color(1f, 0.4f, 0.2f);
            yield return new WaitForSeconds(0.4f);

            _sr.color = Color.white;
            float timer = 0f;
            while (timer < 0.4f)
            {
                timer += Time.deltaTime;
                _rb.MovePosition(new Vector2(
                    transform.position.x + _facingDir * _chargeSpeed * Time.deltaTime,
                    _fixedY));
                yield return null;
            }

            PerformHit(_attackDamage, 10f);
            AudioManager.Instance?.PlaySFX("sword_hit", 0.8f);
            GameFeel.Instance?.CameraShake(0.15f, 0.15f);

            _isActing = false;
        }

        private IEnumerator ComboAttack()
        {
            _isActing = true;
            for (int i = 0; i < 4; i++)
            {
                _sr.color = new Color(1f, 0.5f, 0.5f);
                yield return new WaitForSeconds(0.15f);
                _sr.color = Color.white;

                PerformHit(_attackDamage * 0.7f, 4f + i * 2f);
                AudioManager.Instance?.PlaySFX("sword_swing", 0.6f);
                yield return new WaitForSeconds(0.2f);
            }
            _isActing = false;
        }

        private IEnumerator CounterStance()
        {
            _isActing = true;
            // 방어 자세 — 노란 빛
            _sr.color = Color.yellow;
            yield return new WaitForSeconds(1f);

            // 카운터 — 순간 이동 후 강타
            if (_player != null)
            {
                float behindX = _player.position.x - _facingDir * 2f;
                _rb.MovePosition(new Vector2(behindX, _fixedY));
                _facingDir *= -1;

                yield return new WaitForSeconds(0.1f);
                _sr.color = Color.white;

                PerformHit(_attackDamage * 1.5f, 15f);
                GameFeel.Instance?.CameraShake(0.2f, 0.2f);
                GameFeel.Instance?.HitStop(0.1f);
                AudioManager.Instance?.PlaySFX("sword_hit", 1f);
            }
            _isActing = false;
        }

        private void OnHealthChanged(float current, float max)
        {
            float ratio = current / max;

            // 이길 수 없는 전투: HP가 minHealthPercent 이하로 안 내려감
            if (_isUnwinnable && ratio <= _minHealthPercent)
            {
                _dr.Heal(max * 0.1f);

                if (!_defeated)
                {
                    _defeated = true;
                    StartCoroutine(UnwinnableDefeatSequence());
                }
            }

            // 페이즈 전환
            if (ratio <= 0.5f && _phase == Phase.Phase1)
            {
                _phase = Phase.Phase2;
                _attackCooldown *= 0.7f;
                GameFeel.Instance?.CameraShake(0.15f, 0.3f);
            }
        }

        private IEnumerator UnwinnableDefeatSequence()
        {
            _isActing = true;
            GameFeel.Instance?.HitStop(0.2f);
            GameFeel.Instance?.CameraShake(0.25f, 0.5f);

            yield return new WaitForSecondsRealtime(0.5f);

            // 기사단장 대사
            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "기사단장", text = "스승님의 기사도는 아름다웠지. 하지만 기사도로 슬럼의 아이들을 살릴 수 있었나?" },
                new DialogueLine { speakerName = "기사단장", text = "권력만이 세상을 바꾼다." },
                new DialogueLine { speakerName = "카이", text = "..." },
            };
            DialogueSystem.Instance?.StartDialogue(lines);

            while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive)
                yield return null;

            // 카이 패배 연출
            yield return new WaitForSeconds(0.5f);

            var defeatLines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "", text = "카이는 쓰러졌다." },
                new DialogueLine { speakerName = "카이", text = "풍차가 아니었어. 진짜 거인이었다." },
                new DialogueLine { speakerName = "리나", text = "그래도 또 돌진할 거잖아." },
                new DialogueLine { speakerName = "카이", text = "당연하지." }
            };
            DialogueSystem.Instance?.StartDialogue(defeatLines);

            while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive)
                yield return null;

            // 거점으로 복귀
            yield return new WaitForSeconds(1f);
            if (SceneExists("Hub"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void PerformHit(float damage, float knockback)
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(_facingDir * 1.2f, 0f);
            var hits = Physics2D.OverlapBoxAll(origin, new Vector2(2f, 1.5f), 0f);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                var dmg = hit.GetComponent<IDamageable>();
                if (dmg != null && dmg.IsAlive)
                    dmg.TakeDamage(damage, new Vector2(_facingDir, 0.2f).normalized, knockback);
            }
        }

        private void OnDamaged(float damage, Vector2 dir) { }

        private void UpdateFacing()
        {
            if (_player == null) return;
            _facingDir = _player.position.x > transform.position.x ? 1 : -1;
            if (_sr != null) _sr.flipX = _facingDir < 0;
        }

        private bool SceneExists(string name)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                if (path.Contains(name)) return true;
            }
            return false;
        }
    }
}
