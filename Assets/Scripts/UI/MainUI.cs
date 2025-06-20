using System.Collections;
using System.Collections.Generic;
using Farm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : UIPanelBase<MainUI, MainUIHandler>
{
    public TMP_Text Coin;
    public TMP_Text Workers;
    public TMP_Text ToolsLevel;

    public Button ExpandFarmBtn;
    public Button EmployWorkerBtn;
    public Button UpgradeToolBtn;
    protected override void RegisterInternalEvents()
    {
        ExpandFarmBtn.onClick.AddListener(() => { UIManager.Instance.ShowUI<FarmListView>(); });
    }
    public void UpdateCoin(int coin)
    {
        Coin.text = coin.ToString();
    }
}