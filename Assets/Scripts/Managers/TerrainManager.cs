using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;


namespace Farm
{
    /// <summary>
    /// Manage everything related to the terrain where crop are planted. Hold the content of cells with the states of
    /// crop in those cells. Handle also switching tiles and the like where tilling and watering happens.
    /// </summary>
    public class TerrainManager : MonoBehaviour
    {
        [System.Serializable]
        public class GroundData
        {
            public const float WaterDuration = 60 * 1.0f;

            public float WaterTimer;
        }

        public class CropData
        {
            [Serializable]
            public struct SaveData
            {
                public string CropId;
                public int Stage;
                public float GrowthRatio;
                public float GrowthTimer;
                public int HarvestCount;
                public float DyingTimer;
            }

            public Crop GrowingCrop = null;
            public int CurrentGrowthStage = 0;

            public float GrowthRatio = 0.0f;
            public float GrowthTimer = 0.0f;

            public int HarvestCount = 0;

            public float DyingTimer;
            public bool HarvestDone => HarvestCount == GrowingCrop.NumberOfHarvest;

            public void Init()
            {
                GrowingCrop = null;
                GrowthRatio = 0.0f;
                GrowthTimer = 0.0f;
                CurrentGrowthStage = 0;
                HarvestCount = 0;

                DyingTimer = 0.0f;
            }

            public Crop Harvest()
            {
                var crop = GrowingCrop;

                HarvestCount += 1;

                CurrentGrowthStage = GrowingCrop.StageAfterHarvest;
                GrowthRatio = CurrentGrowthStage / (float)GrowingCrop.GrowthStagesTiles.Length;
                GrowthTimer = GrowingCrop.GrowthTime * GrowthRatio;

                return crop;
            }

            public void Save(ref SaveData data)
            {
                data.Stage = CurrentGrowthStage;
                data.CropId = GrowingCrop.Key;
                data.DyingTimer = DyingTimer;
                data.GrowthRatio = GrowthRatio;
                data.GrowthTimer = GrowthTimer;
                data.HarvestCount = HarvestCount;
            }

            public void Load(SaveData data)
            {
                CurrentGrowthStage = data.Stage;
                //GrowingCrop = GameManager.Instance.CropDatabase.GetFromID(data.CropId);
                DyingTimer = data.DyingTimer;
                GrowthRatio = data.GrowthRatio;
                GrowthTimer = data.GrowthTimer;
                HarvestCount = data.HarvestCount;
            }
        }

        public Grid Grid;

        public Tilemap GroundTilemap;
        public Tilemap CropTilemap;

        [Header("Watering")]
        public Tilemap WaterTilemap;
        public TileBase WateredTile;

        [Header("Tilling")]
        public TileBase TilleableTile;
        public TileBase TilledTile;

        private Dictionary<Vector3Int, GroundData> m_GroundData = new();
        private Dictionary<Vector3Int, CropData> m_CropData = new();

        public bool IsTillable(Vector3Int target)
        {
            return GroundTilemap.GetTile(target) == TilleableTile;
        }

        public bool IsPlantable(Vector3Int target)
        {
            return IsTilled(target) && !m_CropData.ContainsKey(target);
        }

        public bool IsTilled(Vector3Int target)
        {
            return m_GroundData.ContainsKey(target);
        }

        public void TillAt(Vector3Int target)
        {
            if (IsTilled(target))
                return;

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

            if (data.HarvestDone)
            {
                m_CropData.Remove(target);
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

        private void Awake()
        {
            GameManager.Instance.Terrain = this;
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
                        //GroundTilemap.SetColor(cell, Color.white);
                    }
                }

                if (m_CropData.TryGetValue(cell, out var cropData))
                {
                    if (groundData.WaterTimer <= 0.0f)
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
                        cropData.GrowthTimer = Mathf.Clamp(cropData.GrowthTimer + Time.deltaTime, 0.0f,
                            cropData.GrowingCrop.GrowthTime);
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
                CropTilemap.SetTile(target, data.GrowingCrop.GrowthStagesTiles[data.CurrentGrowthStage]);
            }
        }
    }
}
