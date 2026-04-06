using UnityEngine;
using System;

namespace ProjectKai.Core
{
    /// <summary>
    /// 성장 시스템: 경험치, 레벨업, 스탯 포인트.
    /// 싱글톤, DontDestroyOnLoad.
    /// </summary>
    public class ProgressionSystem : MonoBehaviour
    {
        public static ProgressionSystem Instance { get; private set; }

        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int StatPoints { get; private set; }
        public int Gold { get; private set; }

        // 스탯
        public int STR { get; private set; } = 5;
        public int DEX { get; private set; } = 5;
        public int INT { get; private set; } = 5;

        // 파생 스탯
        public float BonusDamage => STR * 2f;
        public float BonusSpeed => DEX * 0.1f;
        public float BonusHealth => STR * 5f;

        public event Action<int> OnLevelUp;
        public event Action<int, int> OnExpChanged;

        private int ExpToNextLevel => Level * 100;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddExperience(int amount)
        {
            Experience += amount;
            OnExpChanged?.Invoke(Experience, ExpToNextLevel);

            while (Experience >= ExpToNextLevel)
            {
                Experience -= ExpToNextLevel;
                Level++;
                StatPoints += 5;
                OnLevelUp?.Invoke(Level);
                Debug.Log($"[Progression] 레벨업! Lv.{Level}, 스탯포인트: {StatPoints}");
            }
        }

        public void AddGold(int amount)
        {
            Gold += amount;
        }

        public bool AllocateStat(string stat)
        {
            if (StatPoints <= 0) return false;

            switch (stat.ToUpper())
            {
                case "STR": STR++; break;
                case "DEX": DEX++; break;
                case "INT": INT++; break;
                default: return false;
            }
            StatPoints--;
            return true;
        }

        /// <summary>
        /// 세이브 데이터 로드용. SaveSystem에서 호출.
        /// </summary>
        public void LoadData(int level, int exp, int statPoints, int gold, int str, int dex, int intStat)
        {
            Level = level;
            Experience = exp;
            StatPoints = statPoints;
            Gold = gold;
            STR = str;
            DEX = dex;
            INT = intStat;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("ProgressionSystem");
                obj.AddComponent<ProgressionSystem>();
            }
        }
    }
}
