using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Enemy;

namespace ProjectKai.Core
{
    /// <summary>
    /// 난이도 스케일링. 챕터/레벨에 따라 적 스탯 자동 조정.
    /// GameSetup에서 호출.
    /// </summary>
    public static class DifficultyScaler
    {
        public static void ScaleEnemies()
        {
            int chapter = GameState.Instance?.CurrentChapter ?? 1;
            int playerLevel = ProgressionSystem.Instance?.Level ?? 1;

            float hpMultiplier = 1f + (chapter - 1) * 0.3f + (playerLevel - 1) * 0.05f;
            float dmgMultiplier = 1f + (chapter - 1) * 0.2f;

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in enemies)
            {
                var dr = e.GetComponent<DamageReceiver>();
                if (dr != null)
                {
                    // DamageReceiver의 maxHealth는 private이므로 직접 수정 불가
                    // 대신 체력을 채워주는 방식으로 간접 스케일링
                    float bonusHP = (hpMultiplier - 1f) * dr.MaxHealth;
                    if (bonusHP > 0f)
                        dr.Heal(bonusHP);
                }
            }

            if (hpMultiplier > 1f)
                Debug.Log($"[Difficulty] 챕터 {chapter}, 레벨 {playerLevel}: HP x{hpMultiplier:F1}, DMG x{dmgMultiplier:F1}");
        }
    }
}
