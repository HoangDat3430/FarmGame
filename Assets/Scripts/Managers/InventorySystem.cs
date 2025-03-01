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
            Entries[0].Item.Init();
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
                    int size = newItem.StackSize - Entries[i].StackSize;
                    toFit -= size;

                    if (toFit <= 0)
                        return true;
                }
            }

            for (int i = 0; i < InventorySize; ++i)
            {
                if (Entries[i].Item == null)
                {
                    toFit -= newItem.StackSize;
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
                if (Entries[i].Item == newItem && Entries[i].StackSize < newItem.StackSize)
                {
                    int fit = Mathf.Min(newItem.StackSize - Entries[i].StackSize, remainingToFit);
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
                    int fit = Mathf.Min(newItem.StackSize - Entries[i].StackSize, remainingToFit);
                    remainingToFit -= fit;
                    Entries[i].StackSize = fit;

                    if (remainingToFit == 0)
                        return true;
                }
            }

            //we couldn't had so no space left
            return remainingToFit == 0;
        }

        //return the actual amount removed
        public int Remove(int index, int count)
        {
            if (index < 0 || index >= Entries.Length)
                return 0;

            int amount = Mathf.Min(count, Entries[index].StackSize);

            Entries[index].StackSize -= amount;

            if (Entries[index].StackSize == 0)
            {
                Entries[index].Item = null;
            }

            return amount;
        }

        public void EquipNext()
        {
            EquippedItemIdx += 1;
            if (EquippedItemIdx >= InventorySize) EquippedItemIdx = 0;
        }

        public void EquipPrev()
        {
            EquippedItemIdx -= 1;
            if (EquippedItemIdx < 0) EquippedItemIdx = InventorySize - 1;
        }

        public void EquipItem(int index)
        {
            if (index < 0 || index >= Entries.Length)
                return;

            EquippedItemIdx = index;
        }
    }
}