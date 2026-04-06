using UnityEngine;
using System.Collections;

namespace ProjectKai.Core
{
    /// <summary>
    /// 게임필 효과 관리: 카메라쉐이크, 히트스톱, 슬로우모션, 킬 연출.
    /// Dead Cells/Hollow Knight 참고.
    /// </summary>
    public class GameFeel : MonoBehaviour
    {
        public static GameFeel Instance { get; private set; }

        private UnityEngine.Camera _cam;
        private float _shakeTimer;
        private float _shakeIntensity;
        private float _shakeDuration;
        private Coroutine _hitStopCoroutine;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.unscaledDeltaTime;
                if (_cam == null) _cam = UnityEngine.Camera.main;
                if (_cam != null)
                {
                    float decay = _shakeTimer / _shakeDuration;
                    Vector2 offset = Random.insideUnitCircle * _shakeIntensity * decay;
                    _cam.transform.position += (Vector3)offset;
                }
            }
        }

        /// <summary>
        /// 카메라 쉐이크. 강도와 지속시간 지정.
        /// - 일반 타격: (0.08, 0.1)
        /// - 강타격: (0.12, 0.15)
        /// - 킬: (0.2, 0.25)
        /// </summary>
        public void CameraShake(float intensity = 0.1f, float duration = 0.15f)
        {
            if (intensity > _shakeIntensity)
            {
                _shakeIntensity = intensity;
                _shakeTimer = duration;
                _shakeDuration = duration;
            }
        }

        /// <summary>
        /// 히트스톱. 짧은 프레임 정지로 타격감 강화.
        /// - 일반: 0.03초
        /// - 콤보 3타: 0.08초
        /// - 킬: 0.15초
        /// </summary>
        public void HitStop(float duration = 0.05f)
        {
            if (_hitStopCoroutine != null)
                StopCoroutine(_hitStopCoroutine);
            _hitStopCoroutine = StartCoroutine(HitStopCoroutine(duration));
        }

        private IEnumerator HitStopCoroutine(float duration)
        {
            Time.timeScale = 0.02f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _hitStopCoroutine = null;
        }

        /// <summary>
        /// 킬 슬로우모션. 적 사망 시 극적 연출.
        /// - duration: 슬로우 지속 시간 (실시간)
        /// - timeScale: 슬로우 배율 (0.2 = 5배 느림)
        /// </summary>
        public void KillSlowMotion(float duration = 0.4f, float timeScale = 0.2f)
        {
            StartCoroutine(KillSlowMotionCoroutine(duration, timeScale));
        }

        private IEnumerator KillSlowMotionCoroutine(float duration, float targetScale)
        {
            // HitStop이 끝난 후 실행
            yield return new WaitForSecondsRealtime(0.02f);

            Time.timeScale = targetScale;
            Time.fixedDeltaTime = 0.02f * targetScale;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        /// <summary>
        /// 킬 플래시. 적 사망 시 화면 순간 백색 플래시.
        /// </summary>
        public void KillFlash(float intensity = 0.3f)
        {
            StartCoroutine(KillFlashCoroutine(intensity));
        }

        private IEnumerator KillFlashCoroutine(float intensity)
        {
            // Canvas로 화면 전체 백색 오버레이
            var flashObj = new GameObject("KillFlash");
            var canvas = flashObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 99;

            var imgObj = new GameObject("FlashImage");
            imgObj.transform.SetParent(flashObj.transform, false);
            var rect = imgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = imgObj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, intensity);
            img.raycastTarget = false;

            float elapsed = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                img.color = new Color(1f, 1f, 1f, intensity * (1f - t));
                yield return null;
            }

            Destroy(flashObj);
        }

        /// <summary>
        /// 카메라 줌 효과 (콤보 3타, 보스 킬 시)
        /// </summary>
        public void CameraZoom(float targetSize, float duration = 0.3f)
        {
            StartCoroutine(CameraZoomCoroutine(targetSize, duration));
        }

        private IEnumerator CameraZoomCoroutine(float targetSize, float duration)
        {
            if (_cam == null) _cam = UnityEngine.Camera.main;
            if (_cam == null) yield break;

            float originalSize = _cam.orthographicSize;
            float elapsed = 0f;

            // 줌 인
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration * 0.5f);
                _cam.orthographicSize = Mathf.Lerp(originalSize, targetSize, t);
                yield return null;
            }

            // 줌 아웃 (원래 크기로)
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration * 0.5f);
                _cam.orthographicSize = Mathf.Lerp(targetSize, originalSize, t);
                yield return null;
            }

            _cam.orthographicSize = originalSize;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("GameFeel");
                obj.AddComponent<GameFeel>();
            }
        }
    }
}
