using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Farm.TerrainManager;


namespace Farm
{
    /// <summary>
    /// Manage everything related to the terrain where crop are planted. Hold the content of cells with the states of
    /// crop in those cells. Handle also switching tiles and the like where tilling and watering happens.
    /// </summary>
    public class TerrainManager : MonoBehaviour
    {
        [Serializable]
        public class GroundData
        {
            public const float WaterDuration = 60 * 60.0f;

            public float WaterTimer;
        }

        public class CropData
        {
            [Serializable]
            public struct CropSaveData
            {
                public int CropId;
                public int Stage;
                public float GrowthRatio;
                public float GrowthTimer;
                public int HarvestCount;
                public float DyingTimer;
                public bool HasAnimal;
            }

            public Crop GrowingCrop = null;
            public int CurrentGrowthStage = 0;

            public float GrowthRatio = 0.0f;
            public float GrowthTimer = 0.0f;

            public int HarvestCount = 0;

            public float DyingTimer;
            public bool HarvestDone => HarvestCount == GrowingCrop.NumberOfHarvest;

            public GameObject Animal;
            public void Init()
            {
                GrowingCrop = null;
                GrowthRatio = 0.0f;
                GrowthTimer = 0.0f;
                CurrentGrowthStage = 0;
                HarvestCount = 0;
                DyingTimer = 0.0f;
            }
            public void DisplayAnimalProduct()
            {
                Animal.transform.Find("Product").gameObject.SetActive(Mathf.Approximately(GrowthRatio, 1.0f));
            }
            public Crop Harvest()
            {
                var crop = GrowingCrop;
                //for all plant and animal because when graze an animal in a field, each cell seem to have a crop for that animal
                HarvestCount += 1;
                CurrentGrowthStage = GrowingCrop.StageAfterHarvest;
                GrowthRatio = CurrentGrowthStage / (float)GrowingCrop.GrowthStagesTiles.Length;
                GrowthTimer = GrowingCrop.GrowthTime * GrowthRatio;
                //for animal product because there is only 1 animal in a field
                if (crop.RuleTile == string.Empty && Animal == null)
                {
                    return null;
                }
                return crop;
            }
            public void Save(ref CropSaveData data)
            {
                data.Stage = CurrentGrowthStage;
                data.CropId = GrowingCrop.Key;
                data.DyingTimer = DyingTimer;
                data.GrowthRatio = GrowthRatio;
                data.GrowthTimer = GrowthTimer;
                data.HarvestCount = HarvestCount;
                data.HasAnimal = Animal != null;
            }

            public void Load(CropSaveData data)
            {
                CurrentGrowthStage = data.Stage;
                GrowingCrop = new Crop(data.CropId);
                DyingTimer = data.DyingTimer;
                GrowthRatio = data.GrowthRatio;
                GrowthTimer = data.GrowthTimer;
                HarvestCount = data.HarvestCount;
            }
        }

        public Grid Grid;

        public Tilemap GroundTilemap;
        public Tilemap CropTilemap;
        public GameObject Lock;

        [Header("Watering")]
        public Tilemap WaterTilemap;
        public TileBase WateredTile;

        [Header("Tilling")]
        public TileBase TilleableTile;
        public TileBase TilledTile;

        private Dictionary<Vector3Int, GroundData> m_GroundData = new();
        private Dictionary<Vector3Int, CropData> m_CropData = new();

        private Dictionary<int, List<Vector3Int>> m_FieldGroups = new Dictionary<int, List<Vector3Int>>();
        private Dictionary<int, GameObject> m_RemainingLock = new Dictionary<int, GameObject>();
        private List<int> m_WaitingToHarvestFields = new List<int>();

        public Dictionary<int, List<Vector3Int>> FieldGroups
        {
            get
            {
                return m_FieldGroups;
            }
        }
        public Dictionary<int, GameObject> RemaningLock
        {
            get
            {
                return m_RemainingLock;
            }
        }
        void Awake()
        {
            GameManager.Instance.TerrainMgr = this;
            transform.Find("Warehouse").AddComponent<WorkerManager>();
            GroupTilesByFields();
            InitFarmLands();
        }

        void GroupTilesByFields()
        {
            m_FieldGroups.Clear();

            BoundsInt bounds = GroundTilemap.cellBounds;
            Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>(); // Track visited tiles

            int fieldID = 1; // Start ID for fields

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);

                    if (GroundTilemap.GetTile(position) != null && !visited.ContainsKey(position))
                    {
                        // Perform flood-fill to collect all connected tiles of the same type
                        List<Vector3Int> fieldTiles = FloodFill(GroundTilemap, position, visited, fieldID);

                        if (fieldTiles.Count > 0)
                        {
                            m_FieldGroups[fieldID] = fieldTiles;
                            fieldID++; // Increment field ID for the next group
                        }
                    }
                }
            }
        }
        List<Vector3Int> FloodFill(Tilemap tilemap, Vector3Int start, Dictionary<Vector3Int, int> visited, int fieldID)
        {
            List<Vector3Int> fieldTiles = new List<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(start);
            TileBase tileType = tilemap.GetTile(start);

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();

                if (!visited.ContainsKey(current) && tilemap.GetTile(current) == tileType)
                {
                    visited[current] = fieldID;
                    fieldTiles.Add(current);

                    // Check 4-directional neighbors (Up, Down, Left, Right)
                    Vector3Int[] neighbors = {
                    new Vector3Int(current.x + 1, current.y, 0),
                    new Vector3Int(current.x - 1, current.y, 0),
                    new Vector3Int(current.x, current.y + 1, 0),
                    new Vector3Int(current.x, current.y - 1, 0)
                };

                    foreach (Vector3Int neighbor in neighbors)
                    {
                        if (!visited.ContainsKey(neighbor) && tilemap.GetTile(neighbor) == tileType)
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return fieldTiles;
        }
        public void UnlockFields(int amount)
        {
            for (int i = 1; i <= m_FieldGroups.Count; i++)
            {
                if (m_RemainingLock.ContainsKey(i))
                {
                    Destroy(m_RemainingLock[i]);
                    m_RemainingLock.Remove(i);
                    amount--;
                    if (amount == 0) return;
                }
            }
        }
        private void InitFarmLands()
        {
            for (int i = 1; i <= m_FieldGroups.Count; i++)
            {
                var field = m_FieldGroups[i];
                m_RemainingLock[i] = Instantiate(Lock, field[3], Quaternion.identity);
            }
        }
        public List<Vector3Int> GetFieldByTile(Vector3Int target)
        {
            foreach (var field in m_FieldGroups)
            {
                if (field.Value.Contains(target))
                {
                    return field.Value;
                }
            }
            return null;
        }
        public int GetFieldIdByTile(Vector3Int target)
        {
            foreach (var field in m_FieldGroups)
            {
                if (field.Value.Contains(target))  
                {
                    return field.Key;
                }
            }
            return -1;
        }
        public bool IsTillable(Vector3Int target)
        {
            foreach (var field in m_FieldGroups)
            {
                if (field.Value.Contains(target))
                {
                    if (m_RemainingLock.ContainsKey(field.Key))
                    {
                        return false;
                    }
                }
            }
            return GroundTilemap.GetTile(target) == TilleableTile;
        }

        public bool IsPlantable(Vector3Int target, Crop crop)
        {
            Crop cropInField = GameManager.Instance.TerrainMgr.GetCropDataByPosition(target)?.GrowingCrop;
            return IsTilled(target) && !m_CropData.ContainsKey(target) && (cropInField == null || cropInField.CropID == crop.CropID);
        }
        public bool IsGrazable(Vector3Int target, Crop crop)
        {
            Crop cropInField = GameManager.Instance.TerrainMgr.GetCropDataByPosition(target)?.GrowingCrop;
            return (IsTillable(target) || IsTilled(target)) && !m_CropData.ContainsKey(target) && cropInField == null;
        }
        public bool IsTilled(Vector3Int target)
        {
            return m_GroundData.ContainsKey(target);
        }

        public void TillAt(Vector3Int target)
        {
            if (IsTilled(target))
            {
                return;
            }
            GroundTilemap.SetTile(target, TilledTile);
            m_GroundData.Add(target, new GroundData());
        }

        public void PlantAt(Vector3Int target, Crop cropToPlant)
        {
            var cropData = new CropData();

            cropData.GrowingCrop = cropToPlant;
            cropData.GrowthTimer = 0.0f;
            cropData.CurrentGrowthStage = 0;

            m_CropData.Add(target, cropData);
            UpdateCropVisual(target);

        }
        public void GrazeAt(Vector3Int target, Crop cattleToGraze)
        {
            List<Vector3Int> field = GetFieldByTile(target);
            for (int i = 0; i < field.Count; i++)
            {
                var cropData = new CropData();

                cropData.GrowingCrop = cattleToGraze;
                cropData.GrowthTimer = 0.0f;
                cropData.CurrentGrowthStage = 0;

                if (i==0)//Graze only 1 animal in a field, cells have the same crop data with each other.
                {
                    cropData.Animal = SpawnAnimalPrefab(cattleToGraze.CropID, target, field);
                }
                TillAt(field[i]);
                m_CropData.Add(field[i], cropData);
            }
        }
        private GameObject SpawnAnimalPrefab(int cropId, Vector3Int target, List<Vector3Int> field)
        {
            ItemList.RowData rowData = GameManager.Instance.GetItemByCropID(cropId);
            GameObject animalPrefab = Resources.Load<GameObject>(rowData.PrefabPath);
            GameObject animal = Instantiate(animalPrefab, target, Quaternion.identity);
            animal.GetComponent<BasicAnimalMovement>().SetField(field);
            return animal;
        }
        public void WaterAt(Vector3Int target)
        {
            var groundData = m_GroundData[target];

            groundData.WaterTimer = GroundData.WaterDuration;

            WaterTilemap.SetTile(target, WateredTile);
        }

        public Crop HarvestAt(Vector3Int target)
        {
            m_CropData.TryGetValue(target, out var data);

            if (data == null || !Mathf.Approximately(data.GrowthRatio, 1.0f)) return null;

            var produce = data.Harvest();
            int fieldId = GetFieldIdByTile(target);
            if (m_WaitingToHarvestFields.Contains(fieldId))
            {
                m_WaitingToHarvestFields.Remove(fieldId);
            }

            if (data.HarvestDone)
            {
                m_CropData.Remove(target);
                if(data.Animal != null)
                {
                    Destroy(data.Animal);
                }
            }

            UpdateCropVisual(target);

            return produce;
        }

        public CropData GetCropDataAt(Vector3Int target)
        {
            m_CropData.TryGetValue(target, out var data);
            return data;
        }

        public void OverrideGrowthStage(Vector3Int target, int newGrowthStage)
        {
            var data = GetCropDataAt(target);

            data.GrowthRatio = Mathf.Clamp01((newGrowthStage + 1) / (float)data.GrowingCrop.GrowthStagesTiles.Length);
            data.GrowthTimer = data.GrowthRatio * data.GrowingCrop.GrowthTime;
            data.CurrentGrowthStage = newGrowthStage;

            UpdateCropVisual(target);
        }
        private void Update()
        {
            foreach (var (cell, groundData) in m_GroundData)
            {
                if (groundData.WaterTimer > 0.0f)
                {
                    groundData.WaterTimer -= Time.deltaTime;

                    if (groundData.WaterTimer <= 0.0f)
                    {
                        WaterTilemap.SetTile(cell, null);
                    }
                }

                if (m_CropData.TryGetValue(cell, out var cropData))
                {
                    if (groundData.WaterTimer <= 0.0f && cropData.GrowingCrop.RuleTile != string.Empty)
                    {
                        cropData.DyingTimer += Time.deltaTime;
                        if (cropData.DyingTimer > cropData.GrowingCrop.DryDeathTimer)
                        {
                            m_CropData.Remove(cell);
                            UpdateCropVisual(cell);
                        }
                    }
                    else
                    {
                        cropData.DyingTimer = 0.0f;
                        cropData.GrowthTimer = Mathf.Clamp(cropData.GrowthTimer + Time.deltaTime, 0.0f, cropData.GrowingCrop.GrowthTime);
                        cropData.GrowthRatio = cropData.GrowthTimer / cropData.GrowingCrop.GrowthTime;
                        int growthStage = cropData.GrowingCrop.GetGrowthStage(cropData.GrowthRatio);
                        if (growthStage != cropData.CurrentGrowthStage)
                        {
                            cropData.CurrentGrowthStage = growthStage;
                            UpdateCropVisual(cell);
                        }
                    }
                }
            }
        }
        void UpdateCropVisual(Vector3Int target)
        {
            if (!m_CropData.TryGetValue(target, out var data))
            {
                CropTilemap.SetTile(target, null);
            }
            else
            {
                if(data.Animal != null)
                {
                    data.DisplayAnimalProduct();
                }
                CropTilemap.SetTile(target, data.GrowingCrop.GrowthStagesTiles?[data.CurrentGrowthStage]);
            }
        }
        public CropData GetCropDataByFieldID(int fieldID)
        {
            foreach (var cell in m_FieldGroups[fieldID])
            {
                if (m_CropData.ContainsKey(cell))
                {
                    return m_CropData[cell];
                }
            }
            return null;
        }
        public CropData GetCropDataByPosition(Vector3Int pos)
        {
            foreach (var field in m_FieldGroups)
            {
                if (field.Value.Contains(pos))
                {
                    return GetCropDataByFieldID(field.Key);
                }
            }
            return null;
        }
        public int GetHarvestableField()
        {
            foreach (var field in m_FieldGroups)
            {
                if (!m_RemainingLock.ContainsKey(field.Key) && !m_WaitingToHarvestFields.Contains(field.Key))
                {
                    CropData cropData = GetCropDataByFieldID(field.Key);
                    if(cropData != null && Mathf.Approximately(cropData.GrowthRatio, 1.0f))
                    {
                        return field.Key;
                    }
                }
            }
            return -1;
        }
        public Vector3Int AddWaitingToHarvestField(int fieldId)
        {
            m_WaitingToHarvestFields.Add(fieldId);
            return m_FieldGroups[fieldId][4];
        }
        public void OnWorkerHavestDone(int fieldId)
        {
            m_WaitingToHarvestFields.Remove(fieldId);
        }
        public void Save(ref TerrainSaveData data)
        {
            data.GroundDatas = new List<GroundData>();
            data.GroundDataPositions = new List<Vector3Int>();
            foreach (var groundData in m_GroundData)
            {
                data.GroundDataPositions.Add(groundData.Key);
                data.GroundDatas.Add(groundData.Value);
            }

            data.CropDatas = new List<CropData.CropSaveData>();
            data.CropDataPositions = new List<Vector3Int>();
            foreach (var cropData in m_CropData)
            {
                data.CropDataPositions.Add(cropData.Key);

                var saveData = new CropData.CropSaveData();
                cropData.Value.Save(ref saveData);
                data.CropDatas.Add(saveData);
            }

            data.OpenedFields = m_FieldGroups.Count - m_RemainingLock.Count;
            data.waitingToHarvestFields = m_WaitingToHarvestFields;
        }

        public void Load(TerrainSaveData data)
        {
            m_GroundData = new Dictionary<Vector3Int, GroundData>();
            for (int i = 0; i < data.GroundDatas.Count; ++i)
            {
                var pos = data.GroundDataPositions[i];
                m_GroundData.Add(pos, data.GroundDatas[i]);

                GroundTilemap.SetTile(pos, TilledTile);

                WaterTilemap.SetTile(data.GroundDataPositions[i], data.GroundDatas[i].WaterTimer > 0.0f ? WateredTile : null);
            }

            m_CropData = new Dictionary<Vector3Int, CropData>();
            for (int i = 0; i < data.CropDatas.Count; ++i)
            {
                CropData newData = new CropData();
                newData.Load(data.CropDatas[i]);
                if (data.CropDatas[i].HasAnimal)
                {
                    GameObject animal = SpawnAnimalPrefab(data.CropDatas[i].CropId, data.CropDataPositions[i], m_FieldGroups[GetFieldIdByTile(data.CropDataPositions[i])]);
                    newData.Animal = animal;
                }
                m_CropData.Add(data.CropDataPositions[i], newData);
                UpdateCropVisual(data.CropDataPositions[i]);
            }
            UnlockFields(data.OpenedFields);
            m_WaitingToHarvestFields = data.waitingToHarvestFields;
        }
    }
    [Serializable]
    public struct TerrainSaveData
    {
        public List<Vector3Int> GroundDataPositions;
        public List<GroundData> GroundDatas;

        public List<Vector3Int> CropDataPositions;
        public List<CropData.CropSaveData> CropDatas;

        public int OpenedFields;
        public List<int> waitingToHarvestFields;
    }
}
