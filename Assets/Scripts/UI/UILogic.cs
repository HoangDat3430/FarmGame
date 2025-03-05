public interface IGameUI
{
    void UpdateCoin();
    void ShowMarket();
    void UpdateInventoryVisual(bool bForce);
    void ShowLandList();
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

    public void UpdateCoin()
    {
        _gameUI.UpdateCoin();
    }
    public void ShowMarket()
    {
        _gameUI.ShowMarket();
    }
    public void ShowLandList()
    {
        _gameUI.ShowLandList();
    }
    public void UpdateInventoryVisual(bool bForce)
    {
        _gameUI.UpdateInventoryVisual(bForce);
    }
    public void UpdateIdleWorkers()
    {
        _gameUI.UpdateIdleWorkers();
    }
    public void UpdateToolLevel()
    {
        _gameUI.UpdateToolLevel();
    }
    public void GameOver()
    {
        _gameUI.ShowGameOver();
    }
}
