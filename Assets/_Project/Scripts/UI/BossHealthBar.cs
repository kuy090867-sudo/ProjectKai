using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ProjectKai.Combat;

namespace ProjectKai.UI
{
    /// <summary>
    /// 보스 체력바. 화면 하단 고정.
    /// 보스 이름 + 체력바 + 페이즈 표시.
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        private Image _fill;
        private TextMeshProUGUI _nameText;
        private GameObject _canvasObj;

        public static BossHealthBar Create(string bossName, DamageReceiver dr)
        {
            var obj = new GameObject("BossHealthBar");
            var bhb = obj.AddComponent<BossHealthBar>();
            bhb.Build(bossName);

            if (dr != null)
            {
                dr.OnHealthChanged += (cur, max) => bhb.UpdateHealth(cur, max);
                dr.OnDeath += () => bhb.Remove();
            }

            DontDestroyOnLoad(obj);
            return bhb;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 보스 씬이 아닌 곳에서는 자동 제거
            if (!scene.name.Contains("Boss"))
                Remove();
        }

        private void Build(string bossName)
        {
            _canvasObj = new GameObject("BossHPCanvas");
            _canvasObj.transform.SetParent(transform);
            var canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;
            var scaler = _canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 배경 바
            var bg = new GameObject("BG");
            bg.transform.SetParent(_canvasObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.2f, 0.03f);
            bgRect.anchorMax = new Vector2(0.8f, 0.07f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.15f, 0.1f, 0.1f, 0.9f);

            // Fill 바
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bg.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.01f, 0.15f);
            fillRect.anchorMax = new Vector2(0.99f, 0.85f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            _fill = fillObj.AddComponent<Image>();
            _fill.color = new Color(0.8f, 0.15f, 0.15f);
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;

            // 이름
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(_canvasObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.3f, 0.07f);
            nameRect.anchorMax = new Vector2(0.7f, 0.11f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            _nameText = nameObj.AddComponent<TextMeshProUGUI>();
            _nameText.text = bossName;
            _nameText.fontSize = 20;
            _nameText.color = new Color(1f, 0.85f, 0.5f);
            _nameText.alignment = TextAlignmentOptions.Center;
        }

        public void UpdateHealth(float current, float max)
        {
            if (_fill != null)
                _fill.fillAmount = Mathf.Clamp01(current / max);
        }

        public void Remove()
        {
            if (_canvasObj != null) Destroy(_canvasObj);
            Destroy(gameObject);
        }
    }
}
