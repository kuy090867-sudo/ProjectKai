using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// VFX 중앙 매니저. 모든 파티클 이펙트를 코드로 생성 (프리팹 불필요).
    /// 싱글톤 패턴 + RuntimeInitializeOnLoadMethod.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        private Material _defaultParticleMaterial;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CacheDefaultMaterial();
        }

        private void CacheDefaultMaterial()
        {
            // Sprites-Default 셰이더 기반 머티리얼 생성
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _defaultParticleMaterial = new Material(shader);
        }

        /// <summary>
        /// 기본 ParticleSystem GameObject를 생성하고 공통 설정 적용.
        /// </summary>
        private ParticleSystem CreateBaseParticle(string name, Vector3 position, float autoDestroyDelay)
        {
            var go = new GameObject(name);
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();

            // 기본 emission 끄기 (Burst로 제어)
            var emission = ps.emission;
            emission.rateOverTime = 0f;

            // 렌더러 머티리얼 설정
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && _defaultParticleMaterial != null)
            {
                renderer.material = _defaultParticleMaterial;
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            // 자동 파괴
            Destroy(go, autoDestroyDelay);

            return ps;
        }

        // ===================================================================
        // 슬래시 이펙트
        // ===================================================================

        /// <summary>
        /// 근접 공격 슬래시 이펙트.
        /// comboStep 0: 수평 슬래시 (3개 파티클, 흰색)
        /// comboStep 1: 대각선 슬래시 (4개 파티클, 노란색)
        /// comboStep 2: 큰 수직 슬래시 (6개 파티클, 주황색, 1.5배)
        /// </summary>
        public void SlashEffect(Vector3 position, int comboStep, int facingDir)
        {
            int burstCount;
            Color slashColor;
            float sizeMultiplier;
            float rotationZ;

            switch (comboStep)
            {
                case 0: // 수평 슬래시
                    burstCount = 3;
                    slashColor = Color.white;
                    sizeMultiplier = 1f;
                    rotationZ = 0f;
                    break;
                case 1: // 대각선 슬래시
                    burstCount = 4;
                    slashColor = new Color(1f, 0.95f, 0.4f, 1f); // 노란색
                    sizeMultiplier = 1f;
                    rotationZ = facingDir > 0 ? -45f : 45f;
                    break;
                default: // 큰 수직 슬래시 (3타)
                    burstCount = 6;
                    slashColor = new Color(1f, 0.6f, 0.2f, 1f); // 주황색
                    sizeMultiplier = 1.5f;
                    rotationZ = facingDir > 0 ? -90f : 90f;
                    break;
            }

            var ps = CreateBaseParticle("VFX_Slash", position, 0.3f);
            var go = ps.gameObject;

            // facingDir에 따라 회전 적용
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

            // Main 모듈
            var main = ps.main;
            main.startLifetime = 0.15f;
            main.startSpeed = 2f * sizeMultiplier;
            main.startSize = 0.15f * sizeMultiplier;
            main.startColor = slashColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            main.maxParticles = burstCount;

            // 방향에 따른 시작 속도 오프셋
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f * sizeMultiplier, 3f * sizeMultiplier);

            // Shape: 호 형태
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f * sizeMultiplier;
            shape.arc = 90f;
            shape.rotation = new Vector3(0f, 0f, facingDir > 0 ? -45f : 135f);

            // Emission: Burst
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)burstCount)
            });

            // Size over Lifetime: 점점 작아짐
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            // Color over Lifetime: 페이드 아웃
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(slashColor, 0f), new GradientColorKey(slashColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            ps.Play();
        }

        // ===================================================================
        // 먼지 이펙트
        // ===================================================================

        /// <summary>
        /// 착지/대시 시 작은 먼지 구름.
        /// Circle shape, Burst 5개, 갈색 반투명, 0.3초 수명.
        /// </summary>
        public void DustPuff(Vector3 position)
        {
            var ps = CreateBaseParticle("VFX_DustPuff", position, 0.5f);

            // Main 모듈
            var main = ps.main;
            main.startLifetime = 0.3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
            main.startColor = new Color(0.6f, 0.5f, 0.35f, 0.5f); // 갈색 반투명
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.1f; // 약간 위로 부유
            main.maxParticles = 5;

            // Shape: 원형
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            // Emission: Burst 5개
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 5)
            });

            // Size over Lifetime: 감소 curve
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            var sizeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over Lifetime: 알파 페이드 아웃
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.6f, 0.5f, 0.35f), 0f), new GradientColorKey(new Color(0.6f, 0.5f, 0.35f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            ps.Play();
        }

        // ===================================================================
        // 적 사망 파편 이펙트
        // ===================================================================

        /// <summary>
        /// 적 사망 시 파편 폭발.
        /// Burst 8-12개, 중력 0.5, startSpeed 3-5 랜덤, 색상 파라미터.
        /// </summary>
        public void EnemyDeathEffect(Vector3 position, Color color)
        {
            var ps = CreateBaseParticle("VFX_EnemyDeath", position, 1f);

            // Main 모듈
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;
            main.maxParticles = 12;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            // Shape: 전방향 폭발
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            // Emission: Burst 8~12개
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 8, 12)
            });

            // Size over Lifetime: 줄어듦
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            // Color over Lifetime: 색 유지 + 알파 페이드 아웃
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.5f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Rotation over Lifetime: 파편 회전
            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

            ps.Play();
        }

        // ===================================================================
        // 타격 스파크 이펙트
        // ===================================================================

        /// <summary>
        /// 타격 시 작은 스파크.
        /// Burst 3개, 수명 0.1초, 밝은 흰색-노란색.
        /// </summary>
        public void HitEffect(Vector3 position)
        {
            var ps = CreateBaseParticle("VFX_Hit", position, 0.2f);

            // Main 모듈
            var main = ps.main;
            main.startLifetime = 0.1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
            main.startColor = new Color(1f, 1f, 0.8f, 1f); // 밝은 흰색-노란색
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;
            main.maxParticles = 3;

            // Shape: 작은 원
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            // Emission: Burst 3개
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, 3)
            });

            // Size over Lifetime: 빠르게 줄어듦
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            // Color over Lifetime: 흰색 → 노란색, 알파 페이드
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.9f, 0.3f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            ps.Play();
        }

        // ===================================================================
        // 자동 초기화
        // ===================================================================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("VFXManager");
                obj.AddComponent<VFXManager>();
            }
        }
    }
}
