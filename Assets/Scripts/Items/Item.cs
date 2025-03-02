using UnityEngine;

namespace Farm
{
    public class Item
    {
        public int Key => ItemID;

        [Tooltip("Name used in the database for that Item, used by save system")]
        public int ItemID;
        public string ItemName;
        public int BuyPrice;
        public int SellPrice;
        public string PrefabPath;
        public string IconPath;
        public int MaxStackSize;
        public int CropID;
        public bool Consumable;
        public string PlayerAnimatorTriggerUse;

        public Item(int itemID)
        {
            this.ItemID = itemID;
            ItemList.RowData rowData = GameManager.Instance.GetItemByID(Key);
            ItemName = rowData.ItemName;
            BuyPrice = rowData.BuyPrice;
            SellPrice = rowData.SellPrice   ;
            BuyPrice = rowData.BuyPrice;
            PrefabPath = rowData.PrefabPath;
            IconPath = rowData.IconPath;
            MaxStackSize = rowData.StackSize;
            CropID = rowData.CropID;
            Consumable = rowData.Consumable;
            PlayerAnimatorTriggerUse = rowData.Animator;
        }

        public virtual bool CanUse(Vector3Int target)
        {
            return true;
        }
        public virtual bool Use(Vector3Int target)
        {
            return true;
        }

        //override this for item that does not need a target (like Product, they can be eaten anytime)
        public virtual bool NeedTarget()
        {
            return true;
        }
    }
}