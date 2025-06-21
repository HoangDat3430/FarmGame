using Farm;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : UIPanelBase<InventoryPanel, InventoryHandler>
{
    protected override void RegisterInternalEvents()
    {
    }
    public override void Show()
    {
        base.Show();
    }
    public override void Hide()
    {
        base.Hide();
    }
    public void UpdateInventoryVisual(InventorySystem.InventoryEntry[] entries, bool bForce = true)
    {
        for (int i = 0; i < entries.Length; i++)
        {
            Transform slot = transform.GetChild(i);
            if (bForce)
            {
                InventorySystem.InventoryEntry entry = entries[i];
                Image icon = slot.Find("Icon").GetComponent<Image>();
                TMP_Text countT = slot.Find("Num").GetComponent<TMP_Text>();
                string text = string.Empty;
                if (entry.Item != null)
                {
                    icon.sprite = Resources.Load<Sprite>(entry.Item.IconPath);
                    switch (entry.Item.Type)
                    {
                        case ItemType.SeedBag:
                        case ItemType.Animal:
                            text = entry.StackSize.ToString();
                            break;
                        case ItemType.Product:
                            text = entry.StackSize.ToString("0.0");
                            break;
                        default:
                            break;
                    }
                }
                icon.gameObject.SetActive(entry.Item != null);
                countT.text = text;
            }
            slot.Find("selected").gameObject.SetActive(i == GameManager.Instance.Player.Inventory.EquippedItemIdx);
        }
    }
}
