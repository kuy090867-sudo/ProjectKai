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
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _weaponText;
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
                _hpFill.fillAmount = _player.CurrentHealth / 100f;
            }

            if (_levelText != null && ProgressionSystem.Instance != null)
                _levelText.text = $"Lv.{ProgressionSystem.Instance.Level}";

            if (_weaponText != null && _player != null)
                _weaponText.text = _player.IsMelee ? "⚔ 검" : "🔫 총";
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
