using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 세이브/로드. PlayerPrefs 기반.
    /// ProgressionSystem + GameState + WeaponUpgrade 전부 저장.
    /// </summary>
    public static class SaveSystem
    {
        public static void Save()
        {
            var prog = ProgressionSystem.Instance;
            var state = GameState.Instance;

            if (prog != null)
            {
                PlayerPrefs.SetInt("Level", prog.Level);
                PlayerPrefs.SetInt("Exp", prog.Experience);
                PlayerPrefs.SetInt("StatPoints", prog.StatPoints);
                PlayerPrefs.SetInt("STR", prog.STR);
                PlayerPrefs.SetInt("DEX", prog.DEX);
                PlayerPrefs.SetInt("INT", prog.INT);
                PlayerPrefs.SetInt("Gold", prog.Gold);
            }

            if (state != null)
            {
                PlayerPrefs.SetInt("Chapter", state.CurrentChapter);
                PlayerPrefs.SetInt("Ch2Unlocked", state.Chapter2Unlocked ? 1 : 0);
                PlayerPrefs.SetInt("Ch3Unlocked", state.Chapter3Unlocked ? 1 : 0);
                PlayerPrefs.SetInt("GameCleared", state.GameCleared ? 1 : 0);
            }

            PlayerPrefs.SetInt("SwordLevel", WeaponUpgrade.SwordLevel);
            PlayerPrefs.SetInt("GunLevel", WeaponUpgrade.GunLevel);

            PlayerPrefs.SetInt("HasSave", 1);
            PlayerPrefs.Save();

            Debug.Log("[SaveSystem] 저장 완료");
        }

        public static void Load()
        {
            if (!HasSave()) return;

            var prog = ProgressionSystem.Instance;
            var state = GameState.Instance;

            if (prog != null)
            {
                // ProgressionSystem의 private 필드는 직접 접근 불가
                // Reflection 또는 public 메서드로 처리
                int savedGold = PlayerPrefs.GetInt("Gold", 0);
                if (savedGold > prog.Gold)
                    prog.AddGold(savedGold - prog.Gold);
            }

            if (state != null)
            {
                state.CurrentChapter = PlayerPrefs.GetInt("Chapter", 1);
                state.Chapter2Unlocked = PlayerPrefs.GetInt("Ch2Unlocked", 0) == 1;
                state.Chapter3Unlocked = PlayerPrefs.GetInt("Ch3Unlocked", 0) == 1;
                state.GameCleared = PlayerPrefs.GetInt("GameCleared", 0) == 1;
            }

            Debug.Log("[SaveSystem] 로드 완료");
        }

        public static bool HasSave()
        {
            return PlayerPrefs.GetInt("HasSave", 0) == 1;
        }

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[SaveSystem] 세이브 삭제");
        }
    }
}
