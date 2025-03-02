public interface IGameUI
{
    void UpdateCoin(int score);
    void ShowMarket();
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
        _gameUI.UpdateCoin(score);  // Gọi UI thông qua Interface
    }
    public void ShowMarket()
    {
        _gameUI.ShowMarket();
    }
    public void GameOver()
    {
        _gameUI.ShowGameOver();
    }
}
