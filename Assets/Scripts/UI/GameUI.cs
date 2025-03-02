using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Farm;
using UnityEngine.Events;
using System.Linq;

public class GameUI : MonoBehaviour, IGameUI
{
    public GameObject itemForSell;
    public GameObject itemInstock;
    public Transform sellPanel;
    public Transform buyPanel;
    public Transform inventoryPanel;

    private TMP_Text m_CoinText;
    private GameObject m_Store;
    private GameObject m_GameOverPanel;

    private Button m_SellBtn;
    private Button m_BuyBtn;
    private Button m_CloseBtn;

    private void Awake()
    {
        m_CoinText = transform.Find("Coin").GetComponent<TMP_Text>();
        m_Store = transform.Find("Store").gameObject;
        m_SellBtn = m_Store.transform.Find("SellBtn").GetComponent<Button>();
        m_BuyBtn = m_Store.transform.Find("BuyBtn").GetComponent<Button>();
        m_CloseBtn = m_Store.transform.Find("CloseBtn").GetComponent<Button>();
    }
    private void Start()
    {
        RegisterButtonEvent(m_SellBtn, ShowSellList);
        RegisterButtonEvent(m_BuyBtn, ShowBuyList);
        RegisterButtonEvent(m_CloseBtn, CloseMarket);
        UpdateInventoryVisual(true);
    }
    public void UpdateCoin(int coin)
    {
        m_CoinText.text = coin.ToString();
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
            newItem.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -50, 0);
            if(i >= sellList.Count)
            {
                newItem.SetActive(false);
                continue;
            }
            newItem.SetActive(true);
            Vector3 pos = newItem.transform.GetComponent<RectTransform>().anchoredPosition;
            newItem.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, pos.y + (-70*i), 0);
            Sprite iconSprite = Resources.Load<Sprite>(sellList[i].Item.IconPath);
            newItem.transform.Find("Bg/Icon").GetComponent<Image>().sprite = iconSprite;
            newItem.transform.Find("Bg/Num").GetComponent<TMP_Text>().text = sellList[i].StackSize.ToString();

            Item item = sellList[i].Item;
            int stack = sellList[i].StackSize;

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
            item.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -50, 0);
            if (i >= canBuyList.Count)
            {
                item.SetActive(false);
                continue;
            }
            item.SetActive(true);
            Vector3 pos = item.transform.GetComponent<RectTransform>().anchoredPosition;
            item.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, pos.y + (-70 * i), 0);

            Sprite iconSprite = Resources.Load<Sprite>(canBuyList[i].IconPath);
            item.transform.Find("Bg/Icon").GetComponent<Image>().sprite = iconSprite;
            CropList.RowData good = GameManager.Instance.GetCropByID(canBuyList[i].CropID);
            item.transform.Find("Data/HarvestNum/Count").GetComponent<TMP_Text>().text = good.HarvestNum.ToString();
            item.transform.Find("Data/GrowthTime/Count").GetComponent<TMP_Text>().text = good.GrowthTime.ToString();

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
        if(GameManager.Instance.Player.CanFitInInventory(newItem, count))
        {
            for(int i = 0; i < count; i++)
            {
                if (GameManager.Instance.Coin >= newItem.BuyPrice && GameManager.Instance.Player.AddItem(newItem))
                {
                    GameManager.Instance.AddCoin(-newItem.BuyPrice);
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
                    if (entry.StackSize > 1)
                    {
                        text = entry.StackSize.ToString();
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
    public void ShowGameOver()
    {
        m_GameOverPanel.SetActive(true);
    }
}
