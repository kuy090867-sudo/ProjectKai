using UnityEngine;
using System.Collections.Generic;

namespace ProjectKai.Core
{
    /// <summary>
    /// 오디오 매니저. WAV 파일 로드 우선, 없으면 개선된 합성음 사용.
    /// 피치 랜덤화로 반복감 제거.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        [Range(0f, 1f)] public float bgmVolume = 0.5f;

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;
        private Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;

            LoadSounds();
        }

        private void LoadSounds()
        {
            // WAV 파일 우선 로드, 없으면 합성
            TryLoadOrSynth("sword_swing", "Audio/SFX/sword_swing", () => SynthSlash(0.12f));
            TryLoadOrSynth("sword_hit", "Audio/SFX/sword_hit", () => SynthImpact(0.1f, 200f));
            TryLoadOrSynth("hit", "Audio/SFX/player_hit", () => SynthImpact(0.15f, 120f));
            TryLoadOrSynth("jump", "Audio/SFX/jump", () => SynthTone(0.08f, 350f, 550f));
            TryLoadOrSynth("dash", "Audio/SFX/dash", () => SynthWhoosh(0.15f));
            TryLoadOrSynth("land", "Audio/SFX/land", () => SynthImpact(0.08f, 80f));
            TryLoadOrSynth("enemy_death", "Audio/SFX/enemy_death", () => SynthDeath(0.4f));
            TryLoadOrSynth("footstep", "Audio/SFX/footstep", () => SynthImpact(0.05f, 60f));
        }

        private void TryLoadOrSynth(string key, string resourcePath, System.Func<AudioClip> synth)
        {
            var clip = Resources.Load<AudioClip>(resourcePath);
            _clips[key] = clip != null ? clip : synth();
        }

        /// <summary>
        /// SFX 재생. 피치 랜덤화로 반복감 제거.
        /// </summary>
        public void PlaySFX(string name, float volume = 1f)
        {
            if (_clips.TryGetValue(name, out var clip))
            {
                _sfxSource.pitch = Random.Range(0.9f, 1.1f);
                _sfxSource.PlayOneShot(clip, volume * sfxVolume);
            }
        }

        public void PlayBGM(string resourcePath)
        {
            var clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null)
            {
                _bgmSource.clip = clip;
                _bgmSource.volume = bgmVolume;
                _bgmSource.Play();
            }
        }

        public void StopBGM() => _bgmSource.Stop();

        // === 개선된 합성음 (노이즈+하모닉스+엔벨로프) ===

        private AudioClip SynthSlash(float duration)
        {
            return GenerateClip(duration, (t, d) =>
            {
                float noise = (Random.value - 0.5f) * 0.4f;
                float sweep = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(800f, 200f, t) * t * d);
                float env = (1f - t) * Mathf.Min(t * 20f, 1f); // fast attack, decay
                return (sweep * 0.5f + noise) * env * 0.3f;
            });
        }

        private AudioClip SynthImpact(float duration, float freq)
        {
            return GenerateClip(duration, (t, d) =>
            {
                float noise = (Random.value - 0.5f) * 0.6f;
                float tone = Mathf.Sin(Mathf.PI * 2f * freq * (1f - t) * t * d);
                float env = Mathf.Exp(-t * 8f); // sharp decay
                return (tone * 0.4f + noise * 0.6f) * env * 0.4f;
            });
        }

        private AudioClip SynthWhoosh(float duration)
        {
            return GenerateClip(duration, (t, d) =>
            {
                float noise = (Random.value - 0.5f) * 0.8f;
                float env = Mathf.Sin(Mathf.PI * t); // fade in/out
                return noise * env * 0.2f;
            });
        }

        private AudioClip SynthDeath(float duration)
        {
            return GenerateClip(duration, (t, d) =>
            {
                float tone1 = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(400f, 60f, t) * t * d);
                float tone2 = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(300f, 40f, t) * t * d) * 0.5f;
                float noise = (Random.value - 0.5f) * 0.3f;
                float env = (1f - t * t);
                return (tone1 + tone2 + noise) * env * 0.3f;
            });
        }

        private AudioClip SynthTone(float duration, float startFreq, float endFreq)
        {
            return GenerateClip(duration, (t, d) =>
            {
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float env = 1f - t;
                return Mathf.Sin(Mathf.PI * 2f * freq * t * d) * env * 0.3f;
            });
        }

        private AudioClip GenerateClip(float duration, System.Func<float, float, float> generator)
        {
            int rate = 44100;
            int count = (int)(rate * duration);
            var clip = AudioClip.Create("synth", count, 1, rate, false);
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
                data[i] = generator((float)i / count, duration);
            clip.SetData(data, 0);
            return clip;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("AudioManager");
                obj.AddComponent<AudioManager>();
            }
        }
    }
}
