using UnityEngine;
using UnityEngine.Tilemaps;
using ProjectKai.Data;
using ProjectKai.Combat;
using System.Collections.Generic;

namespace ProjectKai.Core
{
    /// <summary>
    /// LevelData의 ASCII 맵을 Tilemap으로 변환하는 프로시저럴 레벨 생성기.
    ///
    /// 참고 패턴:
    ///   - AhilKhan/2DPlatformer LevelDesigner.cs: 9분할 이웃 기반 에지 결정
    ///   - Colthor/LudumDareBase TileMapScript.cs: 4-bit 비트마스크 auto-tiling
    ///
    /// ParseMap: '#','=','-','.',장식(W|B|D|K|C),함정(S),엔티티(e,o,a,m,G,P,X)
    /// GetWallTile: 4-bit bitmask(위1,우2,아래4,좌8) → 16가지 벽 변형
    /// SpawnEntities: GameSetup/EnemySpawnPoint 패턴으로 적 생성 + StageManager 등록
    /// CreateParallaxBackground: LevelData.GetBackgroundTheme → Resources 텍스처 로드
    /// SetupCameraBounds: CameraFollow.SetBounds 호출
    /// </summary>
    [DefaultExecutionOrder(-50)] // GameSetup(-100) 후, 적(0) 전에 실행
    public class ProceduralLevel : MonoBehaviour
    {
        // --- 타일 캐시 ---
        private Dictionary<string, Tile> _tileCache = new Dictionary<string, Tile>();
        private Tile _invisibleTile;

        // --- 파싱 결과 ---
        private int _mapWidth;
        private int _mapHeight;
        private readonly List<(Vector2 pos, char type)> _entitySpawns = new List<(Vector2, char)>();

        // --- 런타임 참조 ---
        private Grid _grid;
        private Tilemap _groundTilemap;
        private Tilemap _wallTilemap;
        private Tilemap _decorTilemap;

        // 맵 데이터 원본 (GetWallTile에서 사용)
        private string[] _mapData;
        private string _stageName;

        // ============================================================
        //  자동 생성
        // ============================================================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!sceneName.StartsWith("Stage")) return;

            // 이미 존재하면 스킵
            if (Object.FindFirstObjectByType<ProceduralLevel>() != null) return;

            var obj = new GameObject("ProceduralLevel");
            obj.AddComponent<ProceduralLevel>();
        }

        // ============================================================
        //  Start (메인 파이프라인)
        // ============================================================

        private void Start()
        {
            // 1. 기존 Environment 삭제
            var oldEnv = GameObject.Find("Environment");
            if (oldEnv != null) DestroyImmediate(oldEnv);

            // 기존 ProceduralLevel이 만든 그리드 삭제
            var oldGrid = GameObject.Find("LevelGrid");
            if (oldGrid != null && oldGrid != gameObject)
                DestroyImmediate(oldGrid);

            // 기존 물리 콜라이더 삭제 (이전 버전 잔재)
            DestroyIfExists("GroundCollider");
            DestroyIfExists("LeftWall");
            DestroyIfExists("RightWall");

            // 2. 씬 이름에서 스테이지 번호 추출 (StageManager와 동일 방식)
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!sceneName.StartsWith("Stage"))
            {
                Debug.Log("[ProceduralLevel] Stage 씬이 아님, 스킵");
                return;
            }

            string stripped = sceneName.Replace("Stage", "").Replace("_Boss", "");
            _stageName = stripped.Replace("_", "-");

            // 3. LevelData에서 맵 데이터 로드
            _mapData = LevelData.GetMap(_stageName);
            if (_mapData == null || _mapData.Length == 0)
            {
                Debug.LogWarning($"[ProceduralLevel] 맵 데이터 없음: {_stageName}, 스킵");
                return;
            }

            // 4. 타일 캐시 초기화
            InitTileCache();

            // 5. ParseMap → Tilemap 생성
            ParseMap(_mapData);

            // 6. SpawnEntities → 적/플레이어/출구 배치
            SpawnEntities();

            // 7. CreateParallaxBackground → 배경 레이어
            CreateParallaxBackground();

            // 8. SetupCameraBounds → 카메라 바운드
            SetupCameraBounds();

            // 콜라이더 즉시 동기화 - 플레이어가 첫 프레임에 떨어지지 않도록
            Physics2D.SyncTransforms();

            Debug.Log($"[ProceduralLevel] 맵 생성 완료: {_stageName} ({_mapWidth}x{_mapHeight}), " +
                      $"엔티티 {_entitySpawns.Count}개");
        }

        private static void DestroyIfExists(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null) DestroyImmediate(obj);
        }

        // ============================================================
        //  타일 캐시 초기화
        // ============================================================

        private void InitTileCache()
        {
            // 바닥 타일 (floor_1~8 랜덤 선택용)
            for (int i = 1; i <= 8; i++)
                CacheTile($"floor_{i}", $"Sprites/Tiles/floor_{i}");

            // 벽 타일 (auto-tiling용)
            string[] wallTiles = new string[]
            {
                "wall_mid", "wall_left", "wall_right",
                "wall_top_left", "wall_top_mid", "wall_top_right",
                "wall_edge_left", "wall_edge_right",
                "wall_edge_bottom_left", "wall_edge_bottom_right",
            };
            foreach (var name in wallTiles)
                CacheTile(name, $"Sprites/Tiles/{name}");

            // 장식 타일
            string[] decorTiles = new string[]
            {
                "column", "column_wall", "skull", "crate",
                "wall_banner_blue", "wall_banner_green", "wall_banner_red", "wall_banner_yellow",
                "doors_frame_left", "doors_frame_right", "doors_frame_top",
                "doors_leaf_closed", "doors_leaf_open",
                "floor_spikes_anim_f0",
                "edge_down", "floor_stairs", "floor_ladder",
                "hole", "wall_hole_1", "wall_hole_2",
                "wall_goo", "wall_goo_base",
            };
            foreach (var name in decorTiles)
                CacheTile(name, $"Sprites/Tiles/{name}");

            // 투명 콜라이더 타일
            _invisibleTile = CreateInvisibleTile();
        }

        private void CacheTile(string key, string spritePath)
        {
            var tex = Resources.Load<Texture2D>(spritePath);
            if (tex == null)
            {
                // 스프라이트 없으면 플레이스홀더
                _tileCache[key] = CreatePlaceholderTile(key);
                return;
            }

            tex.filterMode = FilterMode.Point;
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 16f);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            _tileCache[key] = tile;
        }

        private Tile GetCachedTile(string key)
        {
            if (_tileCache.TryGetValue(key, out var tile))
                return tile;
            return null;
        }

        /// <summary>바닥 타일 floor_1~8 중 랜덤 선택</summary>
        private Tile GetRandomFloorTile()
        {
            int idx = Random.Range(1, 9);
            return GetCachedTile($"floor_{idx}") ?? GetCachedTile("floor_1");
        }

        // ============================================================
        //  ParseMap: ASCII → Tilemap
        //  참고: Colthor/LudumDareBase TileMapScript.cs CharacterToTileIndex 패턴
        // ============================================================

        private void ParseMap(string[] mapData)
        {
            _mapHeight = mapData.Length;
            _mapWidth = 0;
            foreach (var row in mapData)
                if (row.Length > _mapWidth) _mapWidth = row.Length;

            // Grid 생성
            var gridObj = new GameObject("LevelGrid");
            _grid = gridObj.AddComponent<Grid>();

            // --- Ground Tilemap (콜라이더 O) ---
            var groundObj = new GameObject("GroundTilemap");
            groundObj.transform.SetParent(gridObj.transform);
            _groundTilemap = groundObj.AddComponent<Tilemap>();
            var groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingOrder = -1;
            var groundCollider = groundObj.AddComponent<TilemapCollider2D>();
            groundCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            groundObj.AddComponent<CompositeCollider2D>();
            groundObj.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            // Ground 레이어 설정
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
                groundObj.layer = groundLayer;

            // --- Wall Tilemap (배경 벽, 콜라이더 없음) ---
            var wallObj = new GameObject("WallTilemap");
            wallObj.transform.SetParent(gridObj.transform);
            _wallTilemap = wallObj.AddComponent<Tilemap>();
            var wallRenderer = wallObj.AddComponent<TilemapRenderer>();
            wallRenderer.sortingOrder = -2;

            // --- Decoration Tilemap (장식, 콜라이더 없음) ---
            var decorObj = new GameObject("DecorationTilemap");
            decorObj.transform.SetParent(gridObj.transform);
            _decorTilemap = decorObj.AddComponent<Tilemap>();
            var decorRenderer = decorObj.AddComponent<TilemapRenderer>();
            decorRenderer.sortingOrder = 0;

            // --- 타일 배치 ---
            // mapData[0]이 맨 위행 → y좌표 변환: tileY = height - row - 1
            for (int row = 0; row < _mapHeight; row++)
            {
                for (int col = 0; col < mapData[row].Length; col++)
                {
                    char c = mapData[row][col];
                    int tileX = col;
                    int tileY = _mapHeight - row - 1;
                    var pos = new Vector3Int(tileX, tileY, 0);

                    switch (c)
                    {
                        // --- 벽 (solid, auto-tiling) ---
                        case '#':
                            var wallTile = GetWallTile(col, row, mapData);
                            _groundTilemap.SetTile(pos, wallTile);
                            break;

                        // --- 바닥 (solid, 랜덤 floor_1~8) ---
                        case '=':
                            _groundTilemap.SetTile(pos, GetRandomFloorTile());
                            break;

                        // --- 플랫폼 (위에서만 착지) ---
                        case '-':
                            CreatePlatformTile(tileX, tileY);
                            break;

                        // --- 장식 타일 ---
                        case 'W': // 벽 상단 장식
                            _decorTilemap.SetTile(pos, GetCachedTile("wall_top_mid"));
                            break;

                        case '|': // 기둥
                            _decorTilemap.SetTile(pos, GetCachedTile("column"));
                            // 기둥은 충돌도 있어야 함
                            _groundTilemap.SetTile(pos, _invisibleTile);
                            break;

                        case 'B': // 배너 (챕터별 색상)
                        {
                            string bannerColor = LevelData.GetBannerColor(_stageName);
                            _decorTilemap.SetTile(pos, GetCachedTile($"wall_banner_{bannerColor}"));
                            break;
                        }

                        case 'D': // 문 장식
                            _decorTilemap.SetTile(pos, GetCachedTile("doors_leaf_closed"));
                            break;

                        case 'K': // 해골 장식
                            _decorTilemap.SetTile(pos, GetCachedTile("skull"));
                            break;

                        case 'C': // 상자
                            _decorTilemap.SetTile(pos, GetCachedTile("crate"));
                            break;

                        // --- 가시 함정: 바닥 타일 + FloorSpikes ---
                        case 'S':
                            _groundTilemap.SetTile(pos, GetRandomFloorTile());
                            _decorTilemap.SetTile(pos, GetCachedTile("floor_spikes_anim_f0"));
                            CreateFloorSpikes(tileX, tileY);
                            break;

                        // --- 엔티티 (위치 기록만, SpawnEntities에서 처리) ---
                        case 'e': // 고블린
                        case 'o': // 오크
                        case 'a': // 궁수
                        case 'm': // 마법사
                        case 'G': // 보스
                        case 'P': // 플레이어
                        case 'X': // 출구
                            _entitySpawns.Add((new Vector2(tileX + 0.5f, tileY + 1f), c));
                            break;

                        case '.': // 빈 공간
                        default:
                            break;
                    }
                }
            }
        }

        // ============================================================
        //  GetWallTile: 4-bit 비트마스크 auto-tiling
        //  참고: Colthor/LudumDareBase TileMapScript.cs DoWallAutotiling
        //  위=1, 우=2, 아래=4, 좌=8
        // ============================================================

        private Tile GetWallTile(int col, int row, string[] mapData)
        {
            int mask = 0;

            // 위(row-1) 검사
            if (IsWall(col, row - 1, mapData)) mask |= 1;
            // 우(col+1) 검사
            if (IsWall(col + 1, row, mapData)) mask |= 2;
            // 아래(row+1) 검사
            if (IsWall(col, row + 1, mapData)) mask |= 4;
            // 좌(col-1) 검사
            if (IsWall(col - 1, row, mapData)) mask |= 8;

            // 비트마스크 → 타일 선택
            // AhilKhan/2DPlatformer LevelDesigner.cs: TilePosition 패턴 참고
            bool hasTop = (mask & 1) != 0;
            bool hasRight = (mask & 2) != 0;
            bool hasBottom = (mask & 4) != 0;
            bool hasLeft = (mask & 8) != 0;

            // 독립 벽 (사방 없음)
            if (mask == 0) return GetCachedTile("wall_mid");

            // 상단 노출 (위 없음)
            if (!hasTop && !hasLeft && !hasRight)
                return GetCachedTile("wall_top_mid");
            if (!hasTop && !hasLeft && hasRight)
                return GetCachedTile("wall_top_left");
            if (!hasTop && hasLeft && !hasRight)
                return GetCachedTile("wall_top_right");
            if (!hasTop && hasLeft && hasRight)
                return GetCachedTile("wall_top_mid");

            // 좌우 에지 (위는 있음)
            if (hasTop && !hasLeft && hasRight)
                return GetCachedTile("wall_edge_left");
            if (hasTop && hasLeft && !hasRight)
                return GetCachedTile("wall_edge_right");

            // 하단 에지
            if (!hasBottom && hasTop && !hasLeft)
                return GetCachedTile("wall_edge_bottom_left");
            if (!hasBottom && hasTop && !hasRight)
                return GetCachedTile("wall_edge_bottom_right");

            // 기본 (사방 둘러싸인 벽)
            return GetCachedTile("wall_mid");
        }

        /// <summary>해당 좌표가 벽('#') 또는 바닥('=')인지 확인 (범위 밖은 false)</summary>
        private bool IsWall(int col, int row, string[] mapData)
        {
            if (row < 0 || row >= mapData.Length) return false;
            if (col < 0 || col >= mapData[row].Length) return false;
            char c = mapData[row][col];
            return c == '#' || c == '=';
        }

        // ============================================================
        //  플랫폼 (PlatformEffector2D)
        // ============================================================

        private void CreatePlatformTile(int tileX, int tileY)
        {
            var platObj = new GameObject($"Platform_{tileX}_{tileY}");
            platObj.transform.SetParent(_grid.transform);
            platObj.transform.position = new Vector3(tileX + 0.5f, tileY + 0.5f, 0f);

            // 비주얼: SpriteRenderer로 바닥 타일 표시
            var sr = platObj.AddComponent<SpriteRenderer>();
            var floorTile = GetCachedTile("edge_down");
            if (floorTile != null && floorTile.sprite != null)
                sr.sprite = floorTile.sprite;
            else
            {
                var ft = GetCachedTile("floor_1");
                if (ft != null) sr.sprite = ft.sprite;
            }
            sr.sortingOrder = -1;

            // 콜라이더 + PlatformEffector2D
            var col = platObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 0.3f);
            col.offset = new Vector2(0f, 0.35f);
            col.usedByEffector = true;

            var effector = platObj.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
            effector.useColliderMask = false;

            // Ground 레이어
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
                platObj.layer = groundLayer;
        }

        // ============================================================
        //  FloorSpikes 생성
        // ============================================================

        private void CreateFloorSpikes(int tileX, int tileY)
        {
            var spikeObj = new GameObject($"Spikes_{tileX}_{tileY}");
            spikeObj.transform.SetParent(_grid.transform);
            spikeObj.transform.position = new Vector3(tileX + 0.5f, tileY + 0.5f, 0f);

            var col = spikeObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 0.5f);
            col.offset = new Vector2(0f, 0.25f);

            spikeObj.AddComponent<FloorSpikes>();
        }

        // ============================================================
        //  SpawnEntities: 적/플레이어/출구 배치
        //  GameSetup/EnemySpawnPoint 패턴 참고
        // ============================================================

        private void SpawnEntities()
        {
            foreach (var (pos, type) in _entitySpawns)
            {
                switch (type)
                {
                    case 'P': SpawnPlayer(pos); break;
                    case 'X': SpawnExit(pos); break;
                    case 'e': SpawnEnemy(pos, "Goblin", typeof(Enemy.EnemyBase)); break;
                    case 'o': SpawnEnemy(pos, "OrcWarrior", typeof(Enemy.OrcWarrior)); break;
                    case 'a': SpawnEnemy(pos, "SkeletonArcher", typeof(Enemy.SkeletonArcher)); break;
                    case 'm': SpawnEnemy(pos, "EtherMage", typeof(Enemy.EtherMage)); break;
                    case 'G': SpawnBoss(pos); break;
                }
            }
        }

        /// <summary>이미 씬에 있는 Player를 맵 위치로 텔레포트</summary>
        private void SpawnPlayer(Vector2 pos)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(pos.x, pos.y, 0f);
                Debug.Log($"[ProceduralLevel] 플레이어 → ({pos.x:F1}, {pos.y:F1})");
            }
        }

        /// <summary>출구 트리거 생성 (StageManager 연동)</summary>
        private void SpawnExit(Vector2 pos)
        {
            var exitObj = new GameObject("Exit");
            exitObj.transform.SetParent(_grid.transform);
            exitObj.transform.position = new Vector3(pos.x, pos.y, 0f);

            var col = exitObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);

            exitObj.AddComponent<ExitTrigger>();
        }

        /// <summary>
        /// 적 생성 (EnemySpawnPoint 패턴)
        /// tag="Enemy", Rigidbody2D(Kinematic), BoxCollider2D, DamageReceiver, EnemyReward, ItemDrop 추가
        /// StageManager.RegisterSpawnedEnemy() 호출
        /// </summary>
        private void SpawnEnemy(Vector2 pos, string enemyName, System.Type enemyComponent)
        {
            var enemyObj = new GameObject(enemyName);
            enemyObj.transform.position = new Vector3(pos.x, pos.y, 0f);
            enemyObj.tag = "Enemy";

            // Sprite 자식 오브젝트
            var spriteChild = new GameObject("Sprite");
            spriteChild.transform.SetParent(enemyObj.transform);
            spriteChild.transform.localPosition = Vector3.zero;
            var sr = spriteChild.AddComponent<SpriteRenderer>();
            sr.color = Color.white;

            // 필수 컴포넌트
            var rb = enemyObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = enemyObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.5f, 0.8f);

            var dr = enemyObj.AddComponent<DamageReceiver>();

            // 적 타입별 컴포넌트
            enemyObj.AddComponent(enemyComponent);
            enemyObj.AddComponent<EnemyReward>();
            enemyObj.AddComponent<ItemDrop>();

            // SpriteAnimator
            spriteChild.AddComponent<SpriteAnimator>();

            // StageManager에 등록
            if (StageManager.Instance != null)
                StageManager.Instance.RegisterSpawnedEnemy(dr);

            // 체력바
            UI.HealthBarUI.CreateHealthBar(
                enemyObj.transform, dr.MaxHealth,
                new Vector3(0f, 1f, 0f),
                new Color(0.9f, 0.2f, 0.2f),
                new Vector2(60f, 8f));
        }

        /// <summary>보스 스폰: 씬에 따라 BossGoblin 또는 KnightCommander</summary>
        private void SpawnBoss(Vector2 pos)
        {
            System.Type bossType;
            string bossName;

            // Chapter 1 → BossGoblin, Chapter 2~3 → KnightCommander
            if (_stageName.StartsWith("1"))
            {
                bossType = typeof(Enemy.BossGoblin);
                bossName = "BossGoblin";
            }
            else
            {
                bossType = typeof(Enemy.KnightCommander);
                bossName = "KnightCommander";
            }

            var bossObj = new GameObject(bossName);
            bossObj.transform.position = new Vector3(pos.x, pos.y, 0f);
            bossObj.tag = "Enemy";

            var spriteChild = new GameObject("Sprite");
            spriteChild.transform.SetParent(bossObj.transform);
            spriteChild.transform.localPosition = Vector3.zero;
            var sr = spriteChild.AddComponent<SpriteRenderer>();
            sr.color = Color.white;

            var rb = bossObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = bossObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1.5f); // 보스는 더 큰 콜라이더

            var dr = bossObj.AddComponent<DamageReceiver>();
            bossObj.AddComponent(bossType);
            bossObj.AddComponent<EnemyReward>();
            bossObj.AddComponent<ItemDrop>();
            spriteChild.AddComponent<SpriteAnimator>();

            if (StageManager.Instance != null)
                StageManager.Instance.RegisterSpawnedEnemy(dr);

            // 보스 체력바 (더 큰 크기)
            UI.HealthBarUI.CreateHealthBar(
                bossObj.transform, dr.MaxHealth,
                new Vector3(0f, 1.5f, 0f),
                new Color(1f, 0.3f, 0.1f),
                new Vector2(100f, 12f));
        }

        // ============================================================
        //  CreateParallaxBackground
        //  LevelData.GetBackgroundTheme → Resources 텍스처 로드
        //  ParallaxLayer.SetFactor() 호출
        // ============================================================

        private void CreateParallaxBackground()
        {
            // 기존 배경 오브젝트 제거 (ParallaxLayer.AutoInitialize와 충돌 방지)
            var oldBg = GameObject.Find("ParallaxBackground");
            if (oldBg != null) DestroyImmediate(oldBg);
            var oldBg2 = GameObject.Find("Background");
            if (oldBg2 != null) DestroyImmediate(oldBg2);

            string theme = LevelData.GetBackgroundTheme(_stageName);
            var bgRoot = new GameObject("ParallaxBackground");

            // Resources/Backgrounds/{theme}/ 에서 텍스처 로드 시도
            var textures = Resources.LoadAll<Texture2D>($"Backgrounds/{theme}");

            if (textures != null && textures.Length > 0)
            {
                // 실제 텍스처 기반 배경
                float[] factors = { 0.05f, 0.15f, 0.3f, 0.5f };
                int[] sortOrders = { -30, -25, -20, -15 };
                int layerCount = Mathf.Min(textures.Length, 4);

                for (int i = 0; i < layerCount; i++)
                {
                    CreateTextureLayer(bgRoot.transform, $"BgLayer_{i}",
                        textures[i], factors[i], sortOrders[i], i);
                }
            }
            else
            {
                // 텍스처 없음 → 그라데이션 폴백 (ParallaxLayer.AutoInitialize와 동일)
                CreateGradientBackground(bgRoot.transform);
            }
        }

        /// <summary>텍스처 기반 배경 레이어 생성</summary>
        private void CreateTextureLayer(Transform parent, string name,
            Texture2D tex, float factor, int sortingOrder, int index)
        {
            var layerObj = new GameObject(name);
            layerObj.transform.SetParent(parent);
            layerObj.transform.localPosition = new Vector3(0f, 0f, 10f + index);

            tex.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 16f);

            var sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            // 맵 크기에 맞게 스케일
            if (sprite != null)
            {
                float spriteW = sprite.bounds.size.x;
                float spriteH = sprite.bounds.size.y;
                float targetW = _mapWidth * 1.5f;
                float targetH = _mapHeight * 1.5f;
                layerObj.transform.localScale = new Vector3(
                    targetW / Mathf.Max(spriteW, 0.01f),
                    targetH / Mathf.Max(spriteH, 0.01f), 1f);
            }

            // ParallaxLayer 컴포넌트 추가
            var pl = layerObj.AddComponent<ParallaxLayer>();
            pl.SetFactor(factor);
        }

        /// <summary>그라데이션 폴백 배경 (텍스처 리소스 없을 때)</summary>
        private void CreateGradientBackground(Transform parent)
        {
            // 씬별 색상 결정
            Color topColor, bottomColor;
            if (_stageName.StartsWith("1"))
            {
                topColor = new Color(0.12f, 0.08f, 0.05f);
                bottomColor = new Color(0.05f, 0.03f, 0.02f);
            }
            else if (_stageName.StartsWith("2"))
            {
                topColor = new Color(0.05f, 0.08f, 0.18f);
                bottomColor = new Color(0.02f, 0.03f, 0.08f);
            }
            else
            {
                topColor = new Color(0.18f, 0.05f, 0.05f);
                bottomColor = new Color(0.08f, 0.02f, 0.02f);
            }

            float[] factors = { 0.05f, 0.15f, 0.3f };
            int[] sortOrders = { -30, -25, -20 };

            for (int i = 0; i < 3; i++)
            {
                float t = (float)i / 2f;
                Color layerTop = Color.Lerp(topColor, topColor * 1.8f, t);
                Color layerBottom = Color.Lerp(bottomColor, bottomColor * 1.5f, t);
                float alpha = i == 2 ? 0.7f : 1f;
                layerTop.a = alpha;
                layerBottom.a = alpha;

                var layerObj = new GameObject($"GradientLayer_{i}");
                layerObj.transform.SetParent(parent);
                layerObj.transform.localPosition = new Vector3(0f, 0f, 10f + i);

                var sr = layerObj.AddComponent<SpriteRenderer>();
                sr.sprite = CreateGradientSprite(layerTop, layerBottom,
                    _mapWidth * 4, _mapHeight * 4);
                sr.sortingOrder = sortOrders[i];

                // 스케일: 맵을 넉넉히 덮도록
                if (sr.sprite != null)
                {
                    float sw = sr.sprite.bounds.size.x;
                    float sh = sr.sprite.bounds.size.y;
                    float targetW = _mapWidth * 2f;
                    float targetH = _mapHeight * 2f;
                    layerObj.transform.localScale = new Vector3(
                        targetW / Mathf.Max(sw, 0.01f),
                        targetH / Mathf.Max(sh, 0.01f), 1f);
                }

                var pl = layerObj.AddComponent<ParallaxLayer>();
                pl.SetFactor(factors[i]);
            }
        }

        private static Sprite CreateGradientSprite(Color topColor, Color bottomColor,
            int texWidth, int texHeight)
        {
            texWidth = Mathf.Max(texWidth, 4);
            texHeight = Mathf.Max(texHeight, 4);
            var tex = new Texture2D(texWidth, texHeight);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[texWidth * texHeight];
            for (int y = 0; y < texHeight; y++)
            {
                float t = (float)y / (texHeight - 1);
                Color rowColor = Color.Lerp(bottomColor, topColor, t);
                for (int x = 0; x < texWidth; x++)
                {
                    float noise = ((float)((x * 73 + y * 137) % 256) / 256f - 0.5f) * 0.03f;
                    pixels[y * texWidth + x] = new Color(
                        Mathf.Clamp01(rowColor.r + noise),
                        Mathf.Clamp01(rowColor.g + noise),
                        Mathf.Clamp01(rowColor.b + noise),
                        rowColor.a);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, texWidth, texHeight),
                new Vector2(0.5f, 0.5f), 4f);
        }

        // ============================================================
        //  SetupCameraBounds
        //  CameraFollow.SetBounds(min, max) 호출
        // ============================================================

        private void SetupCameraBounds()
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            var cameraFollow = cam.GetComponent<Camera.CameraFollow>();
            if (cameraFollow == null) return;

            // 카메라 반사이즈 계산 (뷰포트 오프셋)
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = camHalfHeight * cam.aspect;

            // 맵 바운드: 타일맵 좌표계 기준
            // 최소: 좌하단 (0,0) + 카메라 반사이즈
            // 최대: 우상단 (mapWidth, mapHeight) - 카메라 반사이즈
            Vector2 boundsMin = new Vector2(camHalfWidth, camHalfHeight);
            Vector2 boundsMax = new Vector2(
                _mapWidth - camHalfWidth,
                _mapHeight - camHalfHeight);

            // 맵이 카메라보다 작으면 중앙 고정
            if (boundsMax.x < boundsMin.x)
            {
                float centerX = _mapWidth * 0.5f;
                boundsMin.x = centerX;
                boundsMax.x = centerX;
            }
            if (boundsMax.y < boundsMin.y)
            {
                float centerY = _mapHeight * 0.5f;
                boundsMin.y = centerY;
                boundsMax.y = centerY;
            }

            cameraFollow.SetBounds(boundsMin, boundsMax);
            Debug.Log($"[ProceduralLevel] 카메라 바운드: ({boundsMin}) ~ ({boundsMax})");
        }

        // ============================================================
        //  유틸리티
        // ============================================================

        private Tile CreateInvisibleTile()
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            for (int i = 0; i < 256; i++) pixels[i] = new Color(0, 0, 0, 0);
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            var sprite = Sprite.Create(tex,
                new Rect(0, 0, 16, 16),
                new Vector2(0.5f, 0.5f), 16f);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.clear;
            return tile;
        }

        private Tile CreatePlaceholderTile(string name)
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            // 타일 이름에 따라 다른 색상
            Color c = name.Contains("wall") ? new Color(0.3f, 0.25f, 0.2f)
                     : name.Contains("floor") ? new Color(0.4f, 0.3f, 0.2f)
                     : new Color(0.5f, 0.5f, 0.5f);
            for (int i = 0; i < 256; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            var sprite = Sprite.Create(tex,
                new Rect(0, 0, 16, 16),
                new Vector2(0.5f, 0.5f), 16f);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white;
            return tile;
        }
    }

    // ============================================================
    //  ExitTrigger: 출구 충돌 시 StageManager 클리어 체크
    // ============================================================

    public class ExitTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (StageManager.Instance == null) return;
            if (!StageManager.Instance.IsCleared) return;

            // 클리어 완료 시 다음 씬으로 전환
            Debug.Log("[ExitTrigger] 플레이어 출구 도달, 씬 전환");
            ProjectKai.UI.SceneTransition.Instance?.LoadScene(
                StageDialogueContent.GetNextScene(StageManager.Instance.StageName));
        }
    }
}
