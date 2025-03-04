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
                for (int i = 0; i < product.ProductPerHarvest; ++i)
                {
                    GameManager.Instance.Player.AddItem(product.Product);
                }
               
                return true;
            }

            return false;
        }
    }
}