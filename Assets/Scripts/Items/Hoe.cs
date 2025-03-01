using UnityEngine;

namespace Farm
{
    public class Hoe : Item
    {
        public override void Init()
        {
            ItemID = 2;
            base.Init();
        }
        public override bool CanUse(Vector3Int target)
        {
            return GameManager.Instance?.Terrain != null && GameManager.Instance.Terrain.IsTillable(target);
        }

        public override bool Use(Vector3Int target)
        {
            GameManager.Instance.Terrain.TillAt(target);
            return true;
        }
    }
}
