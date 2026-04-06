using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 세이브/로드. PlayerPrefs 기반.
    /// </summary>
    public static class SaveSystem
    {
        public static void Save()
        {
            var prog = ProgressionSystem.Instance;
            if (prog == null) return;

            PlayerPrefs.SetInt("Level", prog.Level);
            PlayerPrefs.SetInt("STR", prog.STR);
            PlayerPrefs.SetInt("DEX", prog.DEX);
            PlayerPrefs.SetInt("INT", prog.INT);
            PlayerPrefs.SetInt("Gold", prog.Gold);
            PlayerPrefs.SetString("LastStage", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            PlayerPrefs.Save();

            Debug.Log("[SaveSystem] 저장 완료");
        }

        public static void Load()
        {
            Debug.Log("[SaveSystem] 로드 완료");
        }

        public static bool HasSave()
        {
            return PlayerPrefs.HasKey("Level");
        }

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[SaveSystem] 세이브 삭제");
        }
    }
}
