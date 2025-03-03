using UnityEngine;

namespace Farm
{
    public class Product : Item
    {
        public Product(int itemID) : base(itemID) { }
        public override bool CanUse(Vector3Int target)
        {
            return false;
        }

        public override bool Use(Vector3Int target)
        {
            return false;
        }

        public override bool NeedTarget()
        {
            return false;
        }
    }
}
