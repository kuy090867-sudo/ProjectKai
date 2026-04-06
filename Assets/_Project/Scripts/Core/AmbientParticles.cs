using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 씬별 환경 파티클. 분위기 연출.
    /// Stage 씬에서 자동 생성.
    /// </summary>
    public class AmbientParticles : MonoBehaviour
    {
        private ParticleSystem _dustParticles;
        private ParticleSystem _magicParticles;

        private void Start()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            ConfigureForScene(sceneName);
        }

        private void ConfigureForScene(string sceneName)
        {
            if (sceneName.StartsWith("Stage1"))
                CreateDustParticles(new Color(0.5f, 0.4f, 0.3f, 0.3f)); // 던전 먼지
            else if (sceneName.StartsWith("Stage2"))
                CreateMagicParticles(new Color(0.3f, 0.5f, 0.8f, 0.4f)); // 에테르 파티클
            else if (sceneName.StartsWith("Stage3"))
                CreateMagicParticles(new Color(0.8f, 0.3f, 0.3f, 0.4f)); // 붉은 마법 파티클
            else if (sceneName == "Hub")
                CreateDustParticles(new Color(0.6f, 0.55f, 0.4f, 0.2f)); // 잔잔한 먼지

            // 보스방: 추가 이펙트
            if (sceneName.Contains("Boss"))
                CreateBossAura();
        }

        private void CreateDustParticles(Color color)
        {
            var obj = new GameObject("DustParticles");
            obj.transform.SetParent(transform);
            var ps = obj.AddComponent<ParticleSystem>();
            _dustParticles = ps;

            var main = ps.main;
            main.maxParticles = 50;
            main.startLifetime = 4f;
            main.startSpeed = 0.3f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.02f;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 8f, 0f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f),
                        new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 5;

            // 카메라 따라가기
            obj.AddComponent<FollowCamera>();
        }

        private void CreateMagicParticles(Color color)
        {
            var obj = new GameObject("MagicParticles");
            obj.transform.SetParent(transform);
            var ps = obj.AddComponent<ParticleSystem>();
            _magicParticles = ps;

            var main = ps.main;
            main.maxParticles = 30;
            main.startLifetime = 3f;
            main.startSpeed = 0.5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.05f;

            var emission = ps.emission;
            emission.rateOverTime = 5f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(15f, 6f, 0f);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f),
                        new GradientAlphaKey(0.5f, 0.7f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 5;

            obj.AddComponent<FollowCamera>();
        }

        private void CreateBossAura()
        {
            var obj = new GameObject("BossAura");
            obj.transform.SetParent(transform);
            var ps = obj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.maxParticles = 20;
            main.startLifetime = 2f;
            main.startSpeed = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new Color(1f, 0.4f, 0.1f, 0.5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.1f;

            var emission = ps.emission;
            emission.rateOverTime = 10f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 1f, 0f);
            shape.position = new Vector3(0f, -2f, 0f);

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = -1;

            obj.AddComponent<FollowCamera>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Stage") || sceneName == "Hub")
            {
                if (FindFirstObjectByType<AmbientParticles>() == null)
                {
                    var obj = new GameObject("AmbientParticles");
                    obj.AddComponent<AmbientParticles>();
                }
            }
        }
    }

    /// <summary>카메라 위치를 따라가는 헬퍼</summary>
    public class FollowCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main != null)
            {
                transform.position = new Vector3(
                    Camera.main.transform.position.x,
                    Camera.main.transform.position.y,
                    0f);
            }
        }
    }
}
