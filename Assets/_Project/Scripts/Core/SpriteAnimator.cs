using UnityEngine;

namespace ProjectKai.Core
{
    [System.Serializable]
    public class AnimState
    {
        public string name;
        public Sprite[] frames;
        public float fps = 8f;
        public bool loop = true;
        // 보조 효과 (프레임 부족 시 트랜스폼으로 보강)
        public float scaleX = 1f;
        public float scaleY = 1f;
        public float rotationZ = 0f;
    }

    public class SpriteAnimator : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private AnimState _currentState;
        private float _timer;
        private int _currentFrame;
        private string _currentName = "";
        private System.Collections.Generic.Dictionary<string, AnimState> _states;
        private Vector3 _baseScale;

        [Header("Animation Settings")]
        [SerializeField] private string _spriteFolder = "Sprites/Player";
        [SerializeField] private string _prefix = "knight_m";
        [SerializeField] private bool _isEnemy = false;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
            _baseScale = transform.localScale;
            _states = new System.Collections.Generic.Dictionary<string, AnimState>();

            LoadAllAnimations();
            Play("idle");
        }

        private void LoadAllAnimations()
        {
            if (_isEnemy)
            {
                string p = _prefix; // e.g. "goblin"
                RegisterAnim("idle", LoadFrames($"{p}_idle_anim_f", 4), 5f);
                RegisterAnim("run", LoadFrames($"{p}_run_anim_f", 4), 10f);
                // 공격: idle 0번 프레임 + 스케일/회전 효과
                RegisterAnim("attack", LoadFrames($"{p}_idle_anim_f", 1), 8f, false, 1.1f, 1f, -10f);
                RegisterAnim("hit", LoadFrames($"{p}_idle_anim_f", 1), 8f, false, 0.9f, 1.1f, 5f);
                RegisterAnim("death", LoadFrames($"{p}_idle_anim_f", 1), 4f, false, 1f, 0.8f, 15f);
            }
            else
            {
                // Player (knight_m)
                RegisterAnim("idle", LoadFrames($"{_prefix}_idle_anim_f", 4), 5f);
                RegisterAnim("run", LoadFrames($"{_prefix}_run_anim_f", 4), 10f);
                RegisterAnim("hit", LoadFrames($"{_prefix}_hit_anim_f", 1), 8f, false);
                // 점프/낙하: idle 0번 + 세로 스트레치
                RegisterAnim("jump", LoadFrames($"{_prefix}_idle_anim_f", 1), 8f, false, 1f, 1.15f, 0f);
                RegisterAnim("fall", LoadFrames($"{_prefix}_idle_anim_f", 1), 8f, false, 1f, 0.9f, 0f);
                // 공격: hit 프레임 + 회전 효과 (콤보별 다른 각도는 Play 호출 시 지정)
                RegisterAnim("attack", LoadFrames($"{_prefix}_hit_anim_f", 1), 12f, false, 1.05f, 1f, -15f);
                RegisterAnim("attack2", LoadFrames($"{_prefix}_hit_anim_f", 1), 12f, false, 1f, 1.05f, 0f);
                RegisterAnim("attack3", LoadFrames($"{_prefix}_hit_anim_f", 1), 12f, false, 1.1f, 1.1f, 10f);
                // 대시: run 0번 + 가로 스트레치
                RegisterAnim("dash", LoadFrames($"{_prefix}_run_anim_f", 1), 8f, false, 1.2f, 0.85f, 0f);
                RegisterAnim("death", LoadFrames($"{_prefix}_hit_anim_f", 1), 4f, false, 1f, 0.7f, 20f);
            }
        }

        private void RegisterAnim(string name, Sprite[] frames, float fps, bool loop = true,
            float sx = 1f, float sy = 1f, float rz = 0f)
        {
            _states[name] = new AnimState
            {
                name = name, frames = frames, fps = fps, loop = loop,
                scaleX = sx, scaleY = sy, rotationZ = rz
            };
        }

        private Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                var tex = Resources.Load<Texture2D>($"{_spriteFolder}/{prefix}{i}");
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Point;
                    frames[i] = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 16f);
                }
            }
            return frames;
        }

        public void Play(string animName, float fpsOverride = -1f, bool loopOverride = true)
        {
            if (_currentName == animName) return;
            if (!_states.ContainsKey(animName)) return;

            _currentName = animName;
            _currentState = _states[animName];
            _currentFrame = 0;
            _timer = 0f;

            if (fpsOverride > 0) _currentState.fps = fpsOverride;

            // 보조 트랜스폼 효과 적용
            transform.localScale = new Vector3(
                _baseScale.x * _currentState.scaleX,
                _baseScale.y * _currentState.scaleY,
                _baseScale.z);
            transform.localRotation = Quaternion.Euler(0, 0, _currentState.rotationZ);

            if (_currentState.frames != null && _currentState.frames.Length > 0 && _currentState.frames[0] != null)
            {
                _sr.sprite = _currentState.frames[0];
                _sr.color = Color.white;
            }
        }

        /// <summary>
        /// 현재 애니메이션을 강제로 리셋 (같은 이름이어도 다시 재생)
        /// </summary>
        public void ForcePlay(string animName)
        {
            _currentName = "";
            Play(animName);
        }

        private void Update()
        {
            if (_currentState == null || _currentState.frames == null || _currentState.frames.Length <= 1)
                return;

            _timer += Time.deltaTime;
            if (_timer >= 1f / _currentState.fps)
            {
                _timer -= 1f / _currentState.fps;
                _currentFrame++;

                if (_currentFrame >= _currentState.frames.Length)
                {
                    _currentFrame = _currentState.loop ? 0 : _currentState.frames.Length - 1;
                }

                if (_currentState.frames[_currentFrame] != null)
                    _sr.sprite = _currentState.frames[_currentFrame];
            }
        }

        /// <summary>
        /// idle로 리셋 (트랜스폼 효과도 초기화)
        /// </summary>
        public void ResetToIdle()
        {
            transform.localScale = _baseScale;
            transform.localRotation = Quaternion.identity;
            Play("idle");
        }
    }
}
