using UnityEngine;
using System;

namespace ProjectKai.Core
{
    /// <summary>
    /// MP(마나) 시스템. 마법 스킬 사용 시 소모.
    /// 시간에 따라 자동 회복.
    /// </summary>
    public class ManaSystem : MonoBehaviour
    {
        public static ManaSystem Instance { get; private set; }

        [SerializeField] private float _maxMana = 50f;
        [SerializeField] private float _regenRate = 3f;
        [SerializeField] private float _regenDelay = 1.5f;

        public float CurrentMana { get; private set; }
        public float MaxMana => _maxMana;
        public float ManaPercent => CurrentMana / _maxMana;

        public event Action<float, float> OnManaChanged;

        private float _lastUseTime = -999f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentMana = _maxMana;
        }

        private void Update()
        {
            // INT 보너스 반영
            float intBonus = ProgressionSystem.Instance != null ? ProgressionSystem.Instance.INT * 2f : 0f;
            float actualMax = _maxMana + intBonus;

            // 자동 회복
            if (Time.time > _lastUseTime + _regenDelay && CurrentMana < actualMax)
            {
                float regenBonus = ProgressionSystem.Instance != null ? ProgressionSystem.Instance.INT * 0.2f : 0f;
                CurrentMana = Mathf.Min(CurrentMana + (_regenRate + regenBonus) * Time.deltaTime, actualMax);
                OnManaChanged?.Invoke(CurrentMana, actualMax);
            }
        }

        public float ActualMaxMana
        {
            get
            {
                float intBonus = ProgressionSystem.Instance != null ? ProgressionSystem.Instance.INT * 2f : 0f;
                return _maxMana + intBonus;
            }
        }

        public bool UseMana(float amount)
        {
            if (CurrentMana < amount) return false;

            CurrentMana -= amount;
            _lastUseTime = Time.time;
            OnManaChanged?.Invoke(CurrentMana, ActualMaxMana);
            return true;
        }

        public void RestoreMana(float amount)
        {
            CurrentMana = Mathf.Min(CurrentMana + amount, ActualMaxMana);
            OnManaChanged?.Invoke(CurrentMana, ActualMaxMana);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("ManaSystem");
                obj.AddComponent<ManaSystem>();
            }
        }
    }
}
