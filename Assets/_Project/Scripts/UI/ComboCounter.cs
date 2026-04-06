using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 콤보 카운터. 연속 타격 시 화면에 콤보 수 표시.
    /// DMC 스타일.
    /// </summary>
    public class ComboCounter : MonoBehaviour
    {
        public static ComboCounter Instance { get; private set; }

        private TextMeshProUGUI _comboText;
        private TextMeshProUGUI _rankText;
        private int _comboCount;
        private float _comboTimer;
        private float _comboTimeout = 2f;
        private Canvas _canvas;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateUI();
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("ComboCanvas");
            canvasObj.transform.SetParent(transform);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 12;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // 콤보 수
            var countObj = new GameObject("ComboCount");
            countObj.transform.SetParent(canvasObj.transform, false);
            var countRect = countObj.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.82f, 0.6f);
            countRect.anchorMax = new Vector2(0.98f, 0.75f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
            _comboText = countObj.AddComponent<TextMeshProUGUI>();
            _comboText.fontSize = 40;
            _comboText.color = new Color(1f, 0.9f, 0.3f, 0f);
            _comboText.alignment = TextAlignmentOptions.Right;
            _comboText.fontStyle = FontStyles.Bold;

            // 랭크
            var rankObj = new GameObject("ComboRank");
            rankObj.transform.SetParent(canvasObj.transform, false);
            var rankRect = rankObj.AddComponent<RectTransform>();
            rankRect.anchorMin = new Vector2(0.82f, 0.75f);
            rankRect.anchorMax = new Vector2(0.98f, 0.85f);
            rankRect.offsetMin = Vector2.zero;
            rankRect.offsetMax = Vector2.zero;
            _rankText = rankObj.AddComponent<TextMeshProUGUI>();
            _rankText.fontSize = 24;
            _rankText.color = new Color(1f, 0.7f, 0.2f, 0f);
            _rankText.alignment = TextAlignmentOptions.Right;
        }

        public void AddHit()
        {
            _comboCount++;
            _comboTimer = _comboTimeout;

            _comboText.text = $"{_comboCount} HIT";
            _comboText.color = new Color(1f, 0.9f, 0.3f, 1f);

            // 랭크
            string rank = _comboCount switch
            {
                >= 20 => "SSS",
                >= 15 => "SS",
                >= 10 => "S",
                >= 7 => "A",
                >= 5 => "B",
                >= 3 => "C",
                _ => ""
            };
            _rankText.text = rank;
            _rankText.color = new Color(1f, 0.7f, 0.2f, rank.Length > 0 ? 1f : 0f);

            // 사이즈 펀치
            _comboText.transform.localScale = Vector3.one * 1.3f;
        }

        private void Update()
        {
            if (_comboCount <= 0) return;

            _comboTimer -= Time.deltaTime;

            // 스케일 복원
            _comboText.transform.localScale = Vector3.Lerp(
                _comboText.transform.localScale, Vector3.one, Time.deltaTime * 8f);

            if (_comboTimer <= 0f)
            {
                _comboCount = 0;
                _comboText.color = new Color(1f, 0.9f, 0.3f, 0f);
                _rankText.color = new Color(1f, 0.7f, 0.2f, 0f);
            }
            else if (_comboTimer < 0.5f)
            {
                // 페이드 아웃
                float a = _comboTimer / 0.5f;
                _comboText.color = new Color(1f, 0.9f, 0.3f, a);
                _rankText.color = new Color(1f, 0.7f, 0.2f, a);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("ComboCounter");
                obj.AddComponent<ComboCounter>();
            }
        }
    }
}
