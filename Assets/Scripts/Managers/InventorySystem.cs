using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

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
            public int StackSize;
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
            Entries[0].Item = new Hoe();
            Entries[0].StackSize = 1;
            Entries[1].Item = new Basket();
            Entries[1].StackSize = 1;
            Entries[2].Item = new WaterCan();
            Entries[2].StackSize = 1;
            Entries[3].Item = new CornSeedBag();
            Entries[3].StackSize = 10;
            Entries[4].Item = new WheatSeedBag();
            Entries[4].StackSize = 10;
            Entries[5].Item = new Product(4);
            Entries[5].StackSize = 10;
            Entries[6].Item = new Product(5);
            Entries[6].StackSize = 10;
            Entries[7].Item = new Product(6);
            Entries[8].StackSize = 10;
            EquippedItemIdx = 0;
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
                if (Entries[i].Item == newItem)
                {
                    int size = newItem.MaxStackSize - Entries[i].StackSize;
                    toFit -= size;

                    if (toFit <= 0)
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
        public bool AddItem(Item newItem, int amount = 1)
        {
            int remainingToFit = amount;

            //first we check if there is already that item in the inventory
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == newItem && Entries[i].StackSize < newItem.MaxStackSize)
                {
                    int fit = Mathf.Min(newItem.MaxStackSize - Entries[i].StackSize, remainingToFit);
                    Entries[i].StackSize += fit;
                    remainingToFit -= fit;
                    if (remainingToFit == 0)
                        return true;
                }
            }

            //if we reach here we couldn't fit it in existing stack, so we look for an empty place to fit it
            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == null)
                {
                    Entries[i].Item = newItem;
                    int fit = Mathf.Min(newItem.MaxStackSize - Entries[i].StackSize, remainingToFit);
                    remainingToFit -= fit;
                    Entries[i].StackSize = fit;

                    if (remainingToFit == 0)
                        return true;
                }
            }

            //we couldn't had so no space left
            return remainingToFit == 0;
        }

        public void EquipNext()
        {
            EquippedItemIdx += 1;
            if (EquippedItemIdx >= InventorySize) EquippedItemIdx = 0;
            if (EquippedItem != null)
            {
                Debug.LogError(EquippedItem.ItemName + " " + Entries[EquippedItemIdx].StackSize);
            }
        }
        public void EquipPrev()
        {
            EquippedItemIdx -= 1;
            if (EquippedItemIdx < 0) EquippedItemIdx = InventorySize - 1;
            if (EquippedItem != null)
            {
                Debug.LogError(EquippedItem.ItemName + " " + Entries[EquippedItemIdx].StackSize);
            }
        }
        public List<InventoryEntry> GetSellableList()
        {
            List<InventoryEntry> list = new List<InventoryEntry>();
            foreach(var entry in Entries)
            {
                if(entry.Item != null && entry.Item.SellPrice != -1)
                {
                    list.Add(entry);
                }
            }
            return list;
        }
        public void SellItem(Item item, int count)
        {
            foreach (var entry in Entries)
            {
                if (entry.Item != null && entry.Item == item)
                {
                    entry.StackSize -= count;
                    GameManager.Instance.AddCoin(item.SellPrice);
                    if(entry.StackSize == 0)
                    {
                        entry.Item = null;
                    }
                }
            }
        }
    }
}