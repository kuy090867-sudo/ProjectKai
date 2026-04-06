using UnityEngine;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 튜토리얼 텍스트 — 트리거 박스 기반.
    /// 플레이어가 트리거에 진입하면 페이드 인, 빠져나가면 페이드 아웃.
    /// Stage1_1 에서 자동으로 3개 생성 (이동/점프/공격).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class TutorialText : MonoBehaviour
    {
        [SerializeField] private string _text = "A/D: 이동";
        [SerializeField] private float _fadeSpeed = 4f;
        [SerializeField] private Vector2 _triggerSize = new Vector2(4f, 3f);
        [SerializeField] private Vector3 _textOffset = new Vector3(0f, 2f, 0f);

        private TextMeshPro _tmp;
        private float _targetAlpha;

        private void Awake()
        {
            // 트리거 콜라이더 설정
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = _triggerSize;
        }

        private void Start()
        {
            // 텍스트를 자식 오브젝트로 생성 (오프셋 적용)
            var textObj = new GameObject("TutorialLabel");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = _textOffset;

            _tmp = textObj.AddComponent<TextMeshPro>();
            _tmp.text = _text;
            _tmp.fontSize = 4f;
            _tmp.color = new Color(1f, 1f, 1f, 0f);
            _tmp.alignment = TextAlignmentOptions.Center;
            _tmp.sortingOrder = 10;

            // RectTransform 크기 설정 (텍스트 잘림 방지)
            var rt = _tmp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(8f, 2f);
        }

        private void Update()
        {
            if (_tmp == null) return;

            var c = _tmp.color;
            c.a = Mathf.MoveTowards(c.a, _targetAlpha, _fadeSpeed * Time.deltaTime);
            _tmp.color = c;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _targetAlpha = 1f;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _targetAlpha = 0f;
        }

        // ── 팩토리 ──────────────────────────────────────────

        /// <summary>
        /// 월드 좌표에 TutorialText 트리거를 생성합니다.
        /// </summary>
        public static TutorialText Create(float x, float y, string text)
        {
            var obj = new GameObject($"Tutorial_{text}");
            obj.transform.position = new Vector3(x, y, 0f);

            // Rigidbody2D 필요 (트리거 감지용, Kinematic)
            var rb = obj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var tt = obj.AddComponent<TutorialText>();

            // SerializeField에 직접 접근 (같은 클래스이므로 가능)
            tt._text = text;

            return tt;
        }

        // ── Stage1_1 자동 생성 ──────────────────────────────

        /// <summary>
        /// Stage1_1 씬 로드 시 조작법 안내 텍스트 3개를 자동 배치합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSpawnForStage1_1()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Stage1_1 씬에서만 동작
            if (sceneName != "Stage1_1") return;

            // 바닥 높이 기준 (플레이어 스폰 y 근처)
            const float groundY = 0f;

            Create(3f,  groundY, "A/D: 이동");
            Create(8f,  groundY, "Space: 점프");
            Create(15f, groundY, "J: 공격");

            Debug.Log("[TutorialText] Stage1_1 튜토리얼 텍스트 3개 자동 생성");
        }
    }
}
