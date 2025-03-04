using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;

namespace Farm
{
    /// <summary>
    /// Class used to designated a crop on the map. Store all the stage of growth, time to grow etc.
    /// </summary>
    public class Crop
    {
        public int Key => CropID;

        public int CropID;
        
        public TileBase[] GrowthStagesTiles;

        public Product Product;
        
        public float GrowthTime;
        public int NumberOfHarvest;
        public int StageAfterHarvest;
        public int ProductPerHarvest;
        public float DryDeathTimer;
        public int ProductID;
        public int Stages;
        public string RuleTile;

        public Crop(int cropID)
        {
            this.CropID = cropID;
            CropList.RowData rowData = GameManager.Instance.GetCropByCropID(Key);
            GrowthTime = rowData.GrowthTime;
            NumberOfHarvest = rowData.HarvestNum;
            StageAfterHarvest = rowData.StageAfterHarvest;
            ProductPerHarvest = rowData.ProductPerHarvest;
            DryDeathTimer = rowData.DieTime;
            ProductID = rowData.ProductID;
            Stages = rowData.Stages;
            RuleTile = rowData.RuleTile;
            GrowthStagesTiles = new TileBase[Stages];
            if(rowData.RuleTile != string.Empty)
            {
                for (int i = 0; i < rowData.Stages; i++)
                {
                    GrowthStagesTiles[i] = new Tile();
                    (GrowthStagesTiles[i] as Tile).sprite = Resources.Load<Sprite>(rowData.RuleTile + i);
                }
            }
            Product = new Product(rowData.ProductID);
        }

        public int GetGrowthStage(float growRatio)
        {
            return Mathf.FloorToInt(growRatio * (Stages-1));
        }
    }
}
