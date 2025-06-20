using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Farm
{
    /// <summary>
    /// The GameManager is the entry point to all the game system. It's execution order is set very low to make sure
    /// its Awake function is called as early as possible so the instance if valid on other Scripts. 
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_Instance;
        
        
#if UNITY_EDITOR
        //As our manager run first, it will also be destroyed first when the app will be exiting, which lead to s_Instance
        //to become null and so will trigger another instantiate in edit mode (as we dynamically instantiate the Manager)
        //so this is set to true when destroyed, so we do not reinstantiate a new one
        private static bool s_IsQuitting = false;
#endif
        public static GameManager Instance 
        {
            get
            {
//#if UNITY_EDITOR
//                if (!Application.isPlaying || s_IsQuitting)
//                    return null;
                
//                if (s_Instance == null)
//                {
//                    //in editor, we can start any scene to test, so we are not sure the game manager will have been
//                    //created by the first scene starting the game. So we load it manually. This check is useless in
//                    //player build as the 1st scene will have created the GameManager so it will always exists.
//                    Instantiate(Resources.Load<GameManager>("GameManager"));
//                }
//#endif
                return s_Instance;
            }
        }
        public TerrainManager TerrainMgr { get; set; }
        public WorkerManager WorkerMgr { get; set; }
        public PlayerController Player { get; set; }
        public DayCycleHandler DayCycleHandler { get; set; }
        
        // Will return the ratio of time for the current day between 0 (00:00) and 1 (23:59).
        public float CurrentDayRatio => m_CurrentTimeOfTheDay / DayDurationInSeconds;


        [Header("Camera")]
        public CinemachineVirtualCamera MainCamera;
        
        [Header("Time settings")]
        [Min(1.0f)] 
        public float DayDurationInSeconds;
        public float StartingTime = 0.0f;

        private bool m_IsTicking;
        
        private float m_CurrentTimeOfTheDay;

        private ItemList m_ItemConfigs = new ItemList();
        private CropList m_CropConfigs = new CropList();

        private UIHandler uiLogic;

        private void Awake()
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            InitConfigData();

            m_IsTicking = true;
            
            //we need to ensure that we don't have a day length at 0, otherwise we will get stuck into infinite loop in update
            //(and a day with 0 length makes no sense)
            if (DayDurationInSeconds <= 0.0f)
            {
                DayDurationInSeconds = 1.0f;
                Debug.LogError("The day length on the GameManager is set to 0, the length need to be set to a positive value");
            }
        }

        private void Start()
        {
            m_CurrentTimeOfTheDay = StartingTime;
            IGameUI gameUI = FindObjectOfType<GameUI>();  // Inject UI v�o Logic
            uiLogic = new UIHandler(gameUI);
            SaveSystem.SaveData saveData = SaveSystem.Load();
            if(saveData != null)
            {
                Player.Load(saveData.PlayerData);
                TerrainMgr.Load(saveData.TerrainData);
                WorkerMgr.Load(saveData.WorkerSaveData);
            }
            else
            {
                Player.Inventory.SetStartingInventory();
                TerrainMgr.UnlockFields(3);
                WorkerMgr.EmployWorker();
            }
            // UpdateInventoryVisual(true);
            // UpdateIdleWorkers();
            // UpdateToolLevel();
        }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            s_IsQuitting = true;
        }
#endif

        private void Update()
        {
            if (m_IsTicking)
            {
                float previousRatio = CurrentDayRatio;
                m_CurrentTimeOfTheDay += Time.deltaTime;

                while (m_CurrentTimeOfTheDay > DayDurationInSeconds)
                    m_CurrentTimeOfTheDay -= DayDurationInSeconds;
            }
            if(DayCycleHandler != null)
            {
                DayCycleHandler.Tick();
            }
        }
        private void InitConfigData()
        {
            m_ItemConfigs.ReadFile("Data/ItemList.csv");
            m_CropConfigs.ReadFile("Data/CropList.csv");
        }
        public void SetCameraFollow(Transform player)
        {
            player.position = transform.position;
            Instance.MainCamera.Follow = player;
            Instance.MainCamera.LookAt = player;
            Instance.MainCamera.ForceCameraPosition(player.position, Quaternion.identity);
        }
        public List<ItemList.RowData> ItemsToBuy()
        {
            List<ItemList.RowData> list = new List<ItemList.RowData>();
            foreach(var item in m_ItemConfigs.Table)
            {
                if(item.BuyPrice != -1)
                {
                    list.Add(item);
                }
            }
            return list;
        }
        public void OpenMarket()
        {
            uiLogic.ShowMarket();
        }
        public void UpdateCoin()
        {
            uiLogic.UpdateCoin();
        }
        public void OpenFarmStore()
        {
            uiLogic.ShowLandList();
        }
        public void UpdateInventoryVisual(bool bForce)
        {
            uiLogic.UpdateInventoryVisual(bForce);
        }
        public void UpdateIdleWorkers()
        {
            uiLogic.UpdateIdleWorkers();
        }
        public void UpdateToolLevel()
        {
            uiLogic.UpdateToolLevel();
        }
        public ItemList.RowData GetItemByItemID(int itemId)
        {
            return m_ItemConfigs.GetByItemId(itemId);
        }
        public ItemList.RowData GetItemByCropID(int cropId)
        {
            return m_ItemConfigs.GetByCropId(cropId);
        }
        public CropList.RowData GetCropByCropID(int cropId)
        {
            return m_CropConfigs.GetByCropId(cropId);
        }
        private void OnApplicationQuit()
        {
            SaveSystem.Save();
        }
    }
}