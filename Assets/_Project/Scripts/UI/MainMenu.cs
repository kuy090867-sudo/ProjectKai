using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 메인 메뉴. 신서울 야경 배경 + 타이틀.
    /// "PROJECT KAI — 라만차의 기사"
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 1f;
            CreateUI();
        }

        private void CreateUI()
        {
            // Canvas
            var canvasObj = new GameObject("MainMenuCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경 (어두운 파란색 — 신서울 야경)
            var bg = CreatePanel(canvasObj.transform, "BG", Vector2.zero, Vector2.one, new Color(0.05f, 0.08f, 0.15f, 1f));

            // 타이틀
            var titleObj = CreateText(canvasObj.transform, "Title",
                "PROJECT KAI", 60, new Color(0.9f, 0.8f, 0.5f),
                new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.85f));
            titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // 부제
            CreateText(canvasObj.transform, "Subtitle",
                "— 라만차의 기사 —", 28, new Color(0.7f, 0.6f, 0.4f),
                new Vector2(0.3f, 0.52f), new Vector2(0.7f, 0.62f));

            // 버튼들
            CreateMenuButton(canvasObj.transform, "새 게임", new Vector2(0.35f, 0.32f), new Vector2(0.65f, 0.42f), () =>
            {
                SceneManager.LoadScene("Stage1_1");
            });

            CreateMenuButton(canvasObj.transform, "이어하기", new Vector2(0.35f, 0.2f), new Vector2(0.65f, 0.3f), () =>
            {
                SceneManager.LoadScene("Hub");
            });

            CreateMenuButton(canvasObj.transform, "종료", new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f), () =>
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });

            // 하단 인용구
            CreateText(canvasObj.transform, "Quote",
                "\"세상이 미쳤다고 해도, 옳다고 믿는 것을 위해 싸우는 자가 기사다.\"",
                16, new Color(0.5f, 0.5f, 0.5f),
                new Vector2(0.15f, 0.01f), new Vector2(0.85f, 0.06f));
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }

        private GameObject CreateText(Transform parent, string name, string text, float size, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return obj;
        }

        private void CreateMenuButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            var btnObj = new GameObject(text + "Btn");
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.18f, 0.25f, 0.9f);

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            // 호버 색상
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.35f, 0.5f);
            colors.pressedColor = new Color(0.6f, 0.5f, 0.7f);
            btn.colors = colors;

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28;
            tmp.color = new Color(0.9f, 0.85f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
