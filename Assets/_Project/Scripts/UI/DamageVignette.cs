using UnityEngine;
using UnityEngine.UI;
using ProjectKai.Player;

namespace ProjectKai.UI
{
    /// <summary>
    /// 피격 시 화면 가장자리 빨간 비네팅.
    /// 체력이 낮을수록 강하게 표시.
    /// </summary>
    public class DamageVignette : MonoBehaviour
    {
        public static DamageVignette Instance { get; private set; }

        private Image _vignetteImage;
        private float _flashAlpha;
        private float _lowHealthAlpha;
        private PlayerController _player;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateUI();
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("VignetteCanvas");
            canvasObj.transform.SetParent(transform);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 8;

            var imgObj = new GameObject("Vignette");
            imgObj.transform.SetParent(canvasObj.transform, false);
            var rect = imgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _vignetteImage = imgObj.AddComponent<Image>();
            _vignetteImage.color = new Color(0.8f, 0f, 0f, 0f);
            _vignetteImage.raycastTarget = false;
        }

        public void Flash()
        {
            _flashAlpha = 0.4f;
        }

        private void Update()
        {
            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _player = p.GetComponent<PlayerController>();
            }

            // 피격 플래시 감쇠
            _flashAlpha = Mathf.Max(_flashAlpha - Time.deltaTime * 3f, 0f);

            // 체력 낮을 때 지속 비네팅
            _lowHealthAlpha = 0f;
            if (_player != null && _player.IsAlive)
            {
                float hpPercent = _player.CurrentHealth / 100f;
                if (hpPercent < 0.3f)
                    _lowHealthAlpha = (0.3f - hpPercent) * 0.5f * (1f + Mathf.Sin(Time.time * 3f) * 0.3f);
            }

            float alpha = Mathf.Max(_flashAlpha, _lowHealthAlpha);
            if (_vignetteImage != null)
                _vignetteImage.color = new Color(0.8f, 0f, 0f, alpha);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("DamageVignette");
                obj.AddComponent<DamageVignette>();
            }
        }
    }
}
