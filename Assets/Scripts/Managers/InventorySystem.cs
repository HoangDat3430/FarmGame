using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Farm
{
    /// <summary>
    /// Handle the player inventory. This is fixed size (9 right now)
    /// </summary>
    [Serializable]
    public class InventorySystem
    {
        public const int InventorySize = 9;

        public class InventoryEntry
        {
            public Item Item;
            public float StackSize;
        }

        public int EquippedItemIdx { get; private set; }
        public Item EquippedItem => Entries[EquippedItemIdx].Item;

        public InventoryEntry[] Entries;

        public void Init()
        {
            Entries = new InventoryEntry[InventorySize];
            for(int i = 0; i< InventorySize; i++)
            {
                Entries[i] = new InventoryEntry();
            }
            EquippedItemIdx = 0;
        }
        public void SetStartingInventory()
        {
            Entries[0].Item = new Hoe();
            Entries[0].StackSize = 1;
            Entries[1].Item = new Basket();
            Entries[1].StackSize = 1;
            Entries[2].Item = new WaterCan();
            Entries[2].StackSize = 1;
            Entries[3].Item = new SeedBag(7);
            Entries[3].StackSize = 10;
            Entries[4].Item = new SeedBag(8);
            Entries[4].StackSize = 10;
            Entries[5].Item = new Animal(10);
            Entries[5].StackSize = 1;
        }

        //return true if the object could be used
        public bool UseEquippedObject(Vector3Int target)
        {
            if (EquippedItem == null || !EquippedItem.CanUse(target))
                return false;

            bool used = EquippedItem.Use(target);

            if (used)
            {
                if (EquippedItem.Consumable)
                {
                    Entries[EquippedItemIdx].StackSize -= 1;

                    if (Entries[EquippedItemIdx].StackSize == 0)
                    {
                        Entries[EquippedItemIdx].Item = null;
                    }
                    GameManager.Instance.UpdateInventoryVisual(true);
                }
            }

            return used;
        }

        // Will return true if we have enough space in the inventory to fit the required amount of the given item.
        public bool CanFitItem(Item newItem, int amount)
        {
            int toFit = amount;

            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item?.ItemID == newItem.ItemID)
                {
                    float size = newItem.MaxStackSize - Entries[i].StackSize;
                    if (toFit <= size)
                        return true;
                }
            }

            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == null)
                {
                    toFit -= newItem.MaxStackSize;
                    if (toFit <= 0)
                        return true;
                }
            }

            return toFit == 0;
        }
        public bool AddItem(Item newItem, float amount = 1)
        {
            SpecifyItem(ref newItem);
            //first we check if there is already that item in the inventory
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item?.ItemID == newItem.ItemID)
                {
                    float available = newItem.MaxStackSize - Entries[i].StackSize;
                    if(available >= amount)
                    {
                        Entries[i].StackSize += amount;
                        Entries[i].StackSize = (float)Math.Round(Entries[i].StackSize * 10) / 10;
                        GameManager.Instance.UpdateInventoryVisual(true);
                        return true;
                    }
                }
            }

            //if we reach here we couldn't fit it in existing stack, so we look for an empty place to fit it
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == null)
                {
                    Entries[i].Item = newItem;
                    Entries[i].StackSize = amount;
                    GameManager.Instance.UpdateInventoryVisual(true);
                    return true;
                }
            }
            //we couldn't had so no space left
            return false;
        }

        public void EquipNext()
        {
            EquippedItemIdx += 1;
            if (EquippedItemIdx >= InventorySize) EquippedItemIdx = 0;
            GameManager.Instance.UpdateInventoryVisual(false);
        }
        public void EquipPrev()
        {
            EquippedItemIdx -= 1;
            if (EquippedItemIdx < 0) EquippedItemIdx = InventorySize - 1;
            GameManager.Instance.UpdateInventoryVisual(false);
        }
        public List<InventoryEntry> GetSellableList()
        {
            List<InventoryEntry> list = new List<InventoryEntry>();
            foreach(var entry in Entries)
            {
                if (entry.Item != null && entry.Item.SellPrice != -1 && entry.StackSize >= 1)
                {
                    list.Add(entry);
                }
            }
            return list;
        }
        public void OnSellItem(Item item, int count)
        {
            foreach (var entry in Entries)
            {
                if (entry.Item != null && entry.Item == item)
                {
                    entry.StackSize -= count;
                    GameManager.Instance.Player.AddCoin(item.SellPrice * count);
                    if(entry.StackSize == 0)
                    {
                        entry.Item = null;
                    }
                }
            }
        }
        private void SpecifyItem(ref Item item)
        {
            switch (item.Type)
            {
                case ItemType.SeedBag:
                    item = new SeedBag(item.ItemID);
                    break;
                case ItemType.Animal:
                    item = new Animal(item.ItemID);
                    break;
                default:
                    break;
            }
        }
        private Item CreateItem(int itemID, ItemType type)
        {
            switch (type)
            {
                case ItemType.Tool:
                    if (itemID == 1)
                    {
                        return new Basket();
                    }
                    else if (itemID == 2)
                    {
                        return new Hoe();
                    }
                    else if (itemID == 3)
                    {
                        return new WaterCan();
                    }
                    break;
                case ItemType.SeedBag:
                    return new SeedBag(itemID);
                case ItemType.Animal:
                    return new Animal(itemID);
                case ItemType.Product:
                    return new Product(itemID);
                default:
                    break;
            }
            return null;
        }
        public void Save(ref List<InventorySaveData> data)
        {
            foreach (var entry in Entries)
            {
                if (entry.Item != null)
                {
                    data.Add(new InventorySaveData()
                    {
                        Amount = entry.StackSize,
                        ItemID = entry.Item.ItemID,
                        ItemType = (int)entry.Item.Type
                    });
                }
                else
                {
                    data.Add(null);
                }
            }
        }

        // Load the content in the given list inside that inventory.
        public void Load(List<InventorySaveData> data)
        {
            Init();
            for (int i = 0; i < data.Count; ++i)
            {
                if (data[i].Amount != 0)
                {
                    Item item = CreateItem(data[i].ItemID, (ItemType)data[i].ItemType);
                    Entries[i].Item = item;
                    Entries[i].StackSize = data[i].Amount;
                }
                else
                {
                    Entries[i].Item = null;
                    Entries[i].StackSize = 0;
                }
            }
        }
    }
    [Serializable]
    public class InventorySaveData
    {
        public float Amount;
        public int ItemID;
        public int ItemType;
    }
}
    
