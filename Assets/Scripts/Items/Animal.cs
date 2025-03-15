using UnityEngine;

namespace Farm
{
    public class Animal : Item
    {
        public Animal (int itemID) : base(itemID)
        {

        }
        public override bool CanUse(Vector3Int target)
        {

            return GameManager.Instance.TerrainMgr.IsGrazable(target, Crop);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.TerrainMgr.GrazeAt(target, Crop);
            return true;
        }
    }
}