using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    /// <summary>
    /// 적 처치 시 경험치+골드 보상.
    /// DamageReceiver의 OnDeath에 자동 연결.
    /// </summary>
    public class EnemyReward : MonoBehaviour
    {
        [SerializeField] private int _expReward = 30;
        [SerializeField] private int _goldReward = 10;

        private void Start()
        {
            var dr = GetComponent<DamageReceiver>();
            if (dr != null)
                dr.OnDeath += OnDeath;
        }

        private void OnDeath()
        {
            if (ProgressionSystem.Instance != null)
            {
                ProgressionSystem.Instance.AddExperience(_expReward);
                ProgressionSystem.Instance.AddGold(_goldReward);
                Debug.Log($"[Reward] +{_expReward} EXP, +{_goldReward} Gold");
            }
        }
    }
}
