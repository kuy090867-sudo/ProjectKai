using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 설정 메뉴. SFX/BGM 볼륨 조절.
    /// PauseMenu 또는 MainMenu에서 접근.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        public static SettingsMenu Instance { get; private set; }

        private GameObject _panel;
        private bool _isOpen;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("SettingsCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            _panel = new GameObject("BG");
            _panel.transform.SetParent(canvasObj.transform, false);
            var bgRect = _panel.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.2f, 0.2f);
            bgRect.anchorMax = new Vector2(0.8f, 0.8f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            AddText(_panel.transform, "설정", 32, new Color(0.9f, 0.8f, 0.5f),
                new Vector2(0.3f, 0.85f), new Vector2(0.7f, 0.95f));

            // SFX 볼륨
            AddText(_panel.transform, "효과음", 20, Color.white,
                new Vector2(0.1f, 0.65f), new Vector2(0.35f, 0.75f));
            CreateSlider(_panel.transform, new Vector2(0.4f, 0.65f), new Vector2(0.9f, 0.75f),
                Core.AudioManager.Instance?.sfxVolume ?? 0.7f,
                v => { if (Core.AudioManager.Instance != null) Core.AudioManager.Instance.sfxVolume = v; });

            // BGM 볼륨
            AddText(_panel.transform, "배경음", 20, Color.white,
                new Vector2(0.1f, 0.5f), new Vector2(0.35f, 0.6f));
            CreateSlider(_panel.transform, new Vector2(0.4f, 0.5f), new Vector2(0.9f, 0.6f),
                Core.AudioManager.Instance?.bgmVolume ?? 0.5f,
                v => { if (Core.AudioManager.Instance != null) Core.AudioManager.Instance.bgmVolume = v; });

            // 닫기 버튼
            var closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(_panel.transform, false);
            var closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.3f, 0.1f);
            closeRect.anchorMax = new Vector2(0.7f, 0.22f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            closeObj.AddComponent<Image>().color = new Color(0.3f, 0.25f, 0.35f);
            closeObj.AddComponent<Button>().onClick.AddListener(Close);

            AddText(closeObj.transform, "닫기", 22, Color.white, Vector2.zero, Vector2.one);
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
            slider.value = value;
            slider.onValueChanged.AddListener(onChange);

            // Background
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            var bgR = bgObj.AddComponent<RectTransform>();
            bgR.anchorMin = new Vector2(0, 0.4f); bgR.anchorMax = new Vector2(1, 0.6f);
            bgR.offsetMin = Vector2.zero; bgR.offsetMax = Vector2.zero;
            bgObj.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);
            slider.fillRect = null;

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
        }

        private void AddText(Transform parent, string text, float size, Color color, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject("T");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
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
