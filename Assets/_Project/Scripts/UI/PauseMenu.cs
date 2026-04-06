using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 일시정지 메뉴. ESC로 토글.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        private GameObject _panel;
        private bool _isPaused;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_isPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;
            Time.timeScale = 0f;
            CreateUI();
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (_panel != null) Destroy(_panel.transform.root.gameObject);
            _panel = null;
        }

        private void CreateUI()
        {
            var canvasObj = new GameObject("PauseCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            _panel = new GameObject("BG");
            _panel.transform.SetParent(canvasObj.transform, false);
            var bgRect = _panel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // 타이틀
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_panel.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.65f);
            titleRect.anchorMax = new Vector2(0.7f, 0.8f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "일시정지";
            titleTmp.fontSize = 40;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Center;

            CreateBtn(_panel.transform, "계속", new Vector2(0.35f, 0.45f), new Vector2(0.65f, 0.55f), Resume);
            CreateBtn(_panel.transform, "메인 메뉴", new Vector2(0.35f, 0.3f), new Vector2(0.65f, 0.4f), () =>
            {
                Resume();
                SceneManager.LoadScene("MainMenu");
            });
        }

        private void CreateBtn(Transform parent, string text, Vector2 aMin, Vector2 aMax, UnityEngine.Events.UnityAction action)
        {
            var obj = new GameObject(text);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = aMin;
            rect.anchorMax = aMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            obj.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            obj.AddComponent<Button>().onClick.AddListener(action);

            var t = new GameObject("T");
            t.transform.SetParent(obj.transform, false);
            var tr = t.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            var tmp = t.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("PauseMenu");
                obj.AddComponent<PauseMenu>();
            }
        }
    }
}
