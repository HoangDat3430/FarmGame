using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Farm;
using UnityEngine.Events;
using System;

public class GameUI : MonoBehaviour, IGameUI
{
    private GameObject itemForSell;
    private GameObject itemInstock;
    private GameObject farmTpl;
    private Transform sellPanel;
    private Transform buyPanel;
    private Transform inventoryPanel;
    private Transform farmPanel;
    private Transform m_GameOverPanel;

    private TMP_Text m_CoinText;
    private TMP_Text m_IdleWorkers;
    private TMP_Text m_ToolLevel;
    private GameObject m_Store;
    private GameObject m_FarmList;

    private Button m_SellBtn;
    private Button m_BuyBtn;
    private Button m_CloseStoreBtn;
    private Button m_ExpandFarmBtn;
    private Button m_CloseFarmListBtn;
    private Button m_EmployBtn;
    private Button m_UpgradeBtn;
    private Button m_ExitBtn;

    private void Awake()
    {
        itemForSell = Resources.Load<GameObject>("Prefabs/UI/ItemSellTpl");
        itemInstock = Resources.Load<GameObject>("Prefabs/UI/ItemBuyTpl");
        farmTpl = Resources.Load<GameObject>("Prefabs/UI/LandTpl");

        sellPanel = transform.Find("Store/ScrollView/Viewport/Sell");
        buyPanel = transform.Find("Store/ScrollView/Viewport/Buy");
        inventoryPanel = transform.Find("Inventory");
        farmPanel = transform.Find("FarmList/ScrollView/Viewport");
        m_GameOverPanel = transform.Find("Victory");

        m_CoinText = transform.Find("Coin").GetComponent<TMP_Text>();
        m_IdleWorkers = transform.Find("Workers").GetComponent<TMP_Text>();
        m_ToolLevel = transform.Find("ToolLevel").GetComponent<TMP_Text>();
        m_Store = transform.Find("Store").gameObject;
        m_FarmList = transform.Find("FarmList").gameObject;
        m_SellBtn = m_Store.transform.Find("SellBtn").GetComponent<Button>();
        m_BuyBtn = m_Store.transform.Find("BuyBtn").GetComponent<Button>();
        m_CloseStoreBtn = m_Store.transform.Find("CloseBtn").GetComponent<Button>();
        m_ExpandFarmBtn = transform.Find("ExpandFarm").GetComponent<Button>();
        m_CloseFarmListBtn = transform.Find("FarmList/CloseBtn").GetComponent<Button>();
        m_EmployBtn = transform.Find("EmployWorker").GetComponent<Button>();
        m_UpgradeBtn = transform.Find("UpgradeTool").GetComponent<Button>();
        m_ExitBtn = transform.Find("Victory/Exit").GetComponent<Button>();
    }
    private void Start()
    {
        RegisterButtonEvent(m_SellBtn, ShowSellList);
        RegisterButtonEvent(m_BuyBtn, ShowBuyList);
        RegisterButtonEvent(m_CloseStoreBtn, CloseMarket);
        RegisterButtonEvent(m_CloseFarmListBtn, CloseLandList);
        RegisterButtonEvent(m_EmployBtn, EmployWorker);
        RegisterButtonEvent(m_UpgradeBtn, UpgradeTool);
        RegisterButtonEvent(m_ExitBtn, ExitGame);
        UpdateInventoryVisual(true);
    }
    public void UpdateCoin()
    {
        m_CoinText.text = GameManager.Instance.Player.Coins.ToString();
        if(GameManager.Instance.Player.Coins >= 10000)
        {
            ShowGameOver();
        }
    }
    public void ShowMarket()
    {
        m_Store.SetActive(true);
        ShowSellList();
    }
    private void ShowSellList()
    {
        if (buyPanel.gameObject.activeSelf)
        {
            buyPanel.gameObject.SetActive(false);
        }
        sellPanel.gameObject.SetActive(true);
        m_SellBtn.enabled = false;
        m_BuyBtn.enabled = true;
        List<InventorySystem.InventoryEntry> sellList = GameManager.Instance.Player.Inventory.GetSellableList();
        bool enough = sellList.Count <= sellPanel.childCount;
        if (!enough)
        {
            int diff= sellList.Count - sellPanel.childCount;
            for (int i = 0; i < diff; i++)
            {
                Instantiate(itemForSell, sellPanel);
            }
        }
        for(int i = 0; i < sellPanel.childCount; i++)
        {
            GameObject newItem = sellPanel.GetChild(i).gameObject;
            newItem.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -60, 0);
            if(i >= sellList.Count)
            {
                newItem.SetActive(false);
                continue;
            }
            newItem.SetActive(true);
            Item item = sellList[i].Item;
            int stack = (int)Math.Floor(sellList[i].StackSize);

            Vector3 pos = newItem.transform.GetComponent<RectTransform>().anchoredPosition;
            newItem.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, pos.y + (-100*i), 0);
            Sprite iconSprite = Resources.Load<Sprite>(sellList[i].Item.IconPath);
            newItem.transform.Find("Bg/Icon").GetComponent<Image>().sprite = iconSprite;
            newItem.transform.Find("Bg/Num").GetComponent<TMP_Text>().text = stack.ToString();
            

            Button buttonSell = newItem.transform.Find("Sell").GetComponent<Button>();
            buttonSell.transform.Find("Price").GetComponent<TMP_Text>().text = item.SellPrice.ToString();
            RegisterButtonEvent(buttonSell, () => SellItem(item, 1));
            Button buttonSellAll = newItem.transform.Find("SellAll").GetComponent<Button>();
            buttonSellAll.transform.Find("Price").GetComponent<TMP_Text>().text = (item.SellPrice * stack).ToString();
            RegisterButtonEvent(buttonSellAll, () => SellItem(item, stack));
        }
    }
    private void RegisterButtonEvent(Button btn, UnityAction func)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(func);
    }
    private void SellItem(Item item, int count)
    {
        GameManager.Instance.Player.Inventory.OnSellItem(item, count);
        UpdateInventoryVisual(true);
        ShowSellList();
    }
    private void ShowBuyList()
    {
        if (sellPanel.gameObject.activeSelf)
        {
            sellPanel.gameObject.SetActive(false);
        }
        buyPanel.gameObject.SetActive(true);
        m_BuyBtn.enabled = false;
        m_SellBtn.enabled = true;
        List<ItemList.RowData> canBuyList = GameManager.Instance.ItemsToBuy();
        bool enough = canBuyList.Count <= buyPanel.childCount;
        if (!enough)
        {
            int diff = canBuyList.Count - buyPanel.childCount;
            for (int i = 0; i < diff; i++)
            {
                Instantiate(itemInstock, buyPanel);
            }
        }
        for (int i = 0; i < buyPanel.childCount; i++)
        {
            GameObject item = buyPanel.GetChild(i).gameObject;
            item.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -60, 0);
            if (i >= canBuyList.Count)
            {
                item.SetActive(false);
                continue;
            }
            item.SetActive(true);
            Vector3 pos = item.transform.GetComponent<RectTransform>().anchoredPosition;
            item.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, pos.y + (-100 * i), 0);

            Sprite iconSprite = Resources.Load<Sprite>(canBuyList[i].IconPath);
            item.transform.Find("Bg/Icon").GetComponent<Image>().sprite = iconSprite;
            CropList.RowData good = GameManager.Instance.GetCropByCropID(canBuyList[i].CropID);
            int growthTime = good.GrowthTime / 60;
            item.transform.Find("Data/HarvestNum/Count").GetComponent<TMP_Text>().text = good.HarvestNum.ToString();
            item.transform.Find("Data/GrowthTime/Count").GetComponent<TMP_Text>().text = string.Format("{0}m", growthTime);
            
            Item newItem = new Item(canBuyList[i].ItemID);
            Button buttonBuy = item.transform.Find("Buy").GetComponent<Button>();
            buttonBuy.gameObject.SetActive(!newItem.WholeSale);
            if (!newItem.WholeSale)
            {
                buttonBuy.transform.Find("Price").GetComponent<TMP_Text>().text = canBuyList[i].BuyPrice.ToString();
                RegisterButtonEvent(buttonBuy, () => BuyItem(newItem, 1));
            }
            Button buttonBuyTen = item.transform.Find("Buy10").GetComponent<Button>();
            buttonBuyTen.transform.Find("Price").GetComponent<TMP_Text>().text = (canBuyList[i].BuyPrice * 10).ToString();
            RegisterButtonEvent(buttonBuyTen, () => BuyItem(newItem, 10));
        }
    }
    private void BuyItem(Item newItem, int count)
    {
        if(GameManager.Instance.Player.CanFitInInventory(newItem, count) && GameManager.Instance.Player.Coins >= newItem.BuyPrice * count)
        {
            for(int i = 0; i < count; i++)
            {
                if (GameManager.Instance.Player.AddItem(newItem))
                {
                    GameManager.Instance.Player.AddCoin(-newItem.BuyPrice);
                }
                UpdateInventoryVisual(true);
            }
        }
    }
    public void UpdateInventoryVisual(bool bForce = true)
    {
        InventorySystem.InventoryEntry[] inventoryList = GameManager.Instance.Player.Inventory.Entries;
        for(int i = 0; i < inventoryList.Length; i++)
        {
            Transform slot = inventoryPanel.GetChild(i);
            if (bForce)
            {
                InventorySystem.InventoryEntry entry = inventoryList[i];
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
    private void CloseMarket()
    {
        m_Store.SetActive(false);
    }
    
    private void BuyFarm()
    {
        if(GameManager.Instance.Player.Coins < 500)
        {
            return;
        }
        GameManager.Instance.Player.AddCoin(-500);
        GameManager.Instance.TerrainMgr.UnlockFields(1);
        //ShowLandList();
    }
    private void CloseLandList()
    {
        m_FarmList.SetActive(false);
    }
    private void EmployWorker()
    {
        if (GameManager.Instance.Player.Coins < 500 && GameManager.Instance.WorkerMgr.Workers.Count < 9)
        {
            return;
        }

        GameManager.Instance.Player.AddCoin(-500);
        GameManager.Instance.WorkerMgr.EmployWorker();
        UpdateIdleWorkers();
    }
    public void UpdateIdleWorkers()
    {
        int idle = GameManager.Instance.WorkerMgr.GetIdleWorkersCount();
        int total = GameManager.Instance.WorkerMgr.Workers.Count;
        m_IdleWorkers.text = idle + "/" + total;
    }
    private void UpgradeTool()
    {
        if (GameManager.Instance.Player.Coins < 500 && GameManager.Instance.Player.ToolLevel < 10)
        {
            return;
        }
        GameManager.Instance.Player.UpgradeTool();
    }
    public void UpdateToolLevel()
    {
        m_ToolLevel.text = string.Format("Lv.{0}", GameManager.Instance.Player.ToolLevel.ToString());
    }
    public void ShowGameOver()
    {
        m_GameOverPanel.gameObject.SetActive(true);
        Time.timeScale = 0;
    }
    private void ExitGame()
    {
        Application.Quit();
    }
}
