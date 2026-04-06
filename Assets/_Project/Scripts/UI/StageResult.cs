using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectKai.Core;

namespace ProjectKai.UI
{
    /// <summary>
    /// 스테이지 클리어 결과 화면.
    /// 시간, 처치 수, 획득 경험치 표시.
    /// </summary>
    public class StageResult : MonoBehaviour
    {
        public static StageResult Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Show(string stageName, float time, int kills, string nextScene)
        {
            Time.timeScale = 0f;

            var canvasObj = new GameObject("ResultCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvasObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);

            // STAGE CLEAR
            AddText(bg.transform, "STAGE CLEAR", 42,
                new Color(1f, 0.9f, 0.4f), new Vector2(0.2f, 0.7f), new Vector2(0.8f, 0.85f));

            // 스테이지 이름
            AddText(bg.transform, stageName, 28,
                Color.white, new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.7f));

            // 결과
            string resultText = $"시간: {time:F1}초\n처치: {kills}마리";
            AddText(bg.transform, resultText, 24,
                new Color(0.8f, 0.8f, 0.8f), new Vector2(0.25f, 0.4f), new Vector2(0.75f, 0.58f));

            // 계속 버튼
            var btnObj = new GameObject("NextBtn");
            btnObj.transform.SetParent(bg.transform, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.3f, 0.15f);
            btnRect.anchorMax = new Vector2(0.7f, 0.28f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            btnObj.AddComponent<Image>().color = new Color(0.3f, 0.25f, 0.35f, 0.9f);
            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;
                Destroy(canvasObj);
                if (!string.IsNullOrEmpty(nextScene))
                    SceneTransition.Instance?.LoadScene(nextScene);
            });

            AddText(btnObj.transform, "계속", 26, Color.white,
                Vector2.zero, Vector2.one);
        }

        private void AddText(Transform parent, string text, float size, Color color, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject("T");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin;
            rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("StageResult");
                obj.AddComponent<StageResult>();
            }
        }
    }
}
