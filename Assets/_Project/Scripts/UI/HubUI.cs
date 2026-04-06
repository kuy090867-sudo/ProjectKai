using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ProjectKai.Data;
using ProjectKai.Core;

namespace ProjectKai.UI
{
    /// <summary>
    /// 거점 UI. 리나 대화 + 스테이지 선택 + 무기 강화.
    /// Hades식 매번 다른 대화.
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
            "조심해. 다음번엔 내가 못 구할 수도 있어."
        };

        private void Start()
        {
            Time.timeScale = 1f;
            CreateUI();
            ShowRinaDialogue();
        }

        private void ShowRinaDialogue()
        {
            int idx = Random.Range(0, _rinaDialogues.Length);
            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "리나", text = _rinaDialogues[idx] },
                new DialogueLine { speakerName = "카이", text = "..." }
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
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvasObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.12f);

            // 타이틀
            CreateText(canvasObj.transform, "거점 — 카이의 사무실", 36,
                new Color(0.9f, 0.8f, 0.5f), new Vector2(0.2f, 0.85f), new Vector2(0.8f, 0.95f));

            // 스테이지 버튼들
            CreateStageBtn(canvasObj.transform, "1장: 풍차를 향해", "Stage1_1",
                new Vector2(0.1f, 0.55f), new Vector2(0.45f, 0.7f));
            CreateStageBtn(canvasObj.transform, "2장: 거울의 기사", "Stage2_1",
                new Vector2(0.1f, 0.35f), new Vector2(0.45f, 0.5f));
            CreateStageBtn(canvasObj.transform, "3장: 라만차의 기사", "Stage3_1",
                new Vector2(0.1f, 0.15f), new Vector2(0.45f, 0.3f));

            // 스탯 표시
            var prog = ProgressionSystem.Instance;
            string statText = prog != null
                ? $"Lv.{prog.Level}  STR:{prog.STR}  DEX:{prog.DEX}  INT:{prog.INT}  Gold:{prog.Gold}"
                : "Lv.1";
            CreateText(canvasObj.transform, statText, 22,
                Color.white, new Vector2(0.55f, 0.6f), new Vector2(0.95f, 0.7f));

            // 메인 메뉴 버튼
            CreateStageBtn(canvasObj.transform, "메인 메뉴", "MainMenu",
                new Vector2(0.6f, 0.05f), new Vector2(0.9f, 0.15f));
        }

        private void CreateText(Transform parent, string text, float size, Color color, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject("Text");
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

        private void CreateStageBtn(Transform parent, string label, string scene, Vector2 aMin, Vector2 aMax)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin;
            rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            obj.AddComponent<Image>().color = new Color(0.2f, 0.18f, 0.25f, 0.9f);
            var btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(() => SceneManager.LoadScene(scene));
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.4f, 0.35f, 0.5f);
            btn.colors = colors;

            var t = new GameObject("T");
            t.transform.SetParent(obj.transform, false);
            var tr = t.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            var tmp = t.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.color = new Color(0.9f, 0.85f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}
