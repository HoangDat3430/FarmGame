using UnityEngine;

namespace Farm
{
    public class Hoe : Item
    {
        public Hoe() : base(2)
        {
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
