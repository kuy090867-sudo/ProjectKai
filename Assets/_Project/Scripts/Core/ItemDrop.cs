using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Player;

namespace ProjectKai.Core
{
    /// <summary>
    /// 적 사망 시 아이템 드롭. HP 포션 또는 골드.
    /// DamageReceiver.OnDeath에 연결.
    /// 비주얼: 보빙 애니메이션 + 발광 파티클 + 획득 시 흡수 VFX.
    /// </summary>
    public class ItemDrop : MonoBehaviour
    {
        [SerializeField] private float _dropChance = 0.3f;

        private void Start()
        {
            var dr = GetComponent<DamageReceiver>();
            if (dr != null)
                dr.OnDeath += OnDeath;
        }

        private void OnDeath()
        {
            if (Random.value > _dropChance) return;

            // HP 포션 드롭
            var pickupObj = new GameObject("DroppedPotion");
            pickupObj.transform.position = transform.position + new Vector3(0, 0.5f, 0);

            var sr = pickupObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.3f, 0.3f);
            sr.sortingOrder = 3;

            var tex = new Texture2D(4, 4);
            var pix = new Color[16];
            for (int i = 0; i < 16; i++) pix[i] = Color.white;
            tex.SetPixels(pix); tex.filterMode = FilterMode.Point; tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

            pickupObj.transform.localScale = new Vector3(0.4f, 0.5f, 1f);

            var rb = pickupObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), 4f);

            var col = pickupObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            pickupObj.AddComponent<HealthPickup>();

            // 비주얼 이펙트 컴포넌트 추가
            var visual = pickupObj.AddComponent<DroppedItemVisual>();
            visual.Initialize(new Color(1f, 0.3f, 0.3f)); // 포션 색상

            Destroy(pickupObj, 10f);
        }
    }

    /// <summary>
    /// 드롭된 아이템의 비주얼 이펙트.
    /// - 위아래 보빙 애니메이션 (사인파)
    /// - 발광 효과 (주변 빛 파티클)
    /// - 획득 시 흡수 VFX
    /// </summary>
    public class DroppedItemVisual : MonoBehaviour
    {
        private Color _itemColor;
        private SpriteRenderer _sr;
        private Rigidbody2D _rb;
        private ParticleSystem _glowParticles;
        private bool _landed;
        private float _bobTime;
        private float _baseY;

        // 보빙 설정
        private const float BobAmplitude = 0.12f;
        private const float BobSpeed = 2.5f;

        // 착지 판정
        private const float LandCheckDelay = 0.5f;
        private float _spawnTime;

        public void Initialize(Color itemColor)
        {
            _itemColor = itemColor;
        }

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            _spawnTime = Time.time;
        }

        private void Update()
        {
            CheckLanded();

            if (_landed)
            {
                ApplyBobbing();
                ApplyGlowPulse();
            }
        }

        // ===============================================
        //  착지 감지
        // ===============================================

        private void CheckLanded()
        {
            if (_landed) return;
            if (Time.time - _spawnTime < LandCheckDelay) return;

            // Rigidbody 속도가 거의 0이면 착지 판정
            if (_rb != null && _rb.linearVelocity.magnitude < 0.1f)
            {
                _landed = true;
                _baseY = transform.position.y;

                // 착지 후 물리 제거 (보빙만 적용)
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;

                // 발광 파티클 시작
                CreateGlowParticles();
            }
        }

        // ===============================================
        //  보빙 애니메이션 (사인파)
        // ===============================================

        private void ApplyBobbing()
        {
            _bobTime += Time.deltaTime * BobSpeed;
            float offsetY = Mathf.Sin(_bobTime) * BobAmplitude;

            var pos = transform.position;
            pos.y = _baseY + offsetY;
            transform.position = pos;
        }

        // ===============================================
        //  발광 펄스
        // ===============================================

        private void ApplyGlowPulse()
        {
            if (_sr == null) return;

            float pulse = 0.7f + Mathf.Sin(Time.time * 3f) * 0.3f;
            _sr.color = new Color(
                _itemColor.r * pulse + (1f - pulse) * 0.3f,
                _itemColor.g * pulse,
                _itemColor.b * pulse,
                1f);
        }

        // ===============================================
        //  발광 파티클 (주변 빛)
        // ===============================================

        private void CreateGlowParticles()
        {
            var psObj = new GameObject("GlowParticles");
            psObj.transform.SetParent(transform);
            psObj.transform.localPosition = Vector3.zero;

            _glowParticles = psObj.AddComponent<ParticleSystem>();

            // Main
            var main = _glowParticles.main;
            main.maxParticles = 8;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);

            // 아이템 색상 기반 발광 색 (밝게)
            Color glowColor = new Color(
                Mathf.Min(1f, _itemColor.r + 0.4f),
                Mathf.Min(1f, _itemColor.g + 0.4f),
                Mathf.Min(1f, _itemColor.b + 0.4f),
                0.7f);
            main.startColor = glowColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.1f;

            // Emission
            var emission = _glowParticles.emission;
            emission.rateOverTime = 4f;

            // Shape: 작은 원
            var shape = _glowParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;

            // Size over Lifetime: 점점 작아짐
            var sizeOverLifetime = _glowParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Color over Lifetime: 알파 페이드
            var colorOverLifetime = _glowParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(glowColor, 0f),
                    new GradientColorKey(glowColor, 1f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.7f, 0.3f),
                    new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Renderer 머티리얼 설정
            var renderer = psObj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    renderer.material = new Material(shader);
                renderer.sortingOrder = 4;
            }

            _glowParticles.Play();
        }

        // ===============================================
        //  획득 시 흡수 VFX
        // ===============================================

        private void OnDestroy()
        {
            // 플레이어가 획득했을 때만 VFX (타임아웃 자동파괴가 아닌 경우)
            // HealthPickup이 Destroy(gameObject)를 호출하면 여기로 온다.
            // 플레이어가 근처에 있는지 확인하여 획득인지 판별
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist > 3f) return; // 멀면 타임아웃 파괴로 간주

            SpawnAbsorptionVFX(transform.position, player.transform.position);
        }

        /// <summary>
        /// 아이템이 플레이어에게 빨려들어가는 흡수 이펙트.
        /// 작은 파티클들이 플레이어 방향으로 수렴.
        /// </summary>
        private void SpawnAbsorptionVFX(Vector3 itemPos, Vector3 playerPos)
        {
            var vfxObj = new GameObject("VFX_ItemAbsorb");
            vfxObj.transform.position = itemPos;

            var ps = vfxObj.AddComponent<ParticleSystem>();

            // Main
            var main = ps.main;
            main.maxParticles = 10;
            main.startLifetime = 0.4f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
            main.startColor = new Color(
                Mathf.Min(1f, _itemColor.r + 0.3f),
                Mathf.Min(1f, _itemColor.g + 0.3f),
                Mathf.Min(1f, _itemColor.b + 0.3f),
                1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;

            // Shape: 원형 (아이템 위치 주변)
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            // Emission: Burst
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 8, 10)
            });

            // Velocity over Lifetime: 플레이어 방향으로 가속
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            Vector3 dir = (playerPos - itemPos).normalized;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(dir.x * 3f, dir.x * 6f);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(dir.y * 3f, dir.y * 6f);

            // Size over Lifetime: 줄어듦
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f));

            // Color over Lifetime: 밝아지다 페이드 아웃
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(_itemColor, 0f),
                    new GradientColorKey(Color.white, 0.6f),
                    new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Renderer
            var renderer = vfxObj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                    renderer.material = new Material(shader);
                renderer.sortingOrder = 10;
            }

            ps.Play();
            Object.Destroy(vfxObj, 1f);
        }
    }
}
