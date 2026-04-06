using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;
using ProjectKai.Combat;
using System.Collections;

namespace ProjectKai.Core
{
    /// <summary>
    /// 스테이지 관리: 적 카운트, 클리어 조건, 대화 트리거, 씬 전환.
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

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
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

            // 도입 대사
            if (_introDialogue != null)
            {
                DialogueSystem.Instance?.StartDialogue(_introDialogue);
            }

            Debug.Log($"[StageManager] {_stageName} 시작. 적: {_totalEnemies}마리");
        }

        private void Update()
        {
            if (!_isCleared)
                _stageTime += Time.deltaTime;
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

            yield return new WaitForSeconds(1f);

            // 클리어 대사
            if (_clearDialogue != null)
            {
                DialogueSystem.Instance?.StartDialogue(_clearDialogue);

                // 대사 끝날 때까지 대기
                while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive)
                    yield return null;
            }

            // 다음 스테이지 로드
            if (!string.IsNullOrEmpty(_nextSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(_nextSceneName);
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
    }
}
