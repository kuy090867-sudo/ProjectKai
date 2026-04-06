using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ProjectKai.Data;
using ProjectKai.Core;

namespace ProjectKai.UI
{
    /// <summary>
    /// 거점 UI. 리나 대화 + 스테이지 선택 (챕터 해금 기반) + 스탯/강화.
    /// </summary>
    public class HubUI : MonoBehaviour
    {
        private static readonly string[] _rinaDialogues = new string[]
        {
            "넌 왜 맨날 손해 보는 일만 하는 거야?",
            "보수 없는 의뢰는 안 받는다고 했잖아... 라고 말하고 싶지만.",
            "다음 의뢰는 좀 쉬운 거 없나?",
            "그 기사 이야기 책, 아직도 들고 다니는 거야?",
            "슬럼 아이들이 네 이름을 알더라. '라만차의 기사'라고.",
            "돈키호테 선생님, 오늘은 어느 풍차와 싸울 거야?",
            "스승님이 보시면 뭐라고 하실까.",
            "에테르 가격이 또 올랐어. 이 세상은 정말...",
            "가끔은 네가 부러워. 믿는 게 있으니까.",
            "조심해. 다음번엔 내가 못 구할 수도 있어.",
            "그림자 기사단... 우리가 건드릴 수준이 아닌데.",
            "내 부모님도 이 일과 관련이 있을지도 몰라.",
            "풍차든 거인이든, 네 옆에는 내가 있으니까.",
            "오늘도 살아서 돌아와. 그게 최우선이야."
        };

        private void Start()
        {
            Time.timeScale = 1f;
            SaveSystem.Save();
            CreateUI();
            ShowRinaDialogue();
        }

        private void ShowRinaDialogue()
        {
            int idx = Random.Range(0, _rinaDialogues.Length);
            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "리나", text = _rinaDialogues[idx] },
                new DialogueLine { speakerName = "카이", text = "...알았어." }
            };
            StartCoroutine(DelayedDialogue(lines));
        }

        private System.Collections.IEnumerator DelayedDialogue(DialogueLine[] lines)
        {
            yield return new WaitForSeconds(1f);
            DialogueSystem.Instance?.StartDialogue(lines);
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("HubCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 배경
            CreatePanel(canvasObj.transform, Vector2.zero, Vector2.one, new Color(0.06f, 0.05f, 0.1f));

            // 타이틀
            AddText(canvasObj.transform, "거점 — 카이의 사무실", 36,
                new Color(0.9f, 0.8f, 0.5f), new Vector2(0.15f, 0.87f), new Vector2(0.85f, 0.97f));

            // 부제
            AddText(canvasObj.transform, "\"세상이 미쳤다고 해도, 옳다고 믿는 것을 위해 싸우는 자가 기사다.\"", 14,
                new Color(0.5f, 0.45f, 0.4f), new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.87f));

            // 스테이지 선택
            AddText(canvasObj.transform, "— 의뢰 게시판 —", 22,
                new Color(0.7f, 0.65f, 0.5f), new Vector2(0.05f, 0.73f), new Vector2(0.45f, 0.8f));

            CreateStageBtn(canvasObj.transform, "1장: 풍차를 향해", "Stage1_1",
                new Vector2(0.05f, 0.58f), new Vector2(0.45f, 0.72f), true);

            bool ch2 = GameState.Instance != null && GameState.Instance.Chapter2Unlocked;
            CreateStageBtn(canvasObj.transform, ch2 ? "2장: 거울의 기사" : "2장: ???  [미해금]", "Stage2_1",
                new Vector2(0.05f, 0.42f), new Vector2(0.45f, 0.56f), ch2);

            bool ch3 = GameState.Instance != null && GameState.Instance.Chapter3Unlocked;
            CreateStageBtn(canvasObj.transform, ch3 ? "3장: 라만차의 기사" : "3장: ???  [미해금]", "Stage3_1",
                new Vector2(0.05f, 0.26f), new Vector2(0.45f, 0.4f), ch3);

            // 스탯 패널
            AddText(canvasObj.transform, "— 카이의 상태 —", 22,
                new Color(0.7f, 0.65f, 0.5f), new Vector2(0.55f, 0.73f), new Vector2(0.95f, 0.8f));

            var prog = ProgressionSystem.Instance;
            if (prog != null)
            {
                AddText(canvasObj.transform, $"Lv.{prog.Level}", 28,
                    new Color(1f, 0.9f, 0.5f), new Vector2(0.55f, 0.65f), new Vector2(0.75f, 0.73f));
                AddText(canvasObj.transform, $"Gold: {prog.Gold}", 20,
                    new Color(1f, 0.85f, 0.3f), new Vector2(0.75f, 0.65f), new Vector2(0.95f, 0.73f));

                AddText(canvasObj.transform, $"STR: {prog.STR}    DEX: {prog.DEX}    INT: {prog.INT}", 20,
                    Color.white, new Vector2(0.55f, 0.57f), new Vector2(0.95f, 0.65f));

                if (prog.StatPoints > 0)
                {
                    AddText(canvasObj.transform, $"미사용 스탯 포인트: {prog.StatPoints}", 18,
                        new Color(0.3f, 1f, 0.3f), new Vector2(0.55f, 0.5f), new Vector2(0.95f, 0.57f));

                    CreateStatBtn(canvasObj.transform, "STR +1", "STR",
                        new Vector2(0.55f, 0.42f), new Vector2(0.68f, 0.5f));
                    CreateStatBtn(canvasObj.transform, "DEX +1", "DEX",
                        new Vector2(0.7f, 0.42f), new Vector2(0.83f, 0.5f));
                    CreateStatBtn(canvasObj.transform, "INT +1", "INT",
                        new Vector2(0.85f, 0.42f), new Vector2(0.95f, 0.5f));
                }
            }

            // 하단 버튼
            CreateStageBtn(canvasObj.transform, "메인 메뉴", "MainMenu",
                new Vector2(0.7f, 0.05f), new Vector2(0.95f, 0.15f), true);

            // 테스트 스테이지
            CreateStageBtn(canvasObj.transform, "테스트", "TestStage",
                new Vector2(0.5f, 0.05f), new Vector2(0.68f, 0.15f), true);
        }

        private void CreatePanel(Transform parent, Vector2 aMin, Vector2 aMax, Color color)
        {
            var obj = new GameObject("Panel");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            obj.AddComponent<Image>().color = color;
        }

        private void AddText(Transform parent, string text, float size, Color color, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.fontSize = size; tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateStageBtn(Transform parent, string label, string scene, Vector2 aMin, Vector2 aMax, bool enabled)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

            var img = obj.AddComponent<Image>();
            img.color = enabled ? new Color(0.18f, 0.16f, 0.25f, 0.9f) : new Color(0.12f, 0.12f, 0.15f, 0.6f);

            var btn = obj.AddComponent<Button>();
            btn.interactable = enabled;
            if (enabled)
            {
                btn.onClick.AddListener(() => SceneTransition.Instance?.LoadScene(scene));
                var colors = btn.colors;
                colors.highlightedColor = new Color(0.35f, 0.3f, 0.5f);
                colors.pressedColor = new Color(0.5f, 0.4f, 0.65f);
                btn.colors = colors;
            }

            var t = new GameObject("T");
            t.transform.SetParent(obj.transform, false);
            var tr = t.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
            var tmp = t.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 22;
            tmp.color = enabled ? new Color(0.9f, 0.85f, 0.7f) : new Color(0.4f, 0.4f, 0.4f);
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateStatBtn(Transform parent, string label, string stat, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            obj.AddComponent<Image>().color = new Color(0.2f, 0.3f, 0.2f, 0.8f);
            var btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (ProgressionSystem.Instance?.AllocateStat(stat) == true)
                {
                    AudioManager.Instance?.PlaySFX("jump", 0.5f);
                    // UI 새로고침
                    foreach (Transform child in parent) Destroy(child.gameObject);
                    CreateUI();
                }
            });
            var t = new GameObject("T");
            t.transform.SetParent(obj.transform, false);
            var tr = t.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
            var tmp = t.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = 16; tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
