using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;
using ProjectKai.Combat;
using System.Collections;

namespace ProjectKai.Core
{
    /// <summary>
    /// 스테이지 관리: 적 카운트, 클리어 조건, 대화 트리거, 씬 전환.
    /// StageDialogueContent에서 자동으로 대사/다음씬 로드.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("Stage Info")]
        [SerializeField] private string _stageName = "1-1";
        [SerializeField] private string _nextSceneName = "";

        [Header("Dialogue")]
        [SerializeField] private DialogueDataSO _introDialogue;
        [SerializeField] private DialogueDataSO _clearDialogue;

        [Header("Boss")]
        [SerializeField] private string _bossRoomText = "";

        private int _totalEnemies;
        private int _killedEnemies;
        private bool _isCleared;
        private float _stageTime;

        public bool IsCleared => _isCleared;
        public string StageName => _stageName;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // 자동으로 다음 씬 설정 (수동 설정이 없을 때)
            if (string.IsNullOrEmpty(_nextSceneName))
                _nextSceneName = StageDialogueContent.GetNextScene(_stageName);

            // 적 카운트
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            _totalEnemies = enemies.Length;

            // 각 적의 사망 이벤트 구독
            foreach (var e in enemies)
            {
                var dr = e.GetComponent<DamageReceiver>();
                if (dr != null)
                    dr.OnDeath += OnEnemyKilled;
            }

            // 보스방 문구
            if (!string.IsNullOrEmpty(_bossRoomText))
            {
                StartCoroutine(ShowBossRoomText());
            }

            // 도입 대사: 직렬화 데이터 없으면 코드에서 자동 로드
            StartCoroutine(PlayIntroDialogue());

            Debug.Log($"[StageManager] {_stageName} 시작. 적: {_totalEnemies}마리, 다음: {_nextSceneName}");
        }

        private IEnumerator PlayIntroDialogue()
        {
            // DialogueSystem이 준비될 때까지 대기
            yield return null;

            if (_introDialogue != null)
            {
                DialogueSystem.Instance?.StartDialogue(_introDialogue);
            }
            else
            {
                var lines = StageDialogueContent.GetIntroDialogue(_stageName);
                if (lines != null && lines.Length > 0)
                {
                    DialogueSystem.Instance?.EnsureUI();
                    DialogueSystem.Instance?.StartDialogue(lines);
                }
            }
        }

        private void Update()
        {
            if (!_isCleared)
                _stageTime += Time.deltaTime;
        }

        /// <summary>
        /// 런타임에 스폰된 적을 StageManager에 등록
        /// </summary>
        public void RegisterSpawnedEnemy(DamageReceiver dr)
        {
            _totalEnemies++;
            dr.OnDeath += OnEnemyKilled;
            Debug.Log($"[StageManager] 스폰 적 등록. 총 {_totalEnemies}마리");
        }

        private void OnEnemyKilled()
        {
            _killedEnemies++;
            Debug.Log($"[StageManager] 적 처치: {_killedEnemies}/{_totalEnemies}");

            if (_killedEnemies >= _totalEnemies && !_isCleared)
            {
                _isCleared = true;
                StartCoroutine(StageClearSequence());
            }
        }

        private IEnumerator StageClearSequence()
        {
            Debug.Log($"[StageManager] {_stageName} 클리어! 시간: {_stageTime:F1}초");

            // 챕터 진행 업데이트
            UpdateGameProgress();

            // 경험치 보너스
            if (ProgressionSystem.Instance != null)
                ProgressionSystem.Instance.AddExperience(_killedEnemies * 10);

            yield return new WaitForSeconds(1f);

            // 클리어 대사: 직렬화 데이터 없으면 코드에서 자동 로드
            DialogueLine[] clearLines = null;
            if (_clearDialogue != null)
            {
                DialogueSystem.Instance?.StartDialogue(_clearDialogue);
            }
            else
            {
                clearLines = StageDialogueContent.GetClearDialogue(_stageName);
                if (clearLines != null && clearLines.Length > 0)
                {
                    DialogueSystem.Instance?.EnsureUI();
                    DialogueSystem.Instance?.StartDialogue(clearLines);
                }
            }

            // 대사 끝날 때까지 대기
            while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive)
                yield return null;

            // 세이브
            SaveSystem.Save();

            // 2-3 패배 씬일 경우 특수 처리
            if (_stageName == "2-3")
            {
                // 패배 연출 → Hub로 강제 이동
                SceneTransition.Instance?.LoadScene("Hub");
                yield break;
            }

            // 3-3 최종 보스 클리어 시 크레딧
            if (_stageName == "3-3")
            {
                GameState.Instance?.CompleteGame();
                SaveSystem.Save();
                UI.CreditsScreen.Show();
                yield break;
            }

            // 결과 화면
            UI.StageResult.Instance?.Show(_stageName, _stageTime, _killedEnemies, _nextSceneName);
        }

        private void UpdateGameProgress()
        {
            if (GameState.Instance == null) return;

            switch (_stageName)
            {
                case "1-3": // 1장 클리어 → 2장 해금
                    GameState.Instance.UnlockChapter(2);
                    break;
                case "2-3": // 2장 패배 → 3장 해금
                    GameState.Instance.UnlockChapter(3);
                    break;
            }
        }

        private IEnumerator ShowBossRoomText()
        {
            // 보스방 문구 표시 (2초간)
            yield return new WaitForSeconds(0.5f);

            var lines = new DialogueLine[]
            {
                new DialogueLine
                {
                    speakerName = "",
                    text = _bossRoomText
                }
            };
            DialogueSystem.Instance?.StartDialogue(lines);
        }

        /// <summary>
        /// 런타임에 도입 대사를 코드로 생성
        /// </summary>
        public void SetIntroDialogue(DialogueLine[] lines)
        {
            var data = ScriptableObject.CreateInstance<DialogueDataSO>();
            data.lines = lines;
            _introDialogue = data;
        }

        /// <summary>
        /// 씬 이름에 "Stage"가 포함되면 자동으로 StageManager 생성.
        /// 씬 이름에서 스테이지 번호 추출: Stage1_1 → "1-1", Stage2_3_Boss → "2-3"
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null) return;

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!sceneName.StartsWith("Stage")) return;

            // Stage1_1 → "1-1", Stage1_3_Boss → "1-3"
            string stripped = sceneName.Replace("Stage", "").Replace("_Boss", "");
            // "1_1" → "1-1"
            string stageName = stripped.Replace("_", "-");

            var obj = new GameObject("StageManager");
            var sm = obj.AddComponent<StageManager>();

            // 리플렉션으로 _stageName 설정 (SerializeField)
            var field = typeof(StageManager).GetField("_stageName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(sm, stageName);

            Debug.Log($"[StageManager] 자동 생성: {sceneName} → 스테이지 {stageName}");
        }
    }
}
