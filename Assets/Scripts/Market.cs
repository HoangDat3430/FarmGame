using Farm;

public class Market : InteractiveObject
{
    public override void InteractedWith()
    {
        GameManager.Instance.OpenMarket();
    }
}
