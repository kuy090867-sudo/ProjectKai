using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ProjectKai.UI
{
    /// <summary>
    /// 씬 전환 페이드 인/아웃.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        private Image _fadeImage;
        private Canvas _canvas;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeUI();
        }

        private void CreateFadeUI()
        {
            var canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 999;
            canvasObj.AddComponent<CanvasScaler>();

            var imgObj = new GameObject("FadeImage");
            imgObj.transform.SetParent(canvasObj.transform, false);
            _fadeImage = imgObj.AddComponent<Image>();
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;
            var rect = imgObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void LoadScene(string sceneName, float fadeDuration = 0.5f)
        {
            StartCoroutine(FadeAndLoad(sceneName, fadeDuration));
        }

        private IEnumerator FadeAndLoad(string sceneName, float duration)
        {
            // 페이드 아웃 (검은 화면으로)
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _fadeImage.color = new Color(0, 0, 0, t / duration);
                yield return null;
            }
            _fadeImage.color = Color.black;

            // 씬 로드
            SceneManager.LoadScene(sceneName);
            yield return null;

            // 페이드 인 (밝아짐)
            t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _fadeImage.color = new Color(0, 0, 0, 1f - (t / duration));
                yield return null;
            }
            _fadeImage.color = new Color(0, 0, 0, 0);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("SceneTransition");
                obj.AddComponent<SceneTransition>();
            }
        }
    }
}
