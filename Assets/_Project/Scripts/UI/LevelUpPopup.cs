using UnityEngine;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 레벨업 팝업. 화면 중앙에 "LEVEL UP!" 표시 후 페이드 아웃.
    /// </summary>
    public class LevelUpPopup : MonoBehaviour
    {
        public static LevelUpPopup Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Core.ProgressionSystem.Instance != null)
                Core.ProgressionSystem.Instance.OnLevelUp += Show;
        }

        private void OnDestroy()
        {
            if (Core.ProgressionSystem.Instance != null)
                Core.ProgressionSystem.Instance.OnLevelUp -= Show;
        }

        public void Show(int level)
        {
            StartCoroutine(ShowPopup(level));
        }

        private System.Collections.IEnumerator ShowPopup(int level)
        {
            var canvasObj = new GameObject("LevelUpCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 25;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform, false);
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.25f, 0.4f);
            rect.anchorMax = new Vector2(0.75f, 0.6f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"LEVEL UP!\nLv.{level}";
            tmp.fontSize = 48;
            tmp.color = new Color(1f, 0.9f, 0.3f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            Core.AudioManager.Instance?.PlaySFX("jump", 0.8f);
            Core.GameFeel.Instance?.CameraShake(0.1f, 0.2f);

            // 크기 애니메이션
            float timer = 0f;
            float duration = 2f;
            textObj.transform.localScale = Vector3.one * 0.5f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;

                // 빠르게 커졌다가 서서히 원래 크기
                float scale = timer < 0.3f
                    ? Mathf.Lerp(0.5f, 1.2f, timer / 0.3f)
                    : Mathf.Lerp(1.2f, 1f, (timer - 0.3f) / 0.5f);
                textObj.transform.localScale = Vector3.one * Mathf.Max(scale, 1f);

                // 페이드 아웃
                if (timer > 1f)
                {
                    float alpha = 1f - (timer - 1f) / 1f;
                    tmp.color = new Color(1f, 0.9f, 0.3f, alpha);
                }

                yield return null;
            }

            Destroy(canvasObj);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("LevelUpPopup");
                obj.AddComponent<LevelUpPopup>();
            }
        }
    }
}
