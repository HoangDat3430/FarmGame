using Farm;

public class FarmLock : InteractiveObject
{
    public override void InteractedWith()
    {
        GameManager.Instance.OpenFarmStore();
    }
}
