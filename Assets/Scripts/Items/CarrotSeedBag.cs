using UnityEngine;


namespace Farm
{
    [CreateAssetMenu(fileName = "SeedBag", menuName = "2D Farming/Items/SeedBag")]
    public class CarrotSeedBag : Item
    {
        public Crop CarrotCrop;
        public CarrotSeedBag() : base(7)
        {
            CarrotCrop = new Crop(CropID);
        }

        public override bool CanUse(Vector3Int target)
        {
            return GameManager.Instance.Terrain.IsPlantable(target);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.Terrain.PlantAt(target, CarrotCrop);
            return true;
        }
    }
}
