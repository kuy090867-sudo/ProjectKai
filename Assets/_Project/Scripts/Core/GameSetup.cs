using UnityEngine;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    /// <summary>
    /// 게임 시작 시 자동 설정: 레이어 할당, 스프라이트 로드.
    /// </summary>
    [DefaultExecutionOrder(-100)] // 다른 스크립트보다 먼저 실행
    public class GameSetup : MonoBehaviour
    {
        private void Awake()
        {
            AutoAssignSprites();
            AutoAssignLayers();
            AutoAddEnemyRewards();
        }

        private void Start()
        {
            CreateHealthBars();
            DifficultyScaler.ScaleEnemies();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            // Stage나 Hub 씬에서 GameSetup이 없으면 자동 생성
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Stage") || sceneName == "Hub")
            {
                if (FindFirstObjectByType<GameSetup>() == null)
                {
                    var obj = new GameObject("GameSetup");
                    obj.AddComponent<GameSetup>();
                }
            }
        }

        private void AutoAddEnemyRewards()
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                if (e.GetComponent<EnemyReward>() == null)
                    e.AddComponent<EnemyReward>();
                if (e.GetComponent<ItemDrop>() == null)
                    e.AddComponent<ItemDrop>();
            }
        }

        private void CreateHealthBars()
        {
            // Player 체력바
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var pc = player.GetComponent<Player.PlayerController>();
                if (pc != null)
                {
                    HealthBarUI.CreateHealthBar(
                        player.transform, pc.MaxHealth,
                        new Vector3(0f, 1.3f, 0f),
                        new Color(0.2f, 0.9f, 0.2f),
                        new Vector2(80f, 12f));
                }
            }

            // Enemy 체력바
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                var dr = enemy.GetComponent<Combat.DamageReceiver>();
                if (dr != null)
                {
                    var hb = HealthBarUI.CreateHealthBar(
                        enemy.transform, dr.MaxHealth,
                        new Vector3(0f, 1f, 0f),
                        new Color(0.9f, 0.2f, 0.2f),
                        new Vector2(60f, 8f));

                    dr.OnHealthChanged += (current, max) => hb.UpdateHealth(current, max);
                    dr.OnDamaged += (dmg, dir) => DamagePopup.Create(
                        enemy.transform.position, dmg, Color.white);
                }
            }
        }

        private void AutoAssignSprites()
        {
            // Player 스프라이트는 SpriteAnimator가 관리 — 여기서 건드리지 않음

            // Enemy 스프라이트 — 타입별 자동 할당 + SpriteAnimator 설정
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                var sr = enemy.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
                {
                    string spritePath = GetEnemySpritePath(enemy);
                    sr.sprite = LoadPixelSprite(spritePath);
                    sr.color = Color.white;
                    if (sr.sprite == null) sr.sprite = CreatePlaceholder();
                }

                // SpriteAnimator 자동 설정
                var anim = enemy.GetComponentInChildren<SpriteAnimator>();
                if (anim == null)
                {
                    var spriteChild = enemy.GetComponentInChildren<SpriteRenderer>();
                    if (spriteChild != null)
                        anim = spriteChild.gameObject.AddComponent<SpriteAnimator>();
                }
                if (anim != null)
                {
                    string prefix = GetEnemySpritePrefix(enemy);
                    anim.ConfigureEnemy("Sprites/Enemy", prefix);
                }
            }

            // Environment — 플레이스홀더 유지 (타일은 나중에 Tilemap으로)
            var envParent = GameObject.Find("Environment");
            if (envParent != null)
            {
                foreach (var sr in envParent.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (sr.sprite == null)
                        sr.sprite = CreatePlaceholder();
                }
            }
        }

        private static string GetEnemySpritePrefix(GameObject enemy)
        {
            if (enemy.GetComponent<Enemy.BossGoblin>() != null) return "big_demon";
            if (enemy.GetComponent<Enemy.KnightCommander>() != null) return "knight_f";
            if (enemy.GetComponent<Enemy.OrcWarrior>() != null) return "orc_warrior";
            if (enemy.GetComponent<Enemy.SkeletonArcher>() != null) return "necromancer";
            if (enemy.GetComponent<Enemy.EtherMage>() != null) return "wizzard_m";
            return "goblin";
        }

        private static string GetEnemySpritePath(GameObject enemy)
        {
            string prefix = GetEnemySpritePrefix(enemy);
            return $"Sprites/Enemy/{prefix}_idle_anim_f0";
        }

        /// <summary>
        /// 16x16 픽셀아트를 PPU=16으로 로드하여 1유닛=1타일 크기로 표시
        /// </summary>
        private static Sprite LoadPixelSprite(string path)
        {
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null) return null;

            return Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                16f); // PPU=16 → 16px 스프라이트가 1 유닛 크기
        }

        private static Sprite CreatePlaceholder()
        {
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }

        private void AutoAssignLayers()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer >= 0)
                    SetLayerRecursive(player, playerLayer);
            }

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0)
            {
                foreach (var enemy in enemies)
                    SetLayerRecursive(enemy, enemyLayer);
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                var envParent = GameObject.Find("Environment");
                if (envParent != null)
                    SetLayerRecursive(envParent, groundLayer);
            }
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}
