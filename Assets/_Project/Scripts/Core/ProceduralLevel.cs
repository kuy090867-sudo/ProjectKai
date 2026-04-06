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

            // 배경 벽 (뒤쪽)
            for (int x = -halfWidth; x <= halfWidth; x++)
            {
                for (int y = _groundY + 1; y < _groundY + _levelHeight; y++)
                {
                    if (Random.value < 0.05f)
                    {
                        // 5% 확률로 장식 벽 타일
                        wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
                    }
                }
            }

            // 기존 플레이스홀더 Ground 숨기기
            var oldGround = GameObject.Find("Environment/Ground");
            if (oldGround != null)
            {
                var oldRenderers = oldGround.GetComponentsInChildren<SpriteRenderer>();
                foreach (var r in oldRenderers) r.enabled = false;
            }

            Debug.Log($"[ProceduralLevel] 타일맵 생성 완료: {_levelWidth}x{_levelHeight}");
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
