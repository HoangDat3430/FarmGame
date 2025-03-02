using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Farm;
using Unity.Mathematics;
using System;
using UnityEngine.Events;

public class GameUI : MonoBehaviour, IGameUI
{
    public GameObject itemPrefab;
    public Transform contentPanel;

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
        m_SellBtn.onClick.AddListener(ShowSellList);
        m_BuyBtn.onClick.AddListener(ShowBuyList);
        m_CloseBtn.onClick.AddListener(CloseMarket);
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
        List<InventorySystem.InventoryEntry> sellList = GameManager.Instance.Player.Inventory.GetSellableList();
        bool enough = sellList.Count <= contentPanel.childCount;
        if (!enough)
        {
            int diff= sellList.Count - contentPanel.childCount;
            for (int i = 0; i < diff; i++)
            {
                Instantiate(itemPrefab, contentPanel);
            }
        }
        for(int i = 0; i < contentPanel.childCount; i++)
        {
            GameObject newItem = contentPanel.GetChild(i).gameObject;
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

            Button buttonBuy = newItem.transform.Find("Sell").GetComponent<Button>();
            buttonBuy.transform.Find("Price").GetComponent<TMP_Text>().text = item.SellPrice.ToString();
            RegisterButtonEvent(buttonBuy, () => SellItem(item, 1));
            Button buttonBuyAll = newItem.transform.Find("SellAll").GetComponent<Button>();
            buttonBuyAll.transform.Find("Price").GetComponent<TMP_Text>().text = (item.SellPrice * stack).ToString();
            RegisterButtonEvent(buttonBuyAll, () => SellItem(item, stack));
        }
    }
    private void RegisterButtonEvent(Button btn, UnityAction func)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(func);
    }
    private void SellItem(Item item, int count)
    {
        GameManager.Instance.Player.Inventory.SellItem(item, count);
        ShowSellList();
    }
    private void ShowBuyList()
    {
        Debug.LogError("BuyPanel");
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
