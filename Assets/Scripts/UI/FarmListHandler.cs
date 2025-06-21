using Farm;
using UnityEngine;
using System;

public class FarmListHandler : UIHandlerBase<FarmListView>
{
    public override void OnShow()
    {
        _panel.ShowLandList();
    }
    protected override void Init()
    {
        _panel.OnBuyFarm += BuyFarm;
    }
    private void BuyFarm()
    {
        if (GameManager.Instance.Player.Coins < 500)
        {
            return;
        }
        GameManager.Instance.Player.AddCoin(-500);
        GameManager.Instance.TerrainMgr.UnlockFields(1);
        _panel.ShowLandList();
    }
}
