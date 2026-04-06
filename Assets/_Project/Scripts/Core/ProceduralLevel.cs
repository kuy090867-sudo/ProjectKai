using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectKai.Core
{
    /// <summary>
    /// 프로시저럴 타일맵 레벨 생성.
    /// DungeonTilesetII 타일을 사용하여 바닥/벽/천장 자동 배치.
    /// AD 역할 담당.
    /// </summary>
    public class ProceduralLevel : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private int _levelWidth = 60;
        [SerializeField] private int _levelHeight = 15;
        [SerializeField] private int _groundY = -3;

        [Header("Tile Sprites (from Resources)")]
        [SerializeField] private string _floorSpritePath = "Sprites/Tiles/floor_1";
        [SerializeField] private string _wallSpritePath = "Sprites/Tiles/wall_mid";

        [Header("Visual")]
        [SerializeField] private Color _groundColor = new Color(0.4f, 0.3f, 0.2f);
        [SerializeField] private Color _wallColor = new Color(0.3f, 0.25f, 0.2f);

        [Header("Level Variety")]
        [SerializeField] private bool _addPlatforms = true;
        [SerializeField] private int _platformCount = 3;

        private void Start()
        {
            GenerateLevel();
        }

        private void GenerateLevel()
        {
            // Grid + Tilemap 생성
            var gridObj = new GameObject("LevelGrid");
            gridObj.AddComponent<Grid>();

            // 바닥 Tilemap
            var groundObj = new GameObject("GroundTilemap");
            groundObj.transform.SetParent(gridObj.transform);
            var groundTilemap = groundObj.AddComponent<Tilemap>();
            var groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingOrder = -1;
            groundObj.AddComponent<TilemapCollider2D>();

            // 벽 Tilemap (배경)
            var wallObj = new GameObject("WallTilemap");
            wallObj.transform.SetParent(gridObj.transform);
            var wallTilemap = wallObj.AddComponent<Tilemap>();
            var wallRenderer = wallObj.AddComponent<TilemapRenderer>();
            wallRenderer.sortingOrder = -2;

            // 타일 생성
            var floorTile = CreateTile(_floorSpritePath, _groundColor);
            var wallTile = CreateTile(_wallSpritePath, _wallColor);

            if (floorTile == null || wallTile == null)
            {
                Debug.LogWarning("[ProceduralLevel] 타일 스프라이트 로드 실패, 기본 타일 사용");
                floorTile = CreateDefaultTile(_groundColor);
                wallTile = CreateDefaultTile(_wallColor);
            }

            int halfWidth = _levelWidth / 2;

            // 바닥 타일 배치
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                groundTilemap.SetTile(new Vector3Int(x, _groundY, 0), floorTile);
                groundTilemap.SetTile(new Vector3Int(x, _groundY - 1, 0), floorTile);
            }

            // 벽 타일 배치 (좌우 끝 + 천장)
            for (int y = _groundY; y < _groundY + _levelHeight; y++)
            {
                wallTilemap.SetTile(new Vector3Int(-halfWidth - 1, y, 0), wallTile);
                wallTilemap.SetTile(new Vector3Int(halfWidth + 1, y, 0), wallTile);
            }

            // 천장
            for (int x = -halfWidth - 1; x <= halfWidth + 1; x++)
            {
                wallTilemap.SetTile(new Vector3Int(x, _groundY + _levelHeight, 0), wallTile);
            }

            // 배경 벽 (뒤쪽) — 랜덤 장식
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = _groundY + 1; y < _groundY + _levelHeight; y++)
                {
                    if (Random.value < 0.04f)
                    {
                        wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                }
            }

            // === 플랫폼 배치 (공중 발판) ===
            if (_addPlatforms)
            {
                int sectionWidth = _levelWidth / (_platformCount + 1);
                for (int i = 1; i <= _platformCount; i++)
                {
                    int px = -halfWidth + sectionWidth * i + Random.Range(-2, 3);
                    int py = _groundY + Random.Range(2, 5);
                    int pWidth = Random.Range(3, 6);

                    for (int dx = 0; dx < pWidth; dx++)
                    {
                        groundTilemap.SetTile(new Vector3Int(px + dx, py, 0), floorTile);
                    }
                }
            }

            // === 계단식 지형 (우측 끝 근처) ===
            int stairStart = halfWidth - 10;
            for (int step = 0; step < 3; step++)
            {
                int sx = stairStart + step * 3;
                int sy = _groundY + step + 1;
                for (int dx = 0; dx < 3; dx++)
                {
                    groundTilemap.SetTile(new Vector3Int(sx + dx, sy, 0), floorTile);
                }
            }

            // 기존 플레이스홀더 Ground 숨기기
            var oldGround = GameObject.Find("Environment/Ground");
            if (oldGround != null)
            {
                var oldRenderers = oldGround.GetComponentsInChildren<SpriteRenderer>();
                foreach (var r in oldRenderers) r.enabled = false;
            }

            // === 패럴랙스 배경 자동 생성 ===
            CreateParallaxBackground();

            Debug.Log($"[ProceduralLevel] 타일맵 생성 완료: {_levelWidth}x{_levelHeight}, 플랫폼 {_platformCount}개");
        }

        private void CreateParallaxBackground()
        {
            // Layer 1: 먼 배경 (어두운 실루엣)
            CreateBgLayer("BG_Far", -10f, 0.1f, new Color(0.08f, 0.06f, 0.12f), 80, 20);
            // Layer 2: 중간 배경 (건물 실루엣)
            CreateBgLayer("BG_Mid", -5f, 0.3f, new Color(0.12f, 0.1f, 0.18f), 60, 15);
            // Layer 3: 가까운 배경
            CreateBgLayer("BG_Near", -2f, 0.6f, new Color(0.18f, 0.15f, 0.22f), 40, 8);
        }

        private void CreateBgLayer(string name, float z, float parallaxFactor, Color color, int width, int height)
        {
            var tex = new Texture2D(width, height);
            var pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float a = (float)y / height; // 아래쪽이 더 어둡게
                    Color c = color * (0.6f + a * 0.4f);

                    // 건물/기둥 실루엣 (랜덤 수직선)
                    if (Random.value < 0.02f)
                        c *= 0.5f;

                    c.a = 1f;
                    pixels[y * width + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();

            var sprite = Sprite.Create(tex,
                new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 4f); // 큰 PPU 비율로 넓게

            var obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
            obj.transform.position = new Vector3(0, _groundY + height * 0.1f, z);
            obj.transform.localScale = Vector3.one * 2f;

            var parallax = obj.AddComponent<ParallaxLayer>();
            // parallaxFactor는 SerializeField이므로 reflection으로 설정
            var field = typeof(ParallaxLayer).GetField("_parallaxFactor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(parallax, parallaxFactor);
        }

        private Tile CreateTile(string spritePath, Color color)
        {
            var tex = Resources.Load<Texture2D>(spritePath);
            if (tex == null) return null;

            tex.filterMode = FilterMode.Point;
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 16f);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = color;
            return tile;
        }

        private Tile CreateDefaultTile(Color color)
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < 256; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            var sprite = Sprite.Create(tex,
                new Rect(0, 0, 16, 16),
                new Vector2(0.5f, 0.5f), 16f);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = color;
            return tile;
        }
    }
}
