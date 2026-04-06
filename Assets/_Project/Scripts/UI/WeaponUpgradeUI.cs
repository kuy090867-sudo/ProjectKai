using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectKai.Core;

namespace ProjectKai.UI
{
    /// <summary>
    /// 무기 강화 UI. Hub NPC 상호작용 시 표시.
    /// 정적 Show()/Hide()로 제어. 열리면 Time.timeScale = 0.
    /// </summary>
    public class WeaponUpgradeUI : MonoBehaviour
    {
        public static WeaponUpgradeUI Instance { get; private set; }

        private GameObject _panel;
        private TextMeshProUGUI _swordLevelText;
        private TextMeshProUGUI _swordCostText;
        private TextMeshProUGUI _gunLevelText;
        private TextMeshProUGUI _gunCostText;
        private TextMeshProUGUI _goldText;
        private TextMeshProUGUI _feedbackText;
        private Button _swordUpgradeBtn;
        private Button _gunUpgradeBtn;
        private Button _closeBtn;
        private float _feedbackTimer;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateUI();
            _panel.SetActive(false);
        }

        private void Update()
        {
            if (_panel == null || !_panel.activeSelf) return;

            // ESC로 닫기
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Hide();
            }

            // 피드백 텍스트 페이드 아웃
            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.unscaledDeltaTime;
                if (_feedbackTimer <= 0f && _feedbackText != null)
                    _feedbackText.text = "";
            }
        }

        // ═══════════════════════════════════════
        //  정적 Show / Hide
        // ═══════════════════════════════════════

        public static void Show()
        {
            EnsureInstance();
            if (Instance == null) return;
            Instance._panel.SetActive(true);
            Instance.RefreshUI();
            Time.timeScale = 0f;
        }

        public static void Hide()
        {
            if (Instance == null || Instance._panel == null) return;
            Instance._panel.SetActive(false);
            Time.timeScale = 1f;
        }

        private static void EnsureInstance()
        {
            if (Instance != null) return;
            var obj = new GameObject("WeaponUpgradeUI");
            obj.AddComponent<WeaponUpgradeUI>();
        }

        // ═══════════════════════════════════════
        //  UI 생성
        // ═══════════════════════════════════════

        private void CreateUI()
        {
            // 캔버스
            var canvasObj = new GameObject("UpgradeCanvas");
            canvasObj.transform.SetParent(transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 패널 (반투명 배경 + 중앙 정렬)
            _panel = new GameObject("UpgradePanel");
            _panel.transform.SetParent(canvasObj.transform, false);
            var panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.6f);

            // 내부 프레임 (중앙 박스)
            var frame = CreateChild(_panel, "Frame");
            var frameRect = frame.AddComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(0.25f, 0.2f);
            frameRect.anchorMax = new Vector2(0.75f, 0.8f);
            frameRect.offsetMin = Vector2.zero;
            frameRect.offsetMax = Vector2.zero;
            var frameImg = frame.AddComponent<Image>();
            frameImg.color = new Color(0.12f, 0.1f, 0.15f, 0.95f);

            // 제목
            var title = CreateTMP(frame, "Title", "무기 강화", 32,
                new Color(1f, 0.85f, 0.4f), TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.95f));

            // 골드 표시
            _goldText = CreateTMP(frame, "GoldText", "보유 골드: 0", 22,
                new Color(1f, 0.9f, 0.3f), TextAlignmentOptions.Center,
                new Vector2(0.2f, 0.77f), new Vector2(0.8f, 0.85f));

            // ─── 검 강화 영역 ───
            var swordHeader = CreateTMP(frame, "SwordHeader", "--- 검 ---", 24,
                new Color(0.8f, 0.6f, 0.4f), TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.73f));

            _swordLevelText = CreateTMP(frame, "SwordLevel", "검 레벨: 1", 20,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.57f), new Vector2(0.5f, 0.65f));

            _swordCostText = CreateTMP(frame, "SwordCost", "비용: 100G", 20,
                new Color(1f, 0.9f, 0.5f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.57f), new Vector2(0.9f, 0.65f));

            _swordUpgradeBtn = CreateButton(frame, "SwordUpgradeBtn", "검 강화",
                new Vector2(0.3f, 0.48f), new Vector2(0.7f, 0.57f),
                new Color(0.6f, 0.35f, 0.2f));
            _swordUpgradeBtn.onClick.AddListener(OnSwordUpgrade);

            // ─── 총 강화 영역 ───
            var gunHeader = CreateTMP(frame, "GunHeader", "--- 총 ---", 24,
                new Color(0.4f, 0.6f, 0.8f), TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.37f), new Vector2(0.9f, 0.45f));

            _gunLevelText = CreateTMP(frame, "GunLevel", "총 레벨: 1", 20,
                Color.white, TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.29f), new Vector2(0.5f, 0.37f));

            _gunCostText = CreateTMP(frame, "GunCost", "비용: 100G", 20,
                new Color(1f, 0.9f, 0.5f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.29f), new Vector2(0.9f, 0.37f));

            _gunUpgradeBtn = CreateButton(frame, "GunUpgradeBtn", "총 강화",
                new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.29f),
                new Color(0.2f, 0.35f, 0.6f));
            _gunUpgradeBtn.onClick.AddListener(OnGunUpgrade);

            // 피드백 텍스트
            _feedbackText = CreateTMP(frame, "Feedback", "", 22,
                new Color(0.3f, 1f, 0.4f), TextAlignmentOptions.Center,
                new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.2f));

            // 닫기 버튼 (X)
            _closeBtn = CreateButton(frame, "CloseBtn", "X",
                new Vector2(0.88f, 0.9f), new Vector2(0.98f, 0.98f),
                new Color(0.6f, 0.2f, 0.2f));
            _closeBtn.onClick.AddListener(Hide);
        }

        // ═══════════════════════════════════════
        //  강화 로직
        // ═══════════════════════════════════════

        private void OnSwordUpgrade()
        {
            int cost = WeaponUpgrade.SwordLevel * 100;
            var prog = ProgressionSystem.Instance;
            if (prog == null || prog.Gold < cost)
            {
                ShowFeedback("골드가 부족합니다!", new Color(1f, 0.3f, 0.3f));
                return;
            }

            if (WeaponUpgrade.UpgradeSword())
            {
                ShowFeedback($"검 Lv.{WeaponUpgrade.SwordLevel} 강화 성공!", new Color(0.3f, 1f, 0.4f));
                AudioManager.Instance?.PlaySFX("jump", 0.8f);
                RefreshUI();
            }
        }

        private void OnGunUpgrade()
        {
            int cost = WeaponUpgrade.GunLevel * 100;
            var prog = ProgressionSystem.Instance;
            if (prog == null || prog.Gold < cost)
            {
                ShowFeedback("골드가 부족합니다!", new Color(1f, 0.3f, 0.3f));
                return;
            }

            if (WeaponUpgrade.UpgradeGun())
            {
                ShowFeedback($"총 Lv.{WeaponUpgrade.GunLevel} 강화 성공!", new Color(0.3f, 1f, 0.4f));
                AudioManager.Instance?.PlaySFX("jump", 0.8f);
                RefreshUI();
            }
        }

        private void ShowFeedback(string msg, Color color)
        {
            if (_feedbackText == null) return;
            _feedbackText.text = msg;
            _feedbackText.color = color;
            _feedbackTimer = 2f;
        }

        private void RefreshUI()
        {
            int gold = ProgressionSystem.Instance != null ? ProgressionSystem.Instance.Gold : 0;
            if (_goldText != null) _goldText.text = $"보유 골드: {gold}G";
            if (_swordLevelText != null) _swordLevelText.text = $"검 레벨: {WeaponUpgrade.SwordLevel}";
            if (_swordCostText != null) _swordCostText.text = $"비용: {WeaponUpgrade.SwordLevel * 100}G";
            if (_gunLevelText != null) _gunLevelText.text = $"총 레벨: {WeaponUpgrade.GunLevel}";
            if (_gunCostText != null) _gunCostText.text = $"비용: {WeaponUpgrade.GunLevel * 100}G";
        }

        // ═══════════════════════════════════════
        //  UI 헬퍼
        // ═══════════════════════════════════════

        private GameObject CreateChild(GameObject parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);
            return obj;
        }

        private TextMeshProUGUI CreateTMP(GameObject parent, string name, string text,
            float fontSize, Color color, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var obj = CreateChild(parent, name);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            return tmp;
        }

        private Button CreateButton(GameObject parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Color bgColor)
        {
            var obj = CreateChild(parent, name);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.color = bgColor;

            var btn = obj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            // 버튼 텍스트
            var txtObj = CreateChild(obj, "Text");
            var txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            // 필요 시 자동 생성 (Hub 씬에서만)
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (scene == "Hub") EnsureInstance();
        }
    }
}
