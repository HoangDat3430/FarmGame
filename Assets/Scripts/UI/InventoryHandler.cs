using System;
using Farm;
using UnityEngine;

public class InventoryHandler : UIHandlerBase<InventoryPanel>
{
    private InventorySystem _inventorySystem;
    protected override void Init()
    {
        _inventorySystem = GameManager.Instance.Player.Inventory;
        _panel.UpdateInventoryVisual(_inventorySystem.Entries, true);
    }
    protected override void RegisterEvents()
    {
        _inventorySystem.OnInventoryUpdated += OnInventoryUpdated;
    }

    private void OnInventoryUpdated(bool bForce)
    {
        _panel.UpdateInventoryVisual(_inventorySystem.Entries, bForce);
    }
}