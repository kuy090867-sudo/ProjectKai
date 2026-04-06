using UnityEngine;
using ProjectKai.Player;

namespace ProjectKai.Core
{
    /// <summary>
    /// 무기 강화 시스템. 골드로 데미지/속도 업그레이드.
    /// Hub에서 NPC를 통해 접근.
    /// </summary>
    public static class WeaponUpgrade
    {
        public static int SwordLevel { get; private set; } = 1;
        public static int GunLevel { get; private set; } = 1;

        public static int SwordUpgradeCost => SwordLevel * 50;
        public static int GunUpgradeCost => GunLevel * 50;

        public static float SwordDamageBonus => (SwordLevel - 1) * 3f;
        public static float GunDamageBonus => (GunLevel - 1) * 2f;

        public static bool UpgradeSword()
        {
            var prog = ProgressionSystem.Instance;
            if (prog == null || prog.Gold < SwordUpgradeCost) return false;

            prog.AddGold(-SwordUpgradeCost);
            SwordLevel++;
            AudioManager.Instance?.PlaySFX("jump", 0.7f);
            Debug.Log($"[WeaponUpgrade] 검 Lv.{SwordLevel} (+{SwordDamageBonus} DMG)");
            return true;
        }

        public static bool UpgradeGun()
        {
            var prog = ProgressionSystem.Instance;
            if (prog == null || prog.Gold < GunUpgradeCost) return false;

            prog.AddGold(-GunUpgradeCost);
            GunLevel++;
            AudioManager.Instance?.PlaySFX("jump", 0.7f);
            Debug.Log($"[WeaponUpgrade] 총 Lv.{GunLevel} (+{GunDamageBonus} DMG)");
            return true;
        }
    }
}
