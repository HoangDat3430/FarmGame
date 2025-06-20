using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

public class MainUIHandler : UIHandlerBase<MainUI>
{
    private MainUI _mainUI;
    public override void AttachPanel(MainUI panel)
    {
        _mainUI = panel;
        RegisterEvents();
    }
    public override void OnShow()
    {
        RefreshData();
    }
    public override void RefreshData()
    {
        _mainUI.UpdateCoin(GameManager.Instance.Player.Coins);
    }
    protected override void RegisterEvents()
    {
        GameManager.Instance.Player.OnCoinChanged += OnCoinChanged;
    }
    private void OnCoinChanged(int coin)
    {
        _mainUI.UpdateCoin(coin);
    }
}
