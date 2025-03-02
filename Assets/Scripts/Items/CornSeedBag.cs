using UnityEngine;


namespace Farm
{
    public class CornSeedBag : Item
    {
        public Crop CornCrop;
        public CornSeedBag() : base(8)
        {
            CornCrop = new Crop(CropID);
        }

        public override bool CanUse(Vector3Int target)
        {
            return GameManager.Instance.Terrain.IsPlantable(target);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.Terrain.PlantAt(target, CornCrop);
            return true;
        }
    }
}
