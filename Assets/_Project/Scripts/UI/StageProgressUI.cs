using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectKai.Core;

namespace ProjectKai.UI
{
    /// <summary>
    /// Hub 씬 우상단에 표시되는 스테이지 진행도 패널.
    /// 3개 챕터 x 3개 스테이지 아이콘 표시.
    /// GameState 해금 상태 기반으로 클리어/잠김/미클리어 구분.
    /// </summary>
    public class StageProgressUI : MonoBehaviour
    {
        private static StageProgressUI _instance;
        private GameObject _canvasObj;

        // 챕터별 이름
        private static readonly string[] ChapterNames = { "1장", "2장", "3장" };

        // 스테이지 아이콘 문자
        private const string ICON_CLEARED  = "\u25CF"; // ● 클리어
        private const string ICON_OPEN     = "\u25CB"; // ○ 미클리어
        private const string ICON_LOCKED   = "\uD83D\uDD12"; // 잠김

        /// <summary>
        /// 스테이지 진행도 패널을 표시한다.
        /// 이미 표시 중이면 갱신만 수행.
        /// </summary>
        public static void Show()
        {
            if (_instance == null)
            {
                var obj = new GameObject("StageProgressUI");
                _instance = obj.AddComponent<StageProgressUI>();
                Debug.Log("[StageProgressUI] 생성");
            }

            _instance.Rebuild();
        }

        /// <summary>
        /// 패널을 숨기고 파괴한다.
        /// </summary>
        public static void Hide()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        private void OnDestroy()
        {
            if (_canvasObj != null)
                Destroy(_canvasObj);

            if (_instance == this)
                _instance = null;
        }

        // ═══════════════════════════════════════
        //  UI 구축
        // ═══════════════════════════════════════

        private void Rebuild()
        {
            if (_canvasObj != null)
                Destroy(_canvasObj);

            _canvasObj = new GameObject("StageProgressCanvas");
            _canvasObj.transform.SetParent(transform);

            var canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45; // HubCanvas(50) 아래, HUD(10) 위

            var scaler = _canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _canvasObj.AddComponent<GraphicRaycaster>();

            CreatePanel();
        }

        private void CreatePanel()
        {
            // ─── 패널 배경 (우상단 고정) ───
            var panelObj = new GameObject("ProgressPanel");
            panelObj.transform.SetParent(_canvasObj.transform, false);

            var panelRect = panelObj.AddComponent<RectTransform>();
            // 우상단 앵커
            panelRect.anchorMin = new Vector2(0.72f, 0.74f);
            panelRect.anchorMax = new Vector2(0.98f, 0.98f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.85f);

            // ─── 타이틀 ───
            AddText(panelObj.transform, "의뢰 진행",
                18, new Color(0.9f, 0.8f, 0.5f),
                new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.97f),
                TextAlignmentOptions.Center, FontStyles.Bold);

            // ─── 구분선 ───
            var divider = new GameObject("Divider");
            divider.transform.SetParent(panelObj.transform, false);
            var divRect = divider.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.08f, 0.78f);
            divRect.anchorMax = new Vector2(0.92f, 0.80f);
            divRect.offsetMin = Vector2.zero;
            divRect.offsetMax = Vector2.zero;
            divider.AddComponent<Image>().color = new Color(0.5f, 0.4f, 0.3f, 0.5f);

            // ─── 챕터별 행 ───
            var gs = GameState.Instance;
            int currentChapter = gs != null ? gs.CurrentChapter : 1;
            bool ch2Unlocked = gs != null && gs.Chapter2Unlocked;
            bool ch3Unlocked = gs != null && gs.Chapter3Unlocked;
            bool gameCleared = gs != null && gs.GameCleared;

            // 각 챕터의 3스테이지 상태 결정
            // StageStatus: 0=잠김, 1=미클리어(열림), 2=클리어
            for (int ch = 1; ch <= 3; ch++)
            {
                int[] stages = GetChapterStageStatus(ch, ch2Unlocked, ch3Unlocked, gameCleared, currentChapter);
                bool isCurrent = (ch == currentChapter);

                float rowTop = 0.75f - (ch - 1) * 0.25f;
                float rowBot = rowTop - 0.22f;

                CreateChapterRow(panelObj.transform, ch, stages, isCurrent, rowTop, rowBot);
            }
        }

        /// <summary>
        /// 챕터의 3개 스테이지 상태를 반환.
        /// 0=잠김, 1=미클리어(열림), 2=클리어
        /// </summary>
        private int[] GetChapterStageStatus(int chapter, bool ch2Unlocked, bool ch3Unlocked, bool gameCleared, int currentChapter)
        {
            int[] status = new int[3]; // 기본 0(잠김)

            switch (chapter)
            {
                case 1:
                    // 1장은 항상 열림
                    if (ch2Unlocked)
                    {
                        // 1장 전체 클리어 (1-3 클리어 = 2장 해금)
                        status[0] = 2; status[1] = 2; status[2] = 2;
                    }
                    else
                    {
                        // 1장 진행 중: currentChapter=1이면 열린 상태
                        // 현재 챕터가 1이면 모든 스테이지 열림 (미클리어)
                        status[0] = 1; status[1] = 1; status[2] = 1;
                    }
                    break;

                case 2:
                    if (!ch2Unlocked)
                    {
                        // 2장 잠김
                        status[0] = 0; status[1] = 0; status[2] = 0;
                    }
                    else if (ch3Unlocked)
                    {
                        // 2장 전체 클리어 (2-3 클리어 = 3장 해금)
                        status[0] = 2; status[1] = 2; status[2] = 2;
                    }
                    else
                    {
                        // 2장 열림, 진행 중
                        status[0] = 1; status[1] = 1; status[2] = 1;
                    }
                    break;

                case 3:
                    if (!ch3Unlocked)
                    {
                        // 3장 잠김
                        status[0] = 0; status[1] = 0; status[2] = 0;
                    }
                    else if (gameCleared)
                    {
                        // 3장 전체 클리어
                        status[0] = 2; status[1] = 2; status[2] = 2;
                    }
                    else
                    {
                        // 3장 열림, 진행 중
                        status[0] = 1; status[1] = 1; status[2] = 1;
                    }
                    break;
            }

            return status;
        }

        private void CreateChapterRow(Transform parent, int chapter, int[] stages, bool isCurrent, float top, float bottom)
        {
            // 챕터 이름 (좌측)
            Color nameColor = isCurrent
                ? new Color(1f, 0.92f, 0.3f) // 현재 챕터: 노란색 강조
                : new Color(0.65f, 0.6f, 0.55f); // 비활성: 회색

            FontStyles nameStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;

            AddText(parent, ChapterNames[chapter - 1],
                16, nameColor,
                new Vector2(0.05f, bottom), new Vector2(0.35f, top),
                TextAlignmentOptions.MidlineLeft, nameStyle);

            // 스테이지 아이콘 3개 (우측 배치)
            for (int s = 0; s < 3; s++)
            {
                float iconLeft = 0.38f + s * 0.20f;
                float iconRight = iconLeft + 0.18f;

                string icon;
                Color iconColor;

                switch (stages[s])
                {
                    case 2: // 클리어
                        icon = ICON_CLEARED;
                        iconColor = new Color(0.3f, 0.9f, 0.4f); // 초록
                        break;
                    case 1: // 미클리어 (열림)
                        icon = ICON_OPEN;
                        iconColor = isCurrent
                            ? new Color(1f, 0.92f, 0.3f) // 현재 챕터면 노란색
                            : new Color(0.7f, 0.7f, 0.7f);
                        break;
                    default: // 잠김
                        icon = ICON_LOCKED;
                        iconColor = new Color(0.4f, 0.35f, 0.35f);
                        break;
                }

                AddText(parent, icon,
                    18, iconColor,
                    new Vector2(iconLeft, bottom), new Vector2(iconRight, top),
                    TextAlignmentOptions.Center, FontStyles.Normal);
            }
        }

        // ═══════════════════════════════════════
        //  유틸리티
        // ═══════════════════════════════════════

        private void AddText(Transform parent, string text, float size, Color color,
            Vector2 anchorMin, Vector2 anchorMax,
            TextAlignmentOptions alignment, FontStyles style)
        {
            var obj = new GameObject("Text");
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
            tmp.alignment = alignment;
            tmp.fontStyle = style;
        }
    }
}
