public interface IGameUI
{
    void UpdateCoin();
    void ShowMarket();
    void UpdateInventoryVisual(bool bForce);
    void UpdateIdleWorkers();
    void UpdateToolLevel();
    void ShowGameOver();
}

public class UIHandler
{
    private IGameUI _gameUI;

    public UIHandler(IGameUI gameUI)
    {
        _gameUI = gameUI;
    }

    public void ShowMarket()
    {
        _gameUI.ShowMarket();
    }
}
