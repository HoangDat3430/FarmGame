using Farm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : UIPanelBase<InventoryPanel, InventoryHandler>
{
    protected override void RegisterInternalEvents()
    {
        // Register any internal events specific to the InventoryPanel here
    }
    public override void Show()
    {
        base.Show();
        // Additional logic for showing the inventory panel can be added here
    }
    public override void Hide()
    {
        base.Hide();
        // Additional logic for hiding the inventory panel can be added here
    }
}
