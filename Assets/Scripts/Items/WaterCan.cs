using UnityEngine;

namespace Farm
{
    [CreateAssetMenu(fileName = "WaterCan", menuName = "2D Farming/Items/Water Can")]
    public class WaterCan : Item
    {
        public WaterCan() : base(3)
        {
        }
        public override bool CanUse(Vector3Int target)
        {
            return GameManager.Instance.TerrainMgr.IsTilled(target);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.TerrainMgr.WaterAt(target);
            return true;
        }
    }
}