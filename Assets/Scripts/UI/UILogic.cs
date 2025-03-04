public interface IGameUI
{
    void UpdateCoin(int score);
    void ShowMarket();
    void UpdateInventoryVisual(bool bForce);
    void ShowLandList();
    void UpdateIdleWorkers();
    void ShowGameOver();
}

public class UIHandler
{
    private IGameUI _gameUI;
    private int coin = 0;

    public UIHandler(IGameUI gameUI)
    {
        _gameUI = gameUI;
    }

    public void AddCoin(int amount)
    {
        coin += amount;
        _gameUI.UpdateCoin(coin);
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
    public void GameOver()
    {
        _gameUI.ShowGameOver();
    }
}
