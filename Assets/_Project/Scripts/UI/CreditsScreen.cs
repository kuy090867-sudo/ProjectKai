using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace ProjectKai.UI
{
    /// <summary>
    /// 크레딧 화면. 엔딩 후 스크롤.
    /// </summary>
    public class CreditsScreen : MonoBehaviour
    {
        public static void Show()
        {
            var obj = new GameObject("Credits");
            obj.AddComponent<CreditsScreen>().StartCoroutine(obj.GetComponent<CreditsScreen>().RunCredits());
        }

        private IEnumerator RunCredits()
        {
            Time.timeScale = 1f;

            var canvasObj = new GameObject("CreditsCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 배경
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvasObj.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = Color.black;

            // 크레딧 텍스트
            var textObj = new GameObject("CreditText");
            textObj.transform.SetParent(canvasObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, -1f);
            textRect.anchorMax = new Vector2(0.9f, 0f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 28;
            tmp.color = new Color(0.9f, 0.85f, 0.7f);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = @"PROJECT KAI
— 라만차의 기사 —


스토리 & 기획
돈키호테 모티프
현대+중세 마법 융합 세계관


프로그래밍
State Machine 기반 전투 시스템
Unity 6 + C#


아트
DungeonTilesetII by 0x72
픽셀아트 스프라이트


사운드
합성 사운드 엔진


특별히 감사합니다
세르반테스 — 돈키호테의 원작자
모든 인디 게임 개발자들


""세상이 미쳤다고 해도,
옳다고 믿는 것을 위해 싸우는 자가 기사다.""


그림자 기사단은... 시작에 불과했다.


github.com/kuy090867-sudo/ProjectKai";

            // 스크롤
            float scrollSpeed = 50f;
            float elapsed = 0f;
            float maxTime = 20f;

            while (elapsed < maxTime)
            {
                elapsed += Time.deltaTime;
                textRect.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

                // 아무 키 입력 시 스킵
                if (elapsed > 2f && UnityEngine.InputSystem.Keyboard.current?.anyKey.wasPressedThisFrame == true)
                    break;

                yield return null;
            }

            Destroy(canvasObj);
            SceneManager.LoadScene("MainMenu");
            Destroy(gameObject);
        }
    }
}
