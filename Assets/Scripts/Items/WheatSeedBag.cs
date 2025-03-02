using UnityEngine;


namespace Farm
{
    public class WheatSeedBag : Item
    {
        public Crop WheatCrop;
        public WheatSeedBag() : base(9)
        {
            WheatCrop = new Crop(CropID);
        }

        public override bool CanUse(Vector3Int target)
        {
            return GameManager.Instance.Terrain.IsPlantable(target);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.Terrain.PlantAt(target, WheatCrop);
            return true;
        }
    }
}
