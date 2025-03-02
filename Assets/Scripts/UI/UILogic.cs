public interface IGameUI
{
    void UpdateCoin(int score);
    void ShowMarket();
    void UpdateInventoryVisual(bool bForce);
    void ShowGameOver();
}

public class UIHandler
{
    private IGameUI _gameUI;
    private int score = 0;

    public UIHandler(IGameUI gameUI)
    {
        _gameUI = gameUI;
    }

    public void AddCoin(int amount)
    {
        score += amount;
        _gameUI.UpdateCoin(score);
    }
    public void ShowMarket()
    {
        _gameUI.ShowMarket();
    }
    public void UpdateInventoryVisual(bool bForce)
    {
        _gameUI.UpdateInventoryVisual(bForce);
    }
    public void GameOver()
    {
        _gameUI.ShowGameOver();
    }
}
