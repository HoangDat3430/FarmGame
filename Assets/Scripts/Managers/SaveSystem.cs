using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Farm
{
    public class SaveSystem
    {
        private static SaveData s_CurrentData = new SaveData();
        
        [System.Serializable]
        public class SaveData
        {
            public PlayerSaveData PlayerData;
            public TerrainSaveData TerrainData;
            public WorkerMgrSaveData WorkerSaveData;
        }

        public static void Save()
        {
            GameManager.Instance.Player.Save(ref s_CurrentData.PlayerData);
            GameManager.Instance.TerrainMgr.Save(ref s_CurrentData.TerrainData);
            GameManager.Instance.WorkerMgr.Save(ref s_CurrentData.WorkerSaveData);

            string savefile = Application.persistentDataPath + "/save.sav";
            File.WriteAllText(savefile, JsonUtility.ToJson(s_CurrentData));
        }

        public static SaveData Load()
        {
            string savefile = Application.persistentDataPath + "/save.sav";
            if (!File.Exists(savefile)) { return null; }

            string content = File.ReadAllText(savefile);

            s_CurrentData = JsonUtility.FromJson<SaveData>(content);
            return s_CurrentData;

            //SceneManager.sceneLoaded += SceneLoaded;
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameManager.Instance.Player.Load(s_CurrentData.PlayerData);
            GameManager.Instance.TerrainMgr.Load(s_CurrentData.TerrainData);

            SceneManager.sceneLoaded -= SceneLoaded;
        }
    }
}