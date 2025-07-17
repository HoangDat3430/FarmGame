using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

public class MainUIHandler : UIHandlerBase<MainUI>
{
    public override void OnShow()
    {
        RefreshData();
    }
    public override void RefreshData()
    {
        _panel.UpdateCoin(GameManager.Instance.Player.Coins);
        _panel.UpdateWorkers(GameManager.Instance.WorkerMgr.GetIdleWorkersCount());
    }
    protected override void RegisterEvents()
    {
        GameManager.Instance.Player.OnCoinChanged += OnCoinChanged;

        EventBus.Subscribe<WorkerStateChangedEvent>(OnWorkerStateChanged);

    }
    private void OnCoinChanged(int coin)
    {
        _panel.UpdateCoin(coin);
    }
    private void OnWorkerStateChanged(WorkerStateChangedEvent e)
    {
        _panel.UpdateWorkers(GameManager.Instance.WorkerMgr.GetIdleWorkersCount());
    }
}
