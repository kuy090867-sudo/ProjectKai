using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 게임 전체 상태 관리. 챕터 진행, 언락 상태.
    /// 싱글톤, DontDestroyOnLoad.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public int CurrentChapter { get; set; } = 1;
        public bool Chapter2Unlocked { get; set; }
        public bool Chapter3Unlocked { get; set; }
        public bool GameCleared { get; set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void UnlockChapter(int chapter)
        {
            switch (chapter)
            {
                case 2: Chapter2Unlocked = true; break;
                case 3: Chapter3Unlocked = true; break;
            }
            Debug.Log($"[GameState] {chapter}장 해금!");
        }

        public void CompleteGame()
        {
            GameCleared = true;
            Debug.Log("[GameState] 게임 클리어!");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("GameState");
                obj.AddComponent<GameState>();
            }
        }
    }
}
