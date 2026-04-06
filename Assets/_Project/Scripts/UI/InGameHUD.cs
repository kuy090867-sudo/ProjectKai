using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectKai.Core;
using ProjectKai.Player;

namespace ProjectKai.UI
{
    /// <summary>
    /// 인게임 HUD: HP바(좌상단) + 레벨 + 스테이지명.
    /// </summary>
    public class InGameHUD : MonoBehaviour
    {
        public static InGameHUD Instance { get; private set; }

        private Image _hpFill;
        private Image _mpFill;
        private Image _expFill;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _weaponText;
        private TextMeshProUGUI _potionText;
        private PlayerController _player;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            CreateHUD();
        }

        private void Update()
        {
            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _player = p.GetComponent<PlayerController>();
            }

            if (_player != null && _hpFill != null)
            {
                float maxHp = _player.MaxHealth;
                float hpRatio = maxHp > 0f ? Mathf.Clamp01(_player.CurrentHealth / maxHp) : 0f;
                _hpFill.fillAmount = hpRatio;
                // 체력에 따른 색상 변화: 초록 → 노랑 → 빨강
                if (hpRatio > 0.5f)
                    _hpFill.color = Color.Lerp(new Color(0.9f, 0.9f, 0.2f), new Color(0.2f, 0.9f, 0.3f), (hpRatio - 0.5f) * 2f);
                else
                    _hpFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.9f, 0.9f, 0.2f), hpRatio * 2f);
            }

            if (_levelText != null && ProgressionSystem.Instance != null)
                _levelText.text = $"Lv.{ProgressionSystem.Instance.Level}";

            if (_weaponText != null && _player != null)
                _weaponText.text = _player.IsMelee ? "검" : "총";

            if (_mpFill != null && ManaSystem.Instance != null)
                _mpFill.fillAmount = ManaSystem.Instance.ManaPercent;

            if (_expFill != null && ProgressionSystem.Instance != null)
            {
                int exp = ProgressionSystem.Instance.Experience;
                int needed = ProgressionSystem.Instance.Level * 100;
                _expFill.fillAmount = needed > 0 ? (float)exp / needed : 0f;
            }

            if (_potionText != null)
            {
                var inv = Core.InventorySystem.Instance;
                if (inv != null)
                    _potionText.text = $"포션: {inv.PotionCount}/{inv.PotionMax}";
            }
        }

        private void CreateHUD()
        {
            var canvasObj = new GameObject("HUDCanvas");
            canvasObj.transform.SetParent(transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // HP 배경
            var hpBg = new GameObject("HPBg");
            hpBg.transform.SetParent(canvasObj.transform, false);
            var hpBgRect = hpBg.AddComponent<RectTransform>();
            hpBgRect.anchorMin = new Vector2(0.02f, 0.92f);
            hpBgRect.anchorMax = new Vector2(0.22f, 0.97f);
            hpBgRect.offsetMin = Vector2.zero;
            hpBgRect.offsetMax = Vector2.zero;
            hpBg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // HP Fill
            var hpFillObj = new GameObject("HPFill");
            hpFillObj.transform.SetParent(hpBg.transform, false);
            var hpFillRect = hpFillObj.AddComponent<RectTransform>();
            hpFillRect.anchorMin = new Vector2(0.02f, 0.1f);
            hpFillRect.anchorMax = new Vector2(0.98f, 0.9f);
            hpFillRect.offsetMin = Vector2.zero;
            hpFillRect.offsetMax = Vector2.zero;
            _hpFill = hpFillObj.AddComponent<Image>();
            _hpFill.color = new Color(0.2f, 0.9f, 0.3f);
            _hpFill.type = Image.Type.Filled;
            _hpFill.fillMethod = Image.FillMethod.Horizontal;

            // MP 배경
            var mpBg = new GameObject("MPBg");
            mpBg.transform.SetParent(canvasObj.transform, false);
            var mpBgRect = mpBg.AddComponent<RectTransform>();
            mpBgRect.anchorMin = new Vector2(0.02f, 0.88f);
            mpBgRect.anchorMax = new Vector2(0.18f, 0.92f);
            mpBgRect.offsetMin = Vector2.zero;
            mpBgRect.offsetMax = Vector2.zero;
            mpBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 0.8f);

            // MP Fill
            var mpFillObj = new GameObject("MPFill");
            mpFillObj.transform.SetParent(mpBg.transform, false);
            var mpFillRect = mpFillObj.AddComponent<RectTransform>();
            mpFillRect.anchorMin = new Vector2(0.02f, 0.1f);
            mpFillRect.anchorMax = new Vector2(0.98f, 0.9f);
            mpFillRect.offsetMin = Vector2.zero;
            mpFillRect.offsetMax = Vector2.zero;
            _mpFill = mpFillObj.AddComponent<Image>();
            _mpFill.color = new Color(0.3f, 0.4f, 0.9f);
            _mpFill.type = Image.Type.Filled;
            _mpFill.fillMethod = Image.FillMethod.Horizontal;

            // EXP 바 (HP 아래 가는 바)
            var expBg = new GameObject("EXPBg");
            expBg.transform.SetParent(canvasObj.transform, false);
            var expBgRect = expBg.AddComponent<RectTransform>();
            expBgRect.anchorMin = new Vector2(0.02f, 0.865f);
            expBgRect.anchorMax = new Vector2(0.22f, 0.88f);
            expBgRect.offsetMin = Vector2.zero;
            expBgRect.offsetMax = Vector2.zero;
            expBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 0.6f);

            var expFillObj = new GameObject("EXPFill");
            expFillObj.transform.SetParent(expBg.transform, false);
            var expFillRect = expFillObj.AddComponent<RectTransform>();
            expFillRect.anchorMin = new Vector2(0.01f, 0.1f);
            expFillRect.anchorMax = new Vector2(0.99f, 0.9f);
            expFillRect.offsetMin = Vector2.zero;
            expFillRect.offsetMax = Vector2.zero;
            _expFill = expFillObj.AddComponent<Image>();
            _expFill.color = new Color(0.9f, 0.8f, 0.2f);
            _expFill.type = Image.Type.Filled;
            _expFill.fillMethod = Image.FillMethod.Horizontal;

            // 레벨
            var lvObj = new GameObject("Level");
            lvObj.transform.SetParent(canvasObj.transform, false);
            var lvRect = lvObj.AddComponent<RectTransform>();
            lvRect.anchorMin = new Vector2(0.02f, 0.87f);
            lvRect.anchorMax = new Vector2(0.12f, 0.92f);
            lvRect.offsetMin = Vector2.zero;
            lvRect.offsetMax = Vector2.zero;
            _levelText = lvObj.AddComponent<TextMeshProUGUI>();
            _levelText.text = "Lv.1";
            _levelText.fontSize = 18;
            _levelText.color = new Color(0.9f, 0.8f, 0.5f);

            // 무기 표시 (우하단)
            var wpnObj = new GameObject("Weapon");
            wpnObj.transform.SetParent(canvasObj.transform, false);
            var wpnRect = wpnObj.AddComponent<RectTransform>();
            wpnRect.anchorMin = new Vector2(0.88f, 0.03f);
            wpnRect.anchorMax = new Vector2(0.98f, 0.08f);
            wpnRect.offsetMin = Vector2.zero;
            wpnRect.offsetMax = Vector2.zero;
            _weaponText = wpnObj.AddComponent<TextMeshProUGUI>();
            _weaponText.text = "검";
            _weaponText.fontSize = 18;
            _weaponText.color = new Color(0.8f, 0.8f, 0.9f);
            _weaponText.alignment = TextAlignmentOptions.Right;

            // 포션 카운트 (우하단, 무기 표시 위)
            var potObj = new GameObject("Potion");
            potObj.transform.SetParent(canvasObj.transform, false);
            var potRect = potObj.AddComponent<RectTransform>();
            potRect.anchorMin = new Vector2(0.85f, 0.08f);
            potRect.anchorMax = new Vector2(0.98f, 0.13f);
            potRect.offsetMin = Vector2.zero;
            potRect.offsetMax = Vector2.zero;
            _potionText = potObj.AddComponent<TextMeshProUGUI>();
            _potionText.text = "포션: 3/5";
            _potionText.fontSize = 16;
            _potionText.color = new Color(0.4f, 0.9f, 0.5f);
            _potionText.alignment = TextAlignmentOptions.Right;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("InGameHUD");
                obj.AddComponent<InGameHUD>();
            }
        }
    }
}
