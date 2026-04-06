using UnityEngine;
using ProjectKai.Data;

namespace ProjectKai.Core
{
    /// <summary>
    /// 게임 시작 시 필요한 ScriptableObject 데이터가 없으면 런타임에 자동 생성.
    /// 에디터에서 직접 만들지 않아도 기본 데이터로 동작 가능.
    /// </summary>
    public static class DataBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            EnsureComboData();
            EnsureWeaponData();
        }

        private static void EnsureComboData()
        {
            var existing = Resources.Load<ComboDataSO>("Data/BasicSwordCombo");
            if (existing != null) return;

            // Resources에 없으면 런타임 인스턴스 생성
            var combo = ScriptableObject.CreateInstance<ComboDataSO>();
            combo.comboName = "Basic Sword Combo";
            combo.comboResetTime = 0.6f;
            combo.steps = new ComboStep[]
            {
                new ComboStep
                {
                    animationTrigger = "Attack1",
                    damage = 15f,
                    duration = 0.35f,
                    hitStartTime = 0.1f,
                    hitEndTime = 0.25f,
                    knockbackForce = 4f,
                    hitboxOffset = new Vector2(0.8f, 0f),
                    hitboxSize = new Vector2(1.2f, 0.8f),
                    moveForwardForce = 3f
                },
                new ComboStep
                {
                    animationTrigger = "Attack2",
                    damage = 18f,
                    duration = 0.35f,
                    hitStartTime = 0.1f,
                    hitEndTime = 0.25f,
                    knockbackForce = 5f,
                    hitboxOffset = new Vector2(0.9f, 0f),
                    hitboxSize = new Vector2(1.3f, 0.9f),
                    moveForwardForce = 4f
                },
                new ComboStep
                {
                    animationTrigger = "Attack3",
                    damage = 25f,
                    duration = 0.5f,
                    hitStartTime = 0.15f,
                    hitEndTime = 0.35f,
                    knockbackForce = 8f,
                    hitboxOffset = new Vector2(1f, 0f),
                    hitboxSize = new Vector2(1.5f, 1f),
                    moveForwardForce = 6f
                }
            };

            // 런타임 캐시에 등록
            RuntimeDataCache.SwordCombo = combo;
            Debug.Log("[DataBootstrap] Basic Sword Combo 런타임 생성 완료");
        }

        private static void EnsureWeaponData()
        {
            var existing = Resources.Load<WeaponDataSO>("Data/MagicPistol");
            if (existing != null) return;

            var weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            weapon.weaponName = "Magic Pistol";
            weapon.weaponType = WeaponType.Ranged;
            weapon.baseDamage = 8f;
            weapon.attackSpeed = 1.5f;
            weapon.range = 15f;
            weapon.knockbackForce = 3f;
            weapon.projectileSpeed = 18f;
            weapon.fireRate = 0.25f;

            // 프로젝타일 프리팹 런타임 생성
            var bulletObj = new GameObject("MagicBullet_Prefab");
            var sr = bulletObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.3f, 0.8f, 1f, 1f);

            // 작은 흰색 스프라이트 생성
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

            var rb = bulletObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            var col = bulletObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.3f, 0.3f);

            bulletObj.AddComponent<Combat.Projectile>();
            bulletObj.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            bulletObj.SetActive(false);

            weapon.projectilePrefab = bulletObj;
            Object.DontDestroyOnLoad(bulletObj);

            RuntimeDataCache.MagicPistol = weapon;
            Debug.Log("[DataBootstrap] Magic Pistol + Projectile 런타임 생성 완료");
        }
    }

    /// <summary>
    /// Resources에 에셋이 없을 때 런타임 생성된 데이터를 캐시
    /// </summary>
    public static class RuntimeDataCache
    {
        public static ComboDataSO SwordCombo;
        public static WeaponDataSO MagicPistol;
    }
}
