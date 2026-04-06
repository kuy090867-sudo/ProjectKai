using UnityEngine;
using ProjectKai.Player;

namespace ProjectKai.Core
{
    /// <summary>
    /// 인벤토리 시스템. 포션 관리.
    /// 싱글톤, DontDestroyOnLoad.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        private const int MaxPotions = 5;
        private int _potionCount = 3; // 초기 3개 지급

        /// <summary>현재 포션 수</summary>
        public int PotionCount => _potionCount;

        /// <summary>최대 포션 수</summary>
        public int PotionMax => MaxPotions;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 포션 사용. HP 50 회복. 성공 시 true 반환.
        /// </summary>
        public bool UsePotion()
        {
            if (_potionCount <= 0)
            {
                Debug.Log("[Inventory] 포션 없음");
                return false;
            }

            var player = GameObject.FindWithTag("Player");
            if (player == null) return false;

            var pc = player.GetComponent<PlayerController>();
            if (pc == null || !pc.IsAlive) return false;

            // 이미 최대 체력이면 사용 안 함
            if (pc.CurrentHealth >= pc.MaxHealth)
            {
                Debug.Log("[Inventory] 이미 최대 체력");
                return false;
            }

            _potionCount--;
            pc.Heal(50f);
            AudioManager.Instance?.PlaySFX("jump", 0.6f);
            Debug.Log($"[Inventory] 포션 사용! 남은 포션: {_potionCount}/{MaxPotions}");
            return true;
        }

        /// <summary>
        /// 포션 추가. 최대 5개 제한.
        /// </summary>
        public void AddPotion(int count)
        {
            _potionCount = Mathf.Clamp(_potionCount + count, 0, MaxPotions);
            Debug.Log($"[Inventory] 포션 추가: {count}개 -> 현재 {_potionCount}/{MaxPotions}");
        }

        /// <summary>세이브 데이터 로드용</summary>
        public void LoadData(int potionCount)
        {
            _potionCount = Mathf.Clamp(potionCount, 0, MaxPotions);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("InventorySystem");
                obj.AddComponent<InventorySystem>();
            }
        }
    }
}
