using UnityEngine;
using System;

namespace ProjectKai.Core
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private float _maxHealth = 100f;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => _maxHealth;
        public bool IsAlive => CurrentHealth > 0f;
        public float HealthPercent => CurrentHealth / _maxHealth;

        public event Action<float, float> OnHealthChanged;
        public event Action OnDeath;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (!IsAlive)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        public void SetMaxHealth(float newMax, bool healToFull = false)
        {
            _maxHealth = newMax;
            if (healToFull || CurrentHealth > _maxHealth)
            {
                CurrentHealth = _maxHealth;
            }
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }
    }
}
