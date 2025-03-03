using System.Collections;
using System.Collections.Generic;
using Farm;
using UnityEngine;

public class FarmLock : InteractiveObject
{
    public override void InteractedWith()
    {
        if(GameManager.Instance.Coin >= 500)
        {
            GameManager.Instance.Terrain.UnlockFields(1);
            GameManager.Instance.AddCoin(-500);
        }
    }
}
