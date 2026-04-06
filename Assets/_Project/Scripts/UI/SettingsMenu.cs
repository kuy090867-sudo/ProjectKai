using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 설정 메뉴. SFX/BGM 볼륨 슬라이더 + 화면 모드 토글.
    /// PauseMenu 또는 MainMenu에서 접근.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        public static SettingsMenu Instance { get; private set; }

        private GameObject _panel;
        private bool _isOpen;

        private TextMeshProUGUI _sfxLabel;
        private TextMeshProUGUI _bgmLabel;
        private TextMeshProUGUI _screenModeLabel;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>정적 호출용. 인스턴스가 없으면 자동 생성 후 열기.</summary>
        public static void Show()
        {
            if (Instance == null)
            {
                var obj = new GameObject("SettingsMenu");
                obj.AddComponent<SettingsMenu>();
            }
            Instance.Open();
        }

        public void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            CreateUI();
        }

        public void Close()
        {
            _isOpen = false;
            if (_panel != null)
            {
                Destroy(_panel.transform.root.gameObject);
                _panel = null;
            }
            _sfxLabel = null;
            _bgmLabel = null;
            _screenModeLabel = null;
        }

        private void CreateUI()
        {
            // --- Canvas ---
            var canvasObj = new GameObject("SettingsCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // --- 전체 화면 반투명 배경 (클릭 블로킹) ---
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasObj.transform, false);
            var ovRect = overlay.AddComponent<RectTransform>();
            ovRect.anchorMin = Vector2.zero;
            ovRect.anchorMax = Vector2.one;
            ovRect.offsetMin = Vector2.zero;
            ovRect.offsetMax = Vector2.zero;
            overlay.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            // --- 패널 ---
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(canvasObj.transform, false);
            var bgRect = _panel.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.2f, 0.15f);
            bgRect.anchorMax = new Vector2(0.8f, 0.85f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // --- 제목: "설정" ---
            AddText(_panel.transform, "설정", 36, new Color(0.9f, 0.8f, 0.5f),
                new Vector2(0.3f, 0.88f), new Vector2(0.7f, 0.97f));

            // ===== SFX 볼륨 =====
            float currentSfx = Core.AudioManager.Instance != null
                ? Core.AudioManager.Instance.SfxVolume : 0.7f;

            AddText(_panel.transform, "효과음", 22, Color.white,
                new Vector2(0.05f, 0.72f), new Vector2(0.25f, 0.82f));

            _sfxLabel = AddText(_panel.transform,
                Mathf.RoundToInt(currentSfx * 100) + "%", 20, new Color(0.8f, 0.7f, 0.4f),
                new Vector2(0.82f, 0.72f), new Vector2(0.95f, 0.82f))
                .GetComponent<TextMeshProUGUI>();

            CreateSlider(_panel.transform,
                new Vector2(0.27f, 0.72f), new Vector2(0.8f, 0.82f),
                currentSfx,
                v =>
                {
                    if (Core.AudioManager.Instance != null)
                        Core.AudioManager.Instance.SfxVolume = v;
                    if (_sfxLabel != null)
                        _sfxLabel.text = Mathf.RoundToInt(v * 100) + "%";
                });

            // ===== BGM 볼륨 =====
            float currentBgm = Core.AudioManager.Instance != null
                ? Core.AudioManager.Instance.BgmVolume : 0.5f;

            AddText(_panel.transform, "배경음", 22, Color.white,
                new Vector2(0.05f, 0.57f), new Vector2(0.25f, 0.67f));

            _bgmLabel = AddText(_panel.transform,
                Mathf.RoundToInt(currentBgm * 100) + "%", 20, new Color(0.8f, 0.7f, 0.4f),
                new Vector2(0.82f, 0.57f), new Vector2(0.95f, 0.67f))
                .GetComponent<TextMeshProUGUI>();

            CreateSlider(_panel.transform,
                new Vector2(0.27f, 0.57f), new Vector2(0.8f, 0.67f),
                currentBgm,
                v =>
                {
                    if (Core.AudioManager.Instance != null)
                        Core.AudioManager.Instance.BgmVolume = v;
                    if (_bgmLabel != null)
                        _bgmLabel.text = Mathf.RoundToInt(v * 100) + "%";
                });

            // ===== 화면 모드 토글 =====
            AddText(_panel.transform, "화면 모드", 22, Color.white,
                new Vector2(0.05f, 0.42f), new Vector2(0.3f, 0.52f));

            bool isFullscreen = Screen.fullScreen;
            _screenModeLabel = AddText(_panel.transform,
                isFullscreen ? "전체화면" : "창모드", 20, new Color(0.8f, 0.7f, 0.4f),
                new Vector2(0.55f, 0.42f), new Vector2(0.75f, 0.52f))
                .GetComponent<TextMeshProUGUI>();

            var toggleBtnObj = new GameObject("ScreenToggleBtn");
            toggleBtnObj.transform.SetParent(_panel.transform, false);
            var toggleRect = toggleBtnObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.32f, 0.42f);
            toggleRect.anchorMax = new Vector2(0.53f, 0.52f);
            toggleRect.offsetMin = Vector2.zero;
            toggleRect.offsetMax = Vector2.zero;
            toggleBtnObj.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.35f);
            toggleBtnObj.AddComponent<Button>().onClick.AddListener(() =>
            {
                Screen.fullScreen = !Screen.fullScreen;
                if (_screenModeLabel != null)
                    _screenModeLabel.text = Screen.fullScreen ? "창모드" : "전체화면";
                // 토글 직후에는 반대값이 표시됨 (Screen.fullScreen은 다음 프레임에 적용)
            });
            AddText(toggleBtnObj.transform, "전환", 18, Color.white, Vector2.zero, Vector2.one);

            // ===== 닫기 버튼 =====
            var closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(_panel.transform, false);
            var closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.3f, 0.08f);
            closeRect.anchorMax = new Vector2(0.7f, 0.2f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            closeObj.AddComponent<Image>().color = new Color(0.3f, 0.25f, 0.35f);
            var closeBtn = closeObj.AddComponent<Button>();
            closeBtn.onClick.AddListener(Close);
            var closeBtnColors = closeBtn.colors;
            closeBtnColors.highlightedColor = new Color(0.4f, 0.35f, 0.5f);
            closeBtnColors.pressedColor = new Color(0.5f, 0.4f, 0.6f);
            closeBtn.colors = closeBtnColors;

            AddText(closeObj.transform, "닫기", 24, Color.white, Vector2.zero, Vector2.one);
        }

        private void CreateSlider(Transform parent, Vector2 aMin, Vector2 aMax, float value,
            UnityEngine.Events.UnityAction<float> onChange)
        {
            var obj = new GameObject("Slider");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            var slider = obj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            var bgR = bgObj.AddComponent<RectTransform>();
            bgR.anchorMin = new Vector2(0, 0.35f); bgR.anchorMax = new Vector2(1, 0.65f);
            bgR.offsetMin = Vector2.zero; bgR.offsetMax = Vector2.zero;
            bgObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);

            // Fill Area
            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(obj.transform, false);
            var faR = fillArea.AddComponent<RectTransform>();
            faR.anchorMin = new Vector2(0, 0.35f); faR.anchorMax = new Vector2(1, 0.65f);
            faR.offsetMin = Vector2.zero; faR.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fR = fill.AddComponent<RectTransform>();
            fR.anchorMin = Vector2.zero; fR.anchorMax = Vector2.one;
            fR.offsetMin = Vector2.zero; fR.offsetMax = Vector2.zero;
            fill.AddComponent<Image>().color = new Color(0.6f, 0.5f, 0.3f, 0.5f);
            slider.fillRect = fR;

            // Handle
            var handleArea = new GameObject("HandleArea");
            handleArea.transform.SetParent(obj.transform, false);
            var haR = handleArea.AddComponent<RectTransform>();
            haR.anchorMin = Vector2.zero; haR.anchorMax = Vector2.one;
            haR.offsetMin = Vector2.zero; haR.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var hR = handle.AddComponent<RectTransform>();
            hR.sizeDelta = new Vector2(20, 0);
            handle.AddComponent<Image>().color = new Color(0.8f, 0.7f, 0.4f);

            slider.handleRect = hR;
            slider.targetGraphic = handle.GetComponent<Image>();

            // 값 설정 & 리스너 (리스너 전에 value 설정하면 콜백 발생 방지)
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(onChange);
        }

        private GameObject AddText(Transform parent, string text, float size, Color color,
            Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject("T");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return obj;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("SettingsMenu");
                obj.AddComponent<SettingsMenu>();
            }
        }
    }
}
