using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ProjectKai.UI
{
    /// <summary>
    /// 보스 인트로 연출. 보스 이름 + 부제 + 카메라 줌.
    /// "풍차인가, 거인인가"
    /// </summary>
    public class BossIntro : MonoBehaviour
    {
        public static void Show(string bossName, string subtitle = null)
        {
            var obj = new GameObject("BossIntro");
            var bi = obj.AddComponent<BossIntro>();
            bi.StartCoroutine(bi.IntroSequence(bossName, subtitle));
        }

        private IEnumerator IntroSequence(string bossName, string subtitle)
        {
            // 캔버스 생성
            var canvasObj = new GameObject("BossIntroCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 상하단 시네마틱 바
            var topBar = CreateBar(canvasObj.transform, new Vector2(0f, 0.9f), Vector2.one);
            var bottomBar = CreateBar(canvasObj.transform, Vector2.zero, new Vector2(1f, 0.1f));

            // 보스 이름
            var nameObj = new GameObject("BossName");
            nameObj.transform.SetParent(canvasObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.1f, 0.42f);
            nameRect.anchorMax = new Vector2(0.9f, 0.58f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            var nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = bossName;
            nameTmp.fontSize = 52;
            nameTmp.color = new Color(1f, 0.85f, 0.4f, 0f);
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.fontStyle = FontStyles.Bold;

            // 부제
            TextMeshProUGUI subTmp = null;
            if (!string.IsNullOrEmpty(subtitle))
            {
                var subObj = new GameObject("Subtitle");
                subObj.transform.SetParent(canvasObj.transform, false);
                var subRect = subObj.AddComponent<RectTransform>();
                subRect.anchorMin = new Vector2(0.15f, 0.35f);
                subRect.anchorMax = new Vector2(0.85f, 0.42f);
                subRect.offsetMin = Vector2.zero;
                subRect.offsetMax = Vector2.zero;
                subTmp = subObj.AddComponent<TextMeshProUGUI>();
                subTmp.text = subtitle;
                subTmp.fontSize = 24;
                subTmp.color = new Color(0.7f, 0.6f, 0.45f, 0f);
                subTmp.alignment = TextAlignmentOptions.Center;
                subTmp.fontStyle = FontStyles.Italic;
            }

            // 구분선
            var lineObj = new GameObject("Line");
            lineObj.transform.SetParent(canvasObj.transform, false);
            var lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.3f, 0.415f);
            lineRect.anchorMax = new Vector2(0.7f, 0.42f);
            lineRect.offsetMin = Vector2.zero;
            lineRect.offsetMax = Vector2.zero;
            var lineImg = lineObj.AddComponent<Image>();
            lineImg.color = new Color(1f, 0.85f, 0.4f, 0f);

            // 카메라 줌
            Core.GameFeel.Instance?.CameraZoom(4.2f, 2.5f);

            // 페이드 인 (0.8초)
            float timer = 0f;
            while (timer < 0.8f)
            {
                timer += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(timer / 0.8f);
                nameTmp.color = new Color(1f, 0.85f, 0.4f, alpha);
                if (subTmp != null)
                    subTmp.color = new Color(0.7f, 0.6f, 0.45f, alpha * 0.8f);
                lineImg.color = new Color(1f, 0.85f, 0.4f, alpha * 0.6f);
                yield return null;
            }

            // 유지 (1.5초)
            yield return new WaitForSecondsRealtime(1.5f);

            // 페이드 아웃 (0.6초)
            timer = 0f;
            while (timer < 0.6f)
            {
                timer += Time.unscaledDeltaTime;
                float alpha = 1f - Mathf.Clamp01(timer / 0.6f);
                nameTmp.color = new Color(1f, 0.85f, 0.4f, alpha);
                if (subTmp != null)
                    subTmp.color = new Color(0.7f, 0.6f, 0.45f, alpha * 0.8f);
                lineImg.color = new Color(1f, 0.85f, 0.4f, alpha * 0.6f);
                yield return null;
            }

            Destroy(canvasObj);
            Destroy(gameObject);
        }

        private GameObject CreateBar(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var bar = new GameObject("CinemaBar");
            bar.transform.SetParent(parent, false);
            var rect = bar.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            bar.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
            return bar;
        }
    }
}
