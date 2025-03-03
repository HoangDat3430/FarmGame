using UnityEngine;


namespace Farm
{
    public class SeedBag : Item
    {
        public SeedBag(int itemID) : base(itemID)
        {
        }
        public override bool CanUse(Vector3Int target)
        {
            return GameManager.Instance.Terrain.IsPlantable(target, Crop);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.Terrain.PlantAt(target, Crop);
            return true;
        }
    }
}
