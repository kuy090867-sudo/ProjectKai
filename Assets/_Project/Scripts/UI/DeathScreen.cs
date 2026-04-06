using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace ProjectKai.UI
{
    /// <summary>
    /// 사망 화면.
    /// "라만차의 기사는 쓰러져도 다시 일어난다."
    /// </summary>
    public class DeathScreen : MonoBehaviour
    {
        public static DeathScreen Instance { get; private set; }

        private GameObject _panel;
        private bool _isActive;
        public bool IsActive => _isActive;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Show()
        {
            if (_isActive) return;
            _isActive = true;
            Time.timeScale = 0f;
            CreateUI();
        }

        private void CreateUI()
        {
            // Canvas
            var canvasObj = new GameObject("DeathCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 어두운 배경
            _panel = new GameObject("DarkBG");
            _panel.transform.SetParent(canvasObj.transform, false);
            var bgRect = _panel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = _panel.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.9f);

            // 돈키호테 문구
            var quoteObj = new GameObject("Quote");
            quoteObj.transform.SetParent(_panel.transform, false);
            var quoteTmp = quoteObj.AddComponent<TextMeshProUGUI>();
            quoteTmp.text = "라만차의 기사는\n쓰러져도 다시 일어난다.";
            quoteTmp.fontSize = 36;
            quoteTmp.color = new Color(0.9f, 0.8f, 0.5f);
            quoteTmp.alignment = TextAlignmentOptions.Center;
            quoteTmp.fontStyle = FontStyles.Italic;
            var quoteRect = quoteObj.GetComponent<RectTransform>();
            quoteRect.anchorMin = new Vector2(0.1f, 0.5f);
            quoteRect.anchorMax = new Vector2(0.9f, 0.7f);
            quoteRect.offsetMin = Vector2.zero;
            quoteRect.offsetMax = Vector2.zero;

            // 재시도 버튼
            CreateButton(_panel.transform, "재시도", new Vector2(0.3f, 0.25f), new Vector2(0.5f, 0.35f), () =>
            {
                Time.timeScale = 1f;
                _isActive = false;
                Destroy(canvasObj);
                string currentScene = SceneManager.GetActiveScene().name;
                if (SceneTransition.Instance != null)
                    SceneTransition.Instance.LoadScene(currentScene);
                else
                    SceneManager.LoadScene(currentScene);
            });

            // 거점으로 버튼
            CreateButton(_panel.transform, "거점으로", new Vector2(0.55f, 0.25f), new Vector2(0.75f, 0.35f), () =>
            {
                Time.timeScale = 1f;
                _isActive = false;
                Destroy(canvasObj);
                string target = SceneExists("Hub") ? "Hub" : SceneManager.GetActiveScene().name;
                if (SceneTransition.Instance != null)
                    SceneTransition.Instance.LoadScene(target);
                else
                    SceneManager.LoadScene(target);
            });
        }

        private void CreateButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            var btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            var txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
        }

        private bool SceneExists(string name)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (path.Contains(name)) return true;
            }
            return false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("DeathScreen");
                obj.AddComponent<DeathScreen>();
            }
        }
    }
}
