using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ProjectKai.Data;
using System;
using System.Collections;

namespace ProjectKai.UI
{
    /// <summary>
    /// 대화 시스템. TextMeshPro 기반.
    /// Hades식 짧고 임팩트 있는 대화.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _continuePrompt;

        [Header("Settings")]
        [SerializeField] private float _typeSpeed = 0.03f;

        private DialogueDataSO _currentDialogue;
        private int _currentLine;
        private bool _isTyping;
        private bool _isActive;
        private Coroutine _typeCoroutine;

        public bool IsActive => _isActive;
        public event Action OnDialogueComplete;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(false);
        }

        /// <summary>
        /// 대화 시작. Time.timeScale = 0으로 게임 정지.
        /// </summary>
        public void StartDialogue(DialogueDataSO dialogue)
        {
            if (dialogue == null || dialogue.lines.Length == 0) return;

            _currentDialogue = dialogue;
            _currentLine = 0;
            _isActive = true;
            Time.timeScale = 0f;

            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(true);

            ShowLine(_currentDialogue.lines[0]);
        }

        /// <summary>
        /// 런타임에 대화 데이터를 직접 생성하여 시작
        /// </summary>
        public void StartDialogue(DialogueLine[] lines)
        {
            if (lines == null || lines.Length == 0) return;

            var data = ScriptableObject.CreateInstance<DialogueDataSO>();
            data.lines = lines;
            StartDialogue(data);
        }

        private void ShowLine(DialogueLine line)
        {
            if (_nameText != null)
                _nameText.text = line.speakerName;

            if (_portraitImage != null && line.portrait != null)
            {
                _portraitImage.sprite = line.portrait;
                _portraitImage.gameObject.SetActive(true);
            }
            else if (_portraitImage != null)
            {
                _portraitImage.gameObject.SetActive(false);
            }

            if (_continuePrompt != null)
                _continuePrompt.gameObject.SetActive(false);

            if (_typeCoroutine != null)
                StopCoroutine(_typeCoroutine);
            _typeCoroutine = StartCoroutine(TypeText(line.text));
        }

        private IEnumerator TypeText(string fullText)
        {
            _isTyping = true;
            _dialogueText.text = "";

            foreach (char c in fullText)
            {
                _dialogueText.text += c;
                yield return new WaitForSecondsRealtime(_typeSpeed);
            }

            _isTyping = false;
            if (_continuePrompt != null)
                _continuePrompt.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_isActive) return;

            // 아무 키 입력 감지 (unscaledTime 기반)
            bool anyInput = UnityEngine.InputSystem.Keyboard.current?.anyKey.wasPressedThisFrame == true
                || UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame == true;

            if (anyInput)
            {
                if (_isTyping)
                {
                    // 타이핑 중 → 전체 표시
                    if (_typeCoroutine != null)
                        StopCoroutine(_typeCoroutine);
                    _dialogueText.text = _currentDialogue.lines[_currentLine].text;
                    _isTyping = false;
                    if (_continuePrompt != null)
                        _continuePrompt.gameObject.SetActive(true);
                }
                else
                {
                    // 다음 대사
                    _currentLine++;
                    if (_currentLine >= _currentDialogue.lines.Length)
                    {
                        EndDialogue();
                    }
                    else
                    {
                        ShowLine(_currentDialogue.lines[_currentLine]);
                    }
                }
            }
        }

        private void EndDialogue()
        {
            _isActive = false;
            Time.timeScale = 1f;

            if (_dialoguePanel != null)
                _dialoguePanel.SetActive(false);

            OnDialogueComplete?.Invoke();
        }

        /// <summary>
        /// UI가 없을 때 런타임에 자동 생성
        /// </summary>
        public void EnsureUI()
        {
            if (_dialoguePanel != null) return;

            // Canvas 생성
            var canvasObj = new GameObject("DialogueCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 대화 패널 (하단 1/4)
            _dialoguePanel = new GameObject("DialoguePanel");
            _dialoguePanel.transform.SetParent(canvasObj.transform, false);
            var panelRect = _dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0.25f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImg = _dialoguePanel.AddComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0.85f);

            // 이름 텍스트
            var nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(_dialoguePanel.transform, false);
            _nameText = nameObj.AddComponent<TextMeshProUGUI>();
            _nameText.fontSize = 24;
            _nameText.color = new Color(0.9f, 0.7f, 0.3f);
            _nameText.fontStyle = FontStyles.Bold;
            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.7f);
            nameRect.anchorMax = new Vector2(0.5f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // 대사 텍스트
            var textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(_dialoguePanel.transform, false);
            _dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            _dialogueText.fontSize = 20;
            _dialogueText.color = Color.white;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.1f);
            textRect.anchorMax = new Vector2(0.95f, 0.65f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // 계속 프롬프트
            var promptObj = new GameObject("ContinuePrompt");
            promptObj.transform.SetParent(_dialoguePanel.transform, false);
            _continuePrompt = promptObj.AddComponent<TextMeshProUGUI>();
            _continuePrompt.text = "▼";
            _continuePrompt.fontSize = 18;
            _continuePrompt.color = new Color(1, 1, 1, 0.6f);
            _continuePrompt.alignment = TextAlignmentOptions.Right;
            var promptRect = promptObj.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.85f, 0.05f);
            promptRect.anchorMax = new Vector2(0.95f, 0.2f);
            promptRect.offsetMin = Vector2.zero;
            promptRect.offsetMax = Vector2.zero;

            _dialoguePanel.SetActive(false);
            DontDestroyOnLoad(canvasObj);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("DialogueSystem");
                var ds = obj.AddComponent<DialogueSystem>();
                ds.EnsureUI();
                DontDestroyOnLoad(obj);
            }
        }
    }
}
