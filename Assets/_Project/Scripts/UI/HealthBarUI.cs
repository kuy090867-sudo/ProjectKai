using UnityEngine;
using UnityEngine.UI;

namespace ProjectKai.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _bgImage;

        private Transform _target;
        private Vector3 _offset = new Vector3(0f, 1.2f, 0f);
        private float _maxHealth;
        private float _currentHealth;

        public void Initialize(Transform target, float maxHealth, Vector3 offset)
        {
            _target = target;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _offset = offset;
        }

        public void UpdateHealth(float current, float max)
        {
            _currentHealth = current;
            _maxHealth = max;
            if (_fillImage != null)
                _fillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = _target.position + _offset;
        }

        /// <summary>
        /// 코드에서 체력바 UI를 생성하는 팩토리 메서드
        /// </summary>
        public static HealthBarUI CreateHealthBar(Transform target, float maxHealth,
            Vector3 offset, Color fillColor, Vector2 size)
        {
            // Canvas
            var canvasObj = new GameObject("HealthBar_Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            // RectTransform 크기 설정
            var canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = size;
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            // 배경
            var bgObj = new GameObject("BG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(canvasObj.transform, false);
            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1f;
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0.02f, 0.1f);
            fillRect.anchorMax = new Vector2(0.98f, 0.9f);
            fillRect.sizeDelta = Vector2.zero;

            // HealthBarUI 컴포넌트
            var hb = canvasObj.AddComponent<HealthBarUI>();
            hb._fillImage = fillImage;
            hb._bgImage = bgImage;
            hb.Initialize(target, maxHealth, offset);

            return hb;
        }
    }
}
