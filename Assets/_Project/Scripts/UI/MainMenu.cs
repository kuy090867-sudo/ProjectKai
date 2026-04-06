using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ProjectKai.Core;

namespace ProjectKai.UI
{
    /// <summary>
    /// 메인 메뉴. "PROJECT KAI — 라만차의 기사"
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 1f;
            Core.SaveSystem.Load();
            CreateUI();
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("MainMenuCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvasObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f);

            // 타이틀
            var titleObj = AddText(canvasObj.transform, "PROJECT KAI", 64,
                new Color(0.95f, 0.85f, 0.5f), new Vector2(0.15f, 0.62f), new Vector2(0.85f, 0.85f));
            titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // 부제
            AddText(canvasObj.transform, "— 라만차의 기사 —", 30,
                new Color(0.7f, 0.6f, 0.4f), new Vector2(0.25f, 0.54f), new Vector2(0.75f, 0.64f));

            // 게임 클리어 표시
            if (GameState.Instance != null && GameState.Instance.GameCleared)
            {
                AddText(canvasObj.transform, "★ GAME CLEARED ★", 20,
                    new Color(1f, 0.8f, 0.2f), new Vector2(0.3f, 0.48f), new Vector2(0.7f, 0.54f));
            }

            // 버튼들
            CreateBtn(canvasObj.transform, "새 게임", new Vector2(0.3f, 0.34f), new Vector2(0.7f, 0.44f), () =>
            {
                SaveSystem.DeleteSave();
                // 런타임 상태 초기화
                if (GameState.Instance != null)
                {
                    GameState.Instance.CurrentChapter = 1;
                    GameState.Instance.Chapter2Unlocked = false;
                    GameState.Instance.Chapter3Unlocked = false;
                    GameState.Instance.GameCleared = false;
                }
                if (ProgressionSystem.Instance != null)
                    ProgressionSystem.Instance.LoadData(1, 0, 0, 0, 5, 5, 5);
                WeaponUpgrade.LoadLevels(1, 1);
                SceneTransition.Instance?.LoadScene("Stage1_1");
            });

            bool hasSave = SaveSystem.HasSave();
            CreateBtn(canvasObj.transform, hasSave ? "이어하기" : "이어하기 (세이브 없음)",
                new Vector2(0.3f, 0.22f), new Vector2(0.7f, 0.32f), () =>
            {
                SceneTransition.Instance?.LoadScene("Hub");
            }, hasSave);

            CreateBtn(canvasObj.transform, "설정", new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.2f), () =>
            {
                SettingsMenu.Show();
            });

            CreateBtn(canvasObj.transform, "종료", new Vector2(0.3f, -0.02f), new Vector2(0.7f, 0.08f), () =>
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });

            // 하단 인용구
            AddText(canvasObj.transform,
                "\"세상이 미쳤다고 해도, 옳다고 믿는 것을 위해 싸우는 자가 기사다.\"",
                16, new Color(0.4f, 0.4f, 0.4f),
                new Vector2(0.1f, 0.01f), new Vector2(0.9f, 0.06f));

            // 버전
            AddText(canvasObj.transform, "v0.6  |  Unity 6  |  github.com/kuy090867-sudo/ProjectKai",
                12, new Color(0.3f, 0.3f, 0.3f),
                new Vector2(0.2f, 0.06f), new Vector2(0.8f, 0.1f));
        }

        private GameObject AddText(Transform parent, string text, float size, Color color, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return obj;
        }

        private void CreateBtn(Transform parent, string text, Vector2 aMin, Vector2 aMax,
            UnityEngine.Events.UnityAction action, bool interactable = true)
        {
            var obj = new GameObject(text + "Btn");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.color = interactable ? new Color(0.15f, 0.14f, 0.22f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.5f);

            var btn = obj.AddComponent<Button>();
            btn.interactable = interactable;
            btn.onClick.AddListener(action);
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.28f, 0.45f);
            colors.pressedColor = new Color(0.5f, 0.4f, 0.6f);
            btn.colors = colors;

            var t = new GameObject("T");
            t.transform.SetParent(obj.transform, false);
            var tr = t.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
            var tmp = t.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = 26;
            tmp.color = interactable ? new Color(0.9f, 0.85f, 0.7f) : new Color(0.4f, 0.38f, 0.35f);
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
