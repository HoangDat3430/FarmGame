using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

public class FarmLock : InteractiveObject
{
    public override void InteractedWith()
    {
        GameManager.Instance.OpenFarmStore();
    }
}
