using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ProjectKai.Core
{
    /// <summary>
    /// 오디오 매니저. WAV 파일 로드 우선, 없으면 개선된 합성음 사용.
    /// 피치 랜덤화로 반복감 제거. 씬별 자동 BGM 재생.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        [Range(0f, 1f)] public float bgmVolume = 0.5f;

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;
        private Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> _bgmClips = new Dictionary<string, AudioClip>();
        private string _currentBgmKey;

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
            GenerateBGMs();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AutoPlayBGM(scene.name);
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

        public void PlayBGM(string key)
        {
            if (_currentBgmKey == key && _bgmSource.isPlaying) return;

            // WAV 파일 먼저 시도
            var clip = Resources.Load<AudioClip>(key);
            // 절차적 BGM 시도
            if (clip == null && _bgmClips.TryGetValue(key, out var synthClip))
                clip = synthClip;

            if (clip != null)
            {
                _currentBgmKey = key;
                _bgmSource.clip = clip;
                _bgmSource.volume = bgmVolume;
                _bgmSource.Play();
            }
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
            _currentBgmKey = null;
        }

        private void AutoPlayBGM(string sceneName)
        {
            if (sceneName == "MainMenu")
                PlayBGM("menu");
            else if (sceneName == "Hub")
                PlayBGM("hub");
            else if (sceneName.Contains("Boss"))
                PlayBGM("boss");
            else if (sceneName.StartsWith("Stage"))
                PlayBGM("stage");
        }

        private void GenerateBGMs()
        {
            _bgmClips["menu"] = SynthMenuBGM();
            _bgmClips["hub"] = SynthHubBGM();
            _bgmClips["stage"] = SynthStageBGM();
            _bgmClips["boss"] = SynthBossBGM();
        }

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

        // === 절차적 BGM 생성 ===

        private static float Note(int semitone)
        {
            // A4 = 440Hz 기준, 반음 계산
            return 440f * Mathf.Pow(2f, (semitone - 69) / 12f);
        }

        private static float Triangle(float phase)
        {
            float p = phase % 1f;
            return p < 0.5f ? p * 4f - 1f : 3f - p * 4f;
        }

        private static float Square(float phase)
        {
            return (phase % 1f) < 0.5f ? 0.5f : -0.5f;
        }

        private AudioClip GenerateBGMClip(string name, float bpm, int bars, System.Action<float[], int, float> fillFunc)
        {
            int rate = 44100;
            float beatDur = 60f / bpm;
            float barDur = beatDur * 4f;
            float totalDur = barDur * bars;
            int count = (int)(rate * totalDur);
            float[] data = new float[count];
            fillFunc(data, rate, bpm);
            var clip = AudioClip.Create(name, count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>메인메뉴: 느린 패드 + 아르페지오</summary>
        private AudioClip SynthMenuBGM()
        {
            // C minor: C3-Eb3-G3-Bb3, Ab2-C3-Eb3, Bb2-D3-F3, G2-Bb2-D3
            int[][] chords = {
                new[] { 48, 51, 55 },     // Cm
                new[] { 44, 48, 51 },     // Ab
                new[] { 46, 50, 53 },     // Bb
                new[] { 43, 46, 50 }      // Gm
            };

            return GenerateBGMClip("menu_bgm", 72f, 8, (data, rate, bpm) =>
            {
                float beatDur = 60f / bpm;
                for (int i = 0; i < data.Length; i++)
                {
                    float time = (float)i / rate;
                    int bar = (int)(time / (beatDur * 4f)) % 4;
                    float barTime = time % (beatDur * 4f);
                    var chord = chords[bar];

                    // 패드 (삼각파, 부드러운 코드)
                    float pad = 0f;
                    for (int n = 0; n < chord.Length; n++)
                        pad += Triangle(time * Note(chord[n])) * 0.06f;

                    // 아르페지오 (8분음표)
                    int arpIdx = (int)(barTime / (beatDur * 0.5f)) % chord.Length;
                    float arpFreq = Note(chord[arpIdx] + 12); // 1옥타브 위
                    float arpPhase = (barTime % (beatDur * 0.5f)) / (beatDur * 0.5f);
                    float arpEnv = Mathf.Exp(-arpPhase * 4f);
                    float arp = Mathf.Sin(Mathf.PI * 2f * arpFreq * time) * arpEnv * 0.08f;

                    // 서브 베이스
                    float bassFreq = Note(chord[0] - 12);
                    float bass = Mathf.Sin(Mathf.PI * 2f * bassFreq * time) * 0.05f;

                    data[i] = (pad + arp + bass) * 0.7f;
                }
            });
        }

        /// <summary>거점: 잔잔하고 쓸쓸한 아르페지오</summary>
        private AudioClip SynthHubBGM()
        {
            // Am pentatonic melody notes
            int[] melody = { 57, 60, 64, 60, 57, 55, 52, 55, 57, 60, 64, 67, 64, 60, 57, 55 };
            int[][] chords = {
                new[] { 45, 48, 52 },     // Am
                new[] { 43, 47, 50 },     // G
                new[] { 41, 45, 48 },     // F
                new[] { 40, 43, 47 }      // Em
            };

            return GenerateBGMClip("hub_bgm", 80f, 8, (data, rate, bpm) =>
            {
                float beatDur = 60f / bpm;
                for (int i = 0; i < data.Length; i++)
                {
                    float time = (float)i / rate;
                    int bar = (int)(time / (beatDur * 4f)) % 4;
                    float barTime = time % (beatDur * 4f);
                    var chord = chords[bar];

                    // 멜로디 (삼각파)
                    int melIdx = (int)(time / beatDur) % melody.Length;
                    float melEnv = Mathf.Exp(-((time % beatDur) / beatDur) * 3f);
                    float mel = Triangle(time * Note(melody[melIdx])) * melEnv * 0.1f;

                    // 코드 패드
                    float pad = 0f;
                    for (int n = 0; n < chord.Length; n++)
                        pad += Mathf.Sin(Mathf.PI * 2f * Note(chord[n]) * time) * 0.04f;

                    // 베이스
                    float bass = Triangle(time * Note(chord[0] - 12)) * 0.05f;

                    data[i] = (mel + pad + bass) * 0.6f;
                }
            });
        }

        /// <summary>스테이지: 긴장감 있는 드라이빙 비트</summary>
        private AudioClip SynthStageBGM()
        {
            int[][] chords = {
                new[] { 48, 51, 55 },     // Cm
                new[] { 46, 50, 53 },     // Bb
                new[] { 44, 48, 51 },     // Ab
                new[] { 46, 50, 53 }      // Bb
            };

            return GenerateBGMClip("stage_bgm", 130f, 8, (data, rate, bpm) =>
            {
                float beatDur = 60f / bpm;
                for (int i = 0; i < data.Length; i++)
                {
                    float time = (float)i / rate;
                    int bar = (int)(time / (beatDur * 4f)) % 4;
                    float barTime = time % (beatDur * 4f);
                    float beatPhase = (time % beatDur) / beatDur;
                    var chord = chords[bar];

                    // 킥 드럼 (매 박)
                    float kickEnv = Mathf.Exp(-beatPhase * 20f);
                    float kick = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(150f, 40f, beatPhase) * time) * kickEnv * 0.15f;

                    // 하이햇 (8분음표)
                    float hhPhase = (time % (beatDur * 0.5f)) / (beatDur * 0.5f);
                    float hhEnv = Mathf.Exp(-hhPhase * 30f);
                    float hh = ((Mathf.Sin(time * 7919f) + Mathf.Sin(time * 4513f)) * 0.5f) * hhEnv * 0.04f;

                    // 베이스 (사각파)
                    float bassNote = Note(chord[0] - 12);
                    float bassPhase16 = (barTime % (beatDur * 0.25f)) / (beatDur * 0.25f);
                    int bassPattern = (int)(barTime / (beatDur * 0.25f)) % 16;
                    bool bassHit = bassPattern == 0 || bassPattern == 3 || bassPattern == 6 || bassPattern == 10;
                    float bassEnv = bassHit ? Mathf.Exp(-bassPhase16 * 8f) : 0f;
                    float bass = Square(time * bassNote) * bassEnv * 0.08f;

                    // 리드 아르페지오 (16분음표)
                    int arpIdx = (int)(barTime / (beatDur * 0.25f)) % chord.Length;
                    float arpFreq = Note(chord[arpIdx % chord.Length] + 12);
                    float arpEnv = Mathf.Exp(-bassPhase16 * 6f);
                    float arp = Triangle(time * arpFreq) * arpEnv * 0.06f;

                    data[i] = Mathf.Clamp(kick + hh + bass + arp, -0.8f, 0.8f);
                }
            });
        }

        /// <summary>보스: 강렬하고 빠른 전투 음악</summary>
        private AudioClip SynthBossBGM()
        {
            int[][] chords = {
                new[] { 48, 51, 55, 58 },  // Cm7
                new[] { 46, 49, 53, 56 },  // Bbm7
                new[] { 44, 48, 51, 55 },  // AbM7
                new[] { 43, 46, 50, 55 }   // G(sus)
            };

            return GenerateBGMClip("boss_bgm", 155f, 8, (data, rate, bpm) =>
            {
                float beatDur = 60f / bpm;
                for (int i = 0; i < data.Length; i++)
                {
                    float time = (float)i / rate;
                    int bar = (int)(time / (beatDur * 4f)) % 4;
                    float barTime = time % (beatDur * 4f);
                    float beatPhase = (time % beatDur) / beatDur;
                    var chord = chords[bar];

                    // 더블 킥 (8분음표)
                    float kickPhase = (time % (beatDur * 0.5f)) / (beatDur * 0.5f);
                    float kickEnv = Mathf.Exp(-kickPhase * 25f);
                    float kick = Mathf.Sin(Mathf.PI * 2f * Mathf.Lerp(180f, 35f, kickPhase) * time) * kickEnv * 0.18f;

                    // 스네어 (2, 4박)
                    int beatNum = (int)(barTime / beatDur) % 4;
                    float snareEnv = (beatNum == 1 || beatNum == 3) ? Mathf.Exp(-beatPhase * 15f) : 0f;
                    float snare = Mathf.Sin(time * 3571f) * snareEnv * 0.1f;

                    // 하이햇 (16분음표)
                    float hh16Phase = (time % (beatDur * 0.25f)) / (beatDur * 0.25f);
                    float hhEnv = Mathf.Exp(-hh16Phase * 40f);
                    float hh = Mathf.Sin(time * 8831f) * hhEnv * 0.03f;

                    // 파워 코드 (디스토션 사각파)
                    float powerChord = 0f;
                    powerChord += Square(time * Note(chord[0] - 12)) * 0.06f;
                    powerChord += Square(time * Note(chord[0] - 5)) * 0.04f; // 5도

                    // 리드 멜로디 (빠른 아르페지오)
                    int arpIdx = (int)(barTime / (beatDur * 0.25f)) % chord.Length;
                    float arpFreq = Note(chord[arpIdx] + 12);
                    float arpEnv = Mathf.Exp(-hh16Phase * 5f);
                    float arp = Triangle(time * arpFreq) * arpEnv * 0.07f;

                    data[i] = Mathf.Clamp(kick + snare + hh + powerChord + arp, -0.9f, 0.9f);
                }
            });
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
