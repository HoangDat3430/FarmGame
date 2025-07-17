using Farm;

public class FarmLock : InteractiveObject
{
    public override void InteractedWith()
    {
        UIManager.Instance.ShowUI<FarmListView>();
    }
}
