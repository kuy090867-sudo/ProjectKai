using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 패럴랙스 배경 시스템. 3개 레이어가 카메라 이동에 따라 다른 속도로 스크롤.
    /// Stage/Hub 씬에서 RuntimeInitializeOnLoadMethod로 자동 생성.
    /// 스프라이트 리소스 없이 코드로 그라데이션 텍스처 생성.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float _parallaxFactor = 0.5f;

        private Transform _cam;
        private Vector3 _startPos;
        private float _startCamX;

        private void Start()
        {
            var mainCam = UnityEngine.Camera.main;
            _cam = mainCam != null ? mainCam.transform : null;
            _startPos = transform.position;
            if (_cam != null)
                _startCamX = _cam.position.x;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;

            float deltaX = _cam.position.x - _startCamX;
            transform.position = new Vector3(
                _startPos.x + deltaX * _parallaxFactor,
                _startPos.y,
                _startPos.z);
        }

        /// <summary>parallaxFactor 외부 설정용</summary>
        public void SetFactor(float factor)
        {
            _parallaxFactor = factor;
        }

        // ============================================================
        //  자동 생성 시스템
        // ============================================================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!sceneName.StartsWith("Stage") && sceneName != "Hub") return;

            // 이미 생성된 경우 스킵
            if (GameObject.Find("ParallaxBackground") != null) return;

            var root = new GameObject("ParallaxBackground");

            // 씬별 색상 테마 결정
            ColorTheme theme = GetThemeForScene(sceneName);

            // 3개 레이어 생성
            CreateLayer(root.transform, "ParallaxLayer_Back", 10f, 0.1f, -30,
                theme.layer1Top, theme.layer1Bottom, 100f, 30f);

            CreateLayer(root.transform, "ParallaxLayer_Mid", 5f, 0.3f, -20,
                theme.layer2Top, theme.layer2Bottom, 100f, 25f);

            CreateLayer(root.transform, "ParallaxLayer_Front", 2f, 0.7f, -10,
                theme.layer3Top, theme.layer3Bottom, 100f, 20f);
        }

        // ============================================================
        //  레이어 생성
        // ============================================================

        private static void CreateLayer(Transform parent, string name,
            float zDepth, float factor, int sortingOrder,
            Color topColor, Color bottomColor,
            float width, float height)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = new Vector3(0f, 0f, zDepth);

            // SpriteRenderer 설정
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateGradientSprite(topColor, bottomColor, (int)width * 4, (int)height * 4);
            sr.sortingOrder = sortingOrder;
            sr.drawMode = SpriteDrawMode.Simple;

            // 카메라 기준으로 크기 맞추기 (가로 100유닛)
            if (sr.sprite != null)
            {
                float spriteWidth = sr.sprite.bounds.size.x;
                float spriteHeight = sr.sprite.bounds.size.y;
                float scaleX = width / spriteWidth;
                float scaleY = height / spriteHeight;
                obj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            // ParallaxLayer 컴포넌트 추가
            var layer = obj.AddComponent<ParallaxLayer>();
            layer.SetFactor(factor);
        }

        // ============================================================
        //  그라데이션 텍스처 생성
        // ============================================================

        private static Sprite CreateGradientSprite(Color topColor, Color bottomColor,
            int texWidth, int texHeight)
        {
            var tex = new Texture2D(texWidth, texHeight);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[texWidth * texHeight];
            for (int y = 0; y < texHeight; y++)
            {
                float t = (float)y / (texHeight - 1);
                Color rowColor = Color.Lerp(bottomColor, topColor, t);

                // 약간의 노이즈로 자연스러운 느낌 추가
                for (int x = 0; x < texWidth; x++)
                {
                    float noise = ((float)((x * 73 + y * 137) % 256) / 256f - 0.5f) * 0.03f;
                    Color pixel = new Color(
                        Mathf.Clamp01(rowColor.r + noise),
                        Mathf.Clamp01(rowColor.g + noise),
                        Mathf.Clamp01(rowColor.b + noise),
                        rowColor.a);
                    pixels[y * texWidth + x] = pixel;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, texWidth, texHeight),
                new Vector2(0.5f, 0.5f),
                4f); // PPU=4 -> 적당한 해상도
        }

        // ============================================================
        //  씬별 색상 테마
        // ============================================================

        private struct ColorTheme
        {
            public Color layer1Top;   // 가장 뒤 (어두운 실루엣)
            public Color layer1Bottom;
            public Color layer2Top;   // 중간 (건물 실루엣)
            public Color layer2Bottom;
            public Color layer3Top;   // 앞 (가까운 장식)
            public Color layer3Bottom;
        }

        private static ColorTheme GetThemeForScene(string sceneName)
        {
            // Stage1: 던전 갈색 톤
            if (sceneName.StartsWith("Stage1"))
            {
                return new ColorTheme
                {
                    layer1Top    = new Color(0.12f, 0.08f, 0.05f, 1f),  // 짙은 갈색
                    layer1Bottom = new Color(0.05f, 0.03f, 0.02f, 1f),  // 거의 검정
                    layer2Top    = new Color(0.20f, 0.13f, 0.08f, 1f),  // 어두운 갈색
                    layer2Bottom = new Color(0.10f, 0.06f, 0.04f, 1f),  // 짙은 갈색
                    layer3Top    = new Color(0.30f, 0.22f, 0.15f, 0.7f),// 밝은 갈색 (반투명)
                    layer3Bottom = new Color(0.18f, 0.12f, 0.08f, 0.7f) // 중간 갈색 (반투명)
                };
            }

            // Stage2: 에테르 파란색 톤
            if (sceneName.StartsWith("Stage2"))
            {
                return new ColorTheme
                {
                    layer1Top    = new Color(0.05f, 0.08f, 0.18f, 1f),  // 짙은 남색
                    layer1Bottom = new Color(0.02f, 0.03f, 0.08f, 1f),  // 거의 검정
                    layer2Top    = new Color(0.10f, 0.15f, 0.30f, 1f),  // 어두운 파랑
                    layer2Bottom = new Color(0.05f, 0.08f, 0.18f, 1f),  // 짙은 남색
                    layer3Top    = new Color(0.15f, 0.22f, 0.40f, 0.7f),// 에테르 블루 (반투명)
                    layer3Bottom = new Color(0.08f, 0.12f, 0.28f, 0.7f) // 어두운 블루 (반투명)
                };
            }

            // Stage3: 붉은색 톤
            if (sceneName.StartsWith("Stage3"))
            {
                return new ColorTheme
                {
                    layer1Top    = new Color(0.18f, 0.05f, 0.05f, 1f),  // 짙은 붉은색
                    layer1Bottom = new Color(0.08f, 0.02f, 0.02f, 1f),  // 거의 검정
                    layer2Top    = new Color(0.30f, 0.08f, 0.08f, 1f),  // 어두운 붉은색
                    layer2Bottom = new Color(0.15f, 0.04f, 0.04f, 1f),  // 짙은 붉은색
                    layer3Top    = new Color(0.40f, 0.15f, 0.12f, 0.7f),// 밝은 붉은색 (반투명)
                    layer3Bottom = new Color(0.25f, 0.08f, 0.06f, 0.7f) // 중간 붉은색 (반투명)
                };
            }

            // Hub: 따뜻한 주황색 톤
            return new ColorTheme
            {
                layer1Top    = new Color(0.15f, 0.10f, 0.05f, 1f),  // 따뜻한 어두운 톤
                layer1Bottom = new Color(0.06f, 0.04f, 0.02f, 1f),  // 거의 검정
                layer2Top    = new Color(0.25f, 0.18f, 0.08f, 1f),  // 어두운 주황
                layer2Bottom = new Color(0.12f, 0.08f, 0.04f, 1f),  // 짙은 주황
                layer3Top    = new Color(0.35f, 0.25f, 0.12f, 0.7f),// 따뜻한 주황 (반투명)
                layer3Bottom = new Color(0.22f, 0.15f, 0.07f, 0.7f) // 중간 주황 (반투명)
            };
        }
    }
}
