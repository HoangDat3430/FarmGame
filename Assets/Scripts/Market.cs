using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

public class Market : InteractiveObject
{
    public override void InteractedWith()
    {
        GameManager.Instance.OpenMarket();
    }
}
