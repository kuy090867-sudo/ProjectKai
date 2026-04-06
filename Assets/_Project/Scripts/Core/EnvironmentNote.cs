using UnityEngine;
using TMPro;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    /// <summary>
    /// 환경 스토리텔링 시스템 -- 스테이지에 읽을 수 있는 메모/문양 배치.
    /// 플레이어가 2유닛 이내 접근 시 "E: 읽기" 프롬프트 표시.
    /// E키 누르면 DialogueSystem으로 메모 내용 표시.
    /// RuntimeInitializeOnLoadMethod로 각 스테이지에 자동 배치.
    /// </summary>
    public class EnvironmentNote : MonoBehaviour
    {
        [SerializeField] private string _title = "환경 메모";
        [TextArea(2, 5)]
        [SerializeField] private string _content = "";
        [SerializeField] private float _interactRange = 2f;

        private Transform _player;
        private GameObject _promptObj;
        private TextMeshPro _promptText;
        private bool _isPlayerNear;
        private bool _dialogueActive;
        private float _promptPulse;

        // ===============================================
        //  초기화
        // ===============================================

        private void Start()
        {
            CreateIcon();
            CreatePrompt();
        }

        /// <summary>
        /// 메모 아이콘 (느낌표) 생성 -- 16x16 픽셀 아트
        /// </summary>
        private void CreateIcon()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();

            int size = 16;
            var tex = new Texture2D(size, size);
            var pixels = new Color[size * size];

            // 배경: 투명
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            // 책 모양 아이콘
            // 책 표지 (갈색 사각형)
            for (int y = 2; y < 14; y++)
            {
                for (int x = 3; x < 13; x++)
                {
                    pixels[y * size + x] = new Color(0.5f, 0.3f, 0.15f, 1f);
                }
            }
            // 책 페이지 (베이지 안쪽)
            for (int y = 3; y < 13; y++)
            {
                for (int x = 4; x < 12; x++)
                {
                    pixels[y * size + x] = new Color(0.9f, 0.85f, 0.7f, 1f);
                }
            }
            // 느낌표 (가운데, 밝은 노란색)
            // 세로 줄
            for (int y = 6; y < 12; y++)
            {
                pixels[y * size + 7] = new Color(1f, 0.85f, 0.2f, 1f);
                pixels[y * size + 8] = new Color(1f, 0.85f, 0.2f, 1f);
            }
            // 점
            pixels[4 * size + 7] = new Color(1f, 0.85f, 0.2f, 1f);
            pixels[4 * size + 8] = new Color(1f, 0.85f, 0.2f, 1f);

            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();

            sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 16f);
            sr.sortingOrder = 5;

            transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        }

        /// <summary>
        /// "E: 읽기" 프롬프트 (WorldSpace TMP)
        /// </summary>
        private void CreatePrompt()
        {
            _promptObj = new GameObject("NotePrompt");
            _promptObj.transform.SetParent(transform);
            _promptObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            _promptText = _promptObj.AddComponent<TextMeshPro>();
            _promptText.text = "E: 읽기";
            _promptText.fontSize = 4f;
            _promptText.color = new Color(1f, 0.9f, 0.5f, 0.9f);
            _promptText.alignment = TextAlignmentOptions.Center;
            _promptText.sortingOrder = 100;

            var meshRenderer = _promptObj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.sortingOrder = 100;

            _promptObj.SetActive(false);
        }

        // ===============================================
        //  프레임 갱신
        // ===============================================

        private void Update()
        {
            FindPlayer();
            CheckProximity();
            HandleInput();
            AnimatePrompt();
            AnimateIcon();
        }

        private void FindPlayer()
        {
            if (_player != null) return;
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
        }

        private void CheckProximity()
        {
            if (_player == null)
            {
                SetPromptVisible(false);
                return;
            }

            float dist = Vector2.Distance(transform.position, _player.position);
            bool wasNear = _isPlayerNear;
            _isPlayerNear = dist <= _interactRange;

            bool showPrompt = _isPlayerNear && !_dialogueActive;
            SetPromptVisible(showPrompt);
        }

        private void HandleInput()
        {
            if (!_isPlayerNear || _dialogueActive) return;

            // DialogueSystem이 이미 활성 상태면 스킵
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive) return;

            // UnityEngine.Input.GetKeyDown 사용 (기존 Hub 패턴과 동일하되 레거시 입력)
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                ShowNote();
            }
        }

        private void ShowNote()
        {
            _dialogueActive = true;

            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = _title, text = _content }
            };

            DialogueSystem.Instance?.EnsureUI();
            DialogueSystem.Instance?.StartDialogue(lines);

            // 대화 종료 감지
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.OnDialogueComplete -= OnDialogueEnd;
                DialogueSystem.Instance.OnDialogueComplete += OnDialogueEnd;
            }

            Debug.Log($"[EnvironmentNote] 메모 표시: \"{_title}\"");
        }

        private void OnDialogueEnd()
        {
            _dialogueActive = false;
            if (DialogueSystem.Instance != null)
                DialogueSystem.Instance.OnDialogueComplete -= OnDialogueEnd;
        }

        // ===============================================
        //  비주얼 애니메이션
        // ===============================================

        private void SetPromptVisible(bool visible)
        {
            if (_promptObj != null && _promptObj.activeSelf != visible)
                _promptObj.SetActive(visible);
        }

        /// <summary>
        /// 프롬프트 텍스트 페이드 펄스 (HubManager와 동일 패턴)
        /// </summary>
        private void AnimatePrompt()
        {
            if (_promptObj == null || !_promptObj.activeSelf) return;

            _promptPulse += Time.deltaTime * 2f;
            float alpha = 0.6f + Mathf.Sin(_promptPulse) * 0.3f;
            _promptText.color = new Color(1f, 0.9f, 0.5f, alpha);
        }

        /// <summary>
        /// 아이콘 약간 위아래 부유 (존재감 표시)
        /// </summary>
        private void AnimateIcon()
        {
            float bob = Mathf.Sin(Time.time * 1.5f) * 0.05f;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // SpriteRenderer 색상으로 미세한 발광 표현
                float glow = 0.85f + Mathf.Sin(Time.time * 2f) * 0.15f;
                sr.color = new Color(glow, glow, glow, 1f);
            }
        }

        private void OnDestroy()
        {
            if (DialogueSystem.Instance != null)
                DialogueSystem.Instance.OnDialogueComplete -= OnDialogueEnd;
        }

        // ===============================================
        //  스테이지별 자동 배치
        // ===============================================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoPlaceNotes()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            switch (sceneName)
            {
                case "Stage1_1":
                    PlaceNote(25f, "경고문",
                        "벽에 긁힌 글씨: '여기서부터는 그림자 기사단의 영역이다. 돌아가라.'");
                    break;

                case "Stage1_2":
                    PlaceNote(40f, "실험 노트",
                        "에테르 수정이 박힌 실험 노트: '제17차 변이 실험 — 고블린 개체에 에테르 주입. 지능 상승 확인.'");
                    break;

                case "Stage1_3_Boss":
                    PlaceNote(5f, "마법진",
                        "거대한 마법진. 중앙에 '풍차인가, 거인인가'라는 글씨가 빛나고 있다.");
                    break;

                case "Stage2_1":
                    PlaceNote(20f, "훈련 기록",
                        "기사단 훈련 기록: '그림자 보행술 — 적의 눈을 속이는 것이 아닌, 자신의 존재를 지우는 것.'");
                    break;

                case "Stage2_2":
                    PlaceNote(35f, "깨진 거울",
                        "깨진 거울 조각. 표면에 카이의 모습이 비친다. 하지만 어딘가 다르다.");
                    break;

                case "Stage3_1":
                    PlaceNote(15f, "리나의 메모",
                        "리나의 메모: '카이에게 — 이건 용병 일이 아니야. 하지만 네가 간다면, 난 여기서 기다릴게.'");
                    break;

                case "Stage3_2":
                    PlaceNote(30f, "기사도 서약",
                        "벽에 새겨진 옛 기사도 서약: '약한 자를 지키고, 진실을 말하며, 쓰러져도 일어선다.'");
                    break;
            }
        }

        /// <summary>
        /// 지정 X 좌표에 환경 메모 오브젝트를 생성.
        /// Y는 바닥 위 약간 높은 위치(0.5f).
        /// </summary>
        private static void PlaceNote(float x, string title, string content)
        {
            var obj = new GameObject($"EnvironmentNote_{title}");
            obj.transform.position = new Vector3(x, 0.5f, 0f);

            var note = obj.AddComponent<EnvironmentNote>();

            // SerializeField에 값 설정 (리플렉션)
            var titleField = typeof(EnvironmentNote).GetField("_title",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            titleField?.SetValue(note, title);

            var contentField = typeof(EnvironmentNote).GetField("_content",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            contentField?.SetValue(note, content);

            Debug.Log($"[EnvironmentNote] 배치 완료: \"{title}\" at x={x}");
        }
    }
}
