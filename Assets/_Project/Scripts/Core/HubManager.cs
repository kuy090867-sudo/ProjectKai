using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    /// <summary>
    /// Hub 씬 전용 매니저.
    /// - 리나 NPC 자동 배치 (elf_f 스프라이트)
    /// - 상호작용 프롬프트 ("E: 대화")
    /// - E키로 대화 시작 (DialogueSystem 연동)
    /// - 리나 랜덤 대사 (한국어)
    /// - NPC SpriteAnimator 자동 설정
    /// - Hub 비주얼 보강 (조명, 장식 오브젝트)
    /// </summary>
    public class HubManager : MonoBehaviour
    {
        // ═══════════════════════════════════════
        //  리나 대사 (14개 + Hub 분위기)
        // ═══════════════════════════════════════
        private static readonly string[][] _rinaDialogues = new string[][]
        {
            new[] { "리나", "카이", "넌 왜 맨날 손해 보는 일만 하는 거야?", "...누군가는 해야 하니까." },
            new[] { "리나", "카이", "보수 없는 의뢰는 안 받는다고 했잖아.", "...라고 말하고 싶지만." },
            new[] { "리나", "카이", "다음 의뢰는 좀 쉬운 거 없나?", "쉬운 일에 우리가 왜 필요해." },
            new[] { "리나", "카이", "그 기사 이야기 책, 아직도 들고 다니는 거야?", "좋은 책이야." },
            new[] { "리나", "카이", "슬럼 아이들이 네 이름을 알더라. '라만차의 기사'라고.", "...과분한 이름이야." },
            new[] { "리나", "카이", "돈키호테 선생님, 오늘은 어느 풍차와 싸울 거야?", "풍차가 아니길 바라지." },
            new[] { "리나", "카이", "스승님이 보시면 뭐라고 하실까.", "잘하고 있다고... 하시겠지." },
            new[] { "리나", "카이", "에테르 가격이 또 올랐어. 이 세상은 정말...", "그래도 포기하면 안 돼." },
            new[] { "리나", "카이", "가끔은 네가 부러워. 믿는 게 있으니까.", "너도 믿는 게 있잖아." },
            new[] { "리나", "카이", "조심해. 다음번엔 내가 못 구할 수도 있어.", "알았어. 고마워, 리나." },
            new[] { "리나", "카이", "그림자 기사단... 우리가 건드릴 수준이 아닌데.", "수준 밖의 일이 우리 일이야." },
            new[] { "리나", "카이", "내 부모님도 이 일과 관련이 있을지도 몰라.", "...같이 알아보자." },
            new[] { "리나", "카이", "풍차든 거인이든, 네 옆에는 내가 있으니까.", "...고맙다." },
            new[] { "리나", "카이", "오늘도 살아서 돌아와. 그게 최우선이야.", "반드시." },
            new[] { "리나", "카이", "카이, 밥은 먹었어? 기사도로 배 채울 순 없잖아.", "...먹었어. 아마." },
            new[] { "리나", "카이", "이 사무실, 좀 치우면 안 돼? 나 여기서 일한다고.", "정리는 내일..." },
        };

        // NPC 참조
        private GameObject _linaNpc;
        private SpriteAnimator _linaAnimator;
        private GameObject _interactionPrompt;
        private TextMeshPro _promptText;
        private bool _isPlayerNear;
        private bool _dialogueActive;
        private float _interactionRange = 2.0f;

        // 대장장이 NPC 참조
        private GameObject _blacksmithNpc;
        private SpriteAnimator _blacksmithAnimator;
        private GameObject _blacksmithPrompt;
        private TextMeshPro _blacksmithPromptText;
        private bool _isPlayerNearBlacksmith;
        private float _blacksmithRange = 2.0f;

        // Hub 장식
        private GameObject _hubDecorations;

        // ═══════════════════════════════════════
        //  자동 초기화
        // ═══════════════════════════════════════

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Hub") return;

            if (Object.FindFirstObjectByType<HubManager>() == null)
            {
                var obj = new GameObject("HubManager");
                obj.AddComponent<HubManager>();
                Debug.Log("[HubManager] Hub 씬 감지 -> HubManager 자동 생성");
            }
        }

        // ═══════════════════════════════════════
        //  초기화
        // ═══════════════════════════════════════

        private void Awake()
        {
            CreateLinaNpc();
            CreateBlacksmithNpc();
            CreateHubDecorations();
        }

        private void Start()
        {
            SetupLinaAnimator();
            SetupBlacksmithAnimator();
            CreateHubLighting();
            StageProgressUI.Show();
        }

        // ═══════════════════════════════════════
        //  리나 NPC 생성
        // ═══════════════════════════════════════

        private void CreateLinaNpc()
        {
            // 기존 리나가 있으면 스킵
            var existing = GameObject.Find("NPC_Lina");
            if (existing != null)
            {
                _linaNpc = existing;
                return;
            }

            _linaNpc = new GameObject("NPC_Lina");
            _linaNpc.tag = "Untagged"; // NPC 전용 태그가 없으면 Untagged

            // 플레이어 위치 기반으로 배치 (플레이어 오른쪽 약간 앞)
            var player = GameObject.FindWithTag("Player");
            Vector3 npcPos;
            if (player != null)
            {
                npcPos = player.transform.position + new Vector3(3f, 0f, 0f);
            }
            else
            {
                npcPos = new Vector3(3f, -1f, 0f);
            }
            _linaNpc.transform.position = npcPos;

            // SpriteRenderer
            var sr = _linaNpc.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.color = Color.white;

            // 스프라이트 로드 (elf_f)
            var tex = Resources.Load<Texture2D>("Sprites/NPC/elf_f_idle_anim_f0");
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                sr.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 16f);
            }
            else
            {
                // 폴백: 플레이스홀더
                sr.sprite = CreateNpcPlaceholder();
                Debug.LogWarning("[HubManager] elf_f 스프라이트 로드 실패. 플레이스홀더 사용.");
            }

            // BoxCollider2D (상호작용 범위)
            var col = _linaNpc.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(_interactionRange, _interactionRange);

            // 상호작용 프롬프트 (WorldSpace TMP)
            CreateInteractionPrompt();

            Debug.Log("[HubManager] 리나 NPC 생성 완료");
        }

        private void CreateInteractionPrompt()
        {
            _interactionPrompt = new GameObject("InteractionPrompt");
            _interactionPrompt.transform.SetParent(_linaNpc.transform);
            _interactionPrompt.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            _promptText = _interactionPrompt.AddComponent<TextMeshPro>();
            _promptText.text = "E: 대화";
            _promptText.fontSize = 4f;
            _promptText.color = new Color(1f, 0.9f, 0.5f, 0.9f);
            _promptText.alignment = TextAlignmentOptions.Center;
            _promptText.sortingOrder = 100;

            // MeshRenderer sortingOrder 설정
            var meshRenderer = _interactionPrompt.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 100;
            }

            _interactionPrompt.SetActive(false);
        }

        private Sprite CreateNpcPlaceholder()
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            // 연보라색 실루엣
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool inBody = x >= 4 && x < 12 && y >= 0 && y < 12;
                    bool inHead = x >= 5 && x < 11 && y >= 10 && y < 16;
                    pixels[y * 16 + x] = (inBody || inHead)
                        ? new Color(0.7f, 0.5f, 0.9f, 1f)
                        : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }

        // ═══════════════════════════════════════
        //  대장장이 NPC 생성
        // ═══════════════════════════════════════

        private void CreateBlacksmithNpc()
        {
            var existing = GameObject.Find("NPC_Blacksmith");
            if (existing != null)
            {
                _blacksmithNpc = existing;
                return;
            }

            _blacksmithNpc = new GameObject("NPC_Blacksmith");

            // 리나 반대편에 배치 (플레이어 왼쪽)
            var player = GameObject.FindWithTag("Player");
            Vector3 npcPos;
            if (player != null)
            {
                npcPos = player.transform.position + new Vector3(-3.5f, 0f, 0f);
            }
            else
            {
                npcPos = new Vector3(-3.5f, -1f, 0f);
            }
            _blacksmithNpc.transform.position = npcPos;

            // SpriteRenderer
            var sr = _blacksmithNpc.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.color = Color.white;

            // 스프라이트 로드 (dwarf_m)
            var tex = Resources.Load<Texture2D>("Sprites/NPC/dwarf_m_idle_anim_f0");
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                sr.sprite = Sprite.Create(tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 16f);
            }
            else
            {
                sr.sprite = CreateBlacksmithPlaceholder();
                Debug.LogWarning("[HubManager] dwarf_m 스프라이트 로드 실패. 플레이스홀더 사용.");
            }

            // BoxCollider2D
            var col = _blacksmithNpc.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(_blacksmithRange, _blacksmithRange);

            // 상호작용 프롬프트
            CreateBlacksmithPrompt();

            Debug.Log("[HubManager] 대장장이 NPC 생성 완료");
        }

        private void CreateBlacksmithPrompt()
        {
            _blacksmithPrompt = new GameObject("BlacksmithPrompt");
            _blacksmithPrompt.transform.SetParent(_blacksmithNpc.transform);
            _blacksmithPrompt.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            _blacksmithPromptText = _blacksmithPrompt.AddComponent<TextMeshPro>();
            _blacksmithPromptText.text = "E: 무기 강화";
            _blacksmithPromptText.fontSize = 4f;
            _blacksmithPromptText.color = new Color(1f, 0.7f, 0.3f, 0.9f);
            _blacksmithPromptText.alignment = TextAlignmentOptions.Center;
            _blacksmithPromptText.sortingOrder = 100;

            var meshRenderer = _blacksmithPrompt.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.sortingOrder = 100;

            _blacksmithPrompt.SetActive(false);
        }

        private Sprite CreateBlacksmithPlaceholder()
        {
            var tex = new Texture2D(16, 16);
            var pixels = new Color[256];
            // 갈색/주황 대장장이 실루엣
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool inBody = x >= 3 && x < 13 && y >= 0 && y < 10;
                    bool inHead = x >= 5 && x < 11 && y >= 10 && y < 16;
                    pixels[y * 16 + x] = (inBody || inHead)
                        ? new Color(0.7f, 0.45f, 0.2f, 1f)
                        : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }

        private void SetupBlacksmithAnimator()
        {
            if (_blacksmithNpc == null) return;

            _blacksmithAnimator = _blacksmithNpc.GetComponent<SpriteAnimator>();
            if (_blacksmithAnimator == null)
                _blacksmithAnimator = _blacksmithNpc.AddComponent<SpriteAnimator>();

            _blacksmithAnimator.ConfigureEnemy("Sprites/NPC", "dwarf_m");
        }

        // ═══════════════════════════════════════
        //  SpriteAnimator 설정
        // ═══════════════════════════════════════

        private void SetupLinaAnimator()
        {
            if (_linaNpc == null) return;

            _linaAnimator = _linaNpc.GetComponent<SpriteAnimator>();
            if (_linaAnimator == null)
            {
                _linaAnimator = _linaNpc.AddComponent<SpriteAnimator>();
            }

            // NPC용 설정: elf_f 스프라이트, idle 애니메이션
            // ConfigureEnemy를 활용하되 NPC 폴더 지정
            _linaAnimator.ConfigureEnemy("Sprites/NPC", "elf_f");
        }

        // ═══════════════════════════════════════
        //  상호작용 로직
        // ═══════════════════════════════════════

        private void Update()
        {
            CheckPlayerProximity();
            HandleInteraction();
            AnimatePrompt();
        }

        private void CheckPlayerProximity()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                _isPlayerNear = false;
                _isPlayerNearBlacksmith = false;
                if (_interactionPrompt != null)
                    _interactionPrompt.SetActive(false);
                if (_blacksmithPrompt != null)
                    _blacksmithPrompt.SetActive(false);
                return;
            }

            // 리나 근접 확인
            if (_linaNpc != null)
            {
                float distLina = Vector2.Distance(
                    player.transform.position,
                    _linaNpc.transform.position);

                _isPlayerNear = distLina <= _interactionRange;

                if (_interactionPrompt != null)
                {
                    bool showPrompt = _isPlayerNear && !_dialogueActive;
                    _interactionPrompt.SetActive(showPrompt);
                }

                if (_isPlayerNear)
                {
                    var sr = _linaNpc.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.flipX = player.transform.position.x < _linaNpc.transform.position.x;
                }
            }

            // 대장장이 근접 확인
            if (_blacksmithNpc != null)
            {
                float distSmith = Vector2.Distance(
                    player.transform.position,
                    _blacksmithNpc.transform.position);

                _isPlayerNearBlacksmith = distSmith <= _blacksmithRange;

                if (_blacksmithPrompt != null)
                {
                    // WeaponUpgradeUI가 열려있으면 프롬프트 숨김
                    bool upgradeOpen = WeaponUpgradeUI.Instance != null &&
                                       WeaponUpgradeUI.Instance.gameObject.GetComponentInChildren<Canvas>() != null &&
                                       WeaponUpgradeUI.Instance.transform.GetChild(0).gameObject.activeSelf;
                    bool showPrompt = _isPlayerNearBlacksmith && !upgradeOpen;
                    _blacksmithPrompt.SetActive(showPrompt);
                }

                if (_isPlayerNearBlacksmith)
                {
                    var sr = _blacksmithNpc.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.flipX = player.transform.position.x < _blacksmithNpc.transform.position.x;
                }
            }
        }

        private void HandleInteraction()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.eKey.wasPressedThisFrame) return;

            // 대장장이 상호작용 우선 (WeaponUpgradeUI가 닫혀있을 때)
            if (_isPlayerNearBlacksmith && !_dialogueActive)
            {
                WeaponUpgradeUI.Show();
                Debug.Log("[HubManager] 대장장이 상호작용 -> 무기 강화 UI 표시");
                return;
            }

            // 리나 상호작용
            if (_isPlayerNear && !_dialogueActive)
            {
                if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive) return;
                StartLinaDialogue();
            }
        }

        private void StartLinaDialogue()
        {
            _dialogueActive = true;

            int idx = Random.Range(0, _rinaDialogues.Length);
            string[] data = _rinaDialogues[idx];

            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = data[0], text = data[2] },
                new DialogueLine { speakerName = data[1], text = data[3] },
            };

            DialogueSystem.Instance?.EnsureUI();
            DialogueSystem.Instance?.StartDialogue(lines);

            // 대화 종료 감지
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.OnDialogueComplete -= OnDialogueEnd;
                DialogueSystem.Instance.OnDialogueComplete += OnDialogueEnd;
            }

            Debug.Log($"[HubManager] 리나 대화 시작: \"{data[2]}\"");
        }

        private void OnDialogueEnd()
        {
            _dialogueActive = false;
            if (DialogueSystem.Instance != null)
                DialogueSystem.Instance.OnDialogueComplete -= OnDialogueEnd;
        }

        // 프롬프트 텍스트 페이드 애니메이션
        private float _promptPulse;

        private void AnimatePrompt()
        {
            _promptPulse += Time.deltaTime * 2f;
            float alpha = 0.6f + Mathf.Sin(_promptPulse) * 0.3f;

            if (_interactionPrompt != null && _interactionPrompt.activeSelf && _promptText != null)
                _promptText.color = new Color(1f, 0.9f, 0.5f, alpha);

            if (_blacksmithPrompt != null && _blacksmithPrompt.activeSelf && _blacksmithPromptText != null)
                _blacksmithPromptText.color = new Color(1f, 0.7f, 0.3f, alpha);
        }

        // ═══════════════════════════════════════
        //  Hub 비주얼 보강
        // ═══════════════════════════════════════

        private void CreateHubDecorations()
        {
            _hubDecorations = new GameObject("HubDecorations");
            _hubDecorations.transform.SetParent(transform);

            // 바닥 타일 배경
            CreateFloorTiles();

            // 장식 오브젝트들
            CreateDecoObject("Bookshelf", new Vector3(-4f, -0.5f, 0f),
                new Color(0.45f, 0.3f, 0.2f), 1.2f, 1.8f);
            CreateDecoObject("Desk", new Vector3(0f, -1.5f, 0f),
                new Color(0.5f, 0.35f, 0.2f), 2.5f, 0.8f);
            CreateDecoObject("Lamp", new Vector3(-1f, 0.8f, 0f),
                new Color(1f, 0.85f, 0.4f, 0.9f), 0.3f, 0.5f);
            CreateDecoObject("Window", new Vector3(5f, 1f, 0f),
                new Color(0.4f, 0.55f, 0.7f, 0.6f), 1.5f, 2f);

            // 포스터/문양 (의뢰 게시판)
            CreateBulletinBoard();
        }

        private void CreateFloorTiles()
        {
            var floor = new GameObject("HubFloor");
            floor.transform.SetParent(_hubDecorations.transform);

            // 넓은 바닥 스프라이트
            var sr = floor.AddComponent<SpriteRenderer>();
            var tex = new Texture2D(64, 16);
            var pixels = new Color[64 * 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    // 체크 무늬 나무 바닥
                    bool check = ((x / 8) + (y / 8)) % 2 == 0;
                    float noise = ((x * 31 + y * 17) % 100) / 100f * 0.05f;
                    Color c = check
                        ? new Color(0.35f + noise, 0.25f + noise, 0.15f + noise)
                        : new Color(0.30f + noise, 0.20f + noise, 0.12f + noise);
                    pixels[y * 64 + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 16), new Vector2(0.5f, 0.5f), 16f);
            sr.sortingOrder = -5;
            floor.transform.localPosition = new Vector3(0f, -2.2f, 0f);
        }

        private void CreateDecoObject(string objName, Vector3 pos, Color color, float w, float h)
        {
            var obj = new GameObject($"Deco_{objName}");
            obj.transform.SetParent(_hubDecorations.transform);
            obj.transform.localPosition = pos;

            var sr = obj.AddComponent<SpriteRenderer>();
            int texW = Mathf.Max(4, (int)(w * 8));
            int texH = Mathf.Max(4, (int)(h * 8));
            var tex = new Texture2D(texW, texH);
            var pixels = new Color[texW * texH];
            for (int y = 0; y < texH; y++)
            {
                for (int x = 0; x < texW; x++)
                {
                    float edge = 0f;
                    if (x == 0 || x == texW - 1 || y == 0 || y == texH - 1)
                        edge = -0.08f;
                    float noise = ((x * 53 + y * 97) % 100) / 100f * 0.04f;
                    pixels[y * texW + x] = new Color(
                        Mathf.Clamp01(color.r + noise + edge),
                        Mathf.Clamp01(color.g + noise + edge),
                        Mathf.Clamp01(color.b + noise + edge),
                        color.a);
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), 8f);
            sr.sortingOrder = -2;
        }

        private void CreateBulletinBoard()
        {
            var board = new GameObject("BulletinBoard");
            board.transform.SetParent(_hubDecorations.transform);
            board.transform.localPosition = new Vector3(-2f, 0.5f, 0f);

            var sr = board.AddComponent<SpriteRenderer>();
            int w = 16, h = 20;
            var tex = new Texture2D(w, h);
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool border = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                    bool paper1 = x >= 2 && x <= 6 && y >= 3 && y <= 8;
                    bool paper2 = x >= 8 && x <= 13 && y >= 5 && y <= 11;
                    bool paper3 = x >= 3 && x <= 10 && y >= 13 && y <= 17;

                    if (border)
                        pixels[y * w + x] = new Color(0.4f, 0.28f, 0.15f);
                    else if (paper1)
                        pixels[y * w + x] = new Color(0.9f, 0.85f, 0.7f);
                    else if (paper2)
                        pixels[y * w + x] = new Color(0.85f, 0.8f, 0.65f);
                    else if (paper3)
                        pixels[y * w + x] = new Color(0.88f, 0.82f, 0.68f);
                    else
                        pixels[y * w + x] = new Color(0.5f, 0.35f, 0.2f);
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
            sr.sortingOrder = -1;
        }

        // ═══════════════════════════════════════
        //  Hub 조명 효과
        // ═══════════════════════════════════════

        private void CreateHubLighting()
        {
            // 따뜻한 포인트 라이트 효과 (SpriteRenderer로 구현)
            CreateLightGlow("WarmLight_Main", new Vector3(0f, 1f, 0f),
                new Color(1f, 0.85f, 0.5f, 0.12f), 6f);
            CreateLightGlow("WarmLight_Lamp", new Vector3(-1f, 0.8f, 0f),
                new Color(1f, 0.9f, 0.4f, 0.2f), 3f);
            CreateLightGlow("WindowLight", new Vector3(5f, 0.5f, 0f),
                new Color(0.5f, 0.7f, 1f, 0.08f), 4f);

            // 먼지 파티클 (AmbientParticles가 이미 처리하지만, 추가 분위기)
            CreateHubDustMotes();
        }

        private void CreateLightGlow(string objName, Vector3 pos, Color color, float radius)
        {
            var obj = new GameObject($"Light_{objName}");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = pos;

            var sr = obj.AddComponent<SpriteRenderer>();
            int size = 32;
            var tex = new Texture2D(size, size);
            var pixels = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                    float intensity = Mathf.Clamp01(1f - dist * dist);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * intensity);
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();

            sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2f));
            sr.sortingOrder = 15;
            // Additive 블렌딩 효과를 위해 머티리얼 설정은 런타임에서 제한적이므로
            // 알파로 대체
        }

        private void CreateHubDustMotes()
        {
            var obj = new GameObject("HubDustMotes");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;

            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 25;
            main.startLifetime = 6f;
            main.startSpeed = 0.1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new Color(1f, 0.9f, 0.6f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.01f;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10f, 5f, 0f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.3f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
                renderer.sortingOrder = 12;
        }

        private void OnDestroy()
        {
            if (DialogueSystem.Instance != null)
                DialogueSystem.Instance.OnDialogueComplete -= OnDialogueEnd;
        }
    }
}
