using UnityEngine;

namespace Farm
{
    public class Basket : Item
    {
        public Basket() : base(1)
        {
        }
        public override bool CanUse(Vector3Int target)
        {
            var data = GameManager.Instance.TerrainMgr.GetCropDataAt(target);
            return data != null && data.GrowingCrop != null && Mathf.Approximately(data.GrowthRatio, 1.0f);
        }

        public override bool Use(Vector3Int target)
        {
            var data = GameManager.Instance.TerrainMgr.GetCropDataAt(target);
            if (!GameManager.Instance.Player.CanFitInInventory(data.GrowingCrop.Product,
                    data.GrowingCrop.ProductPerHarvest))
                return false;
            
            var product = GameManager.Instance.TerrainMgr.HarvestAt(target);

            if (product != null)
            {
                float toolBoost = (float)(GameManager.Instance.ToolLevel - 1) / 10;
                float productivityBoost = product.ProductPerHarvest + toolBoost;
                GameManager.Instance.Player.AddItem(product.Product, productivityBoost);
                return true;
            }

            return false;
        }
    }
}