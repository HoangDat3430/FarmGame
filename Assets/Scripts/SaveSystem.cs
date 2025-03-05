using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Farm
{
    public class SaveSystem
    {
        private static SaveData s_CurrentData = new SaveData();
        
        [System.Serializable]
        public struct SaveData
        {
            public PlayerSaveData PlayerData;
            public TerrainDataSave TerrainData;
        }

        public static void Save()
        {
            GameManager.Instance.Player.Save(ref s_CurrentData.PlayerData);
            GameManager.Instance.TerrainMgr.Save(ref s_CurrentData.TerrainData);

            string savefile = Application.persistentDataPath + "/save.sav";
            File.WriteAllText(savefile, JsonUtility.ToJson(s_CurrentData));
            Debug.LogError("Saved");
        }

        public static void Load()
        {
            string savefile = Application.persistentDataPath + "/save.sav";
            string content = File.ReadAllText(savefile);

            s_CurrentData = JsonUtility.FromJson<SaveData>(content);
            
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            Debug.LogError("Loaded");
        }

        static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            GameManager.Instance.Player.Load(s_CurrentData.PlayerData);
            GameManager.Instance.TerrainMgr.Load(s_CurrentData.TerrainData);

            SceneManager.sceneLoaded -= SceneLoaded;
        }
    }
}