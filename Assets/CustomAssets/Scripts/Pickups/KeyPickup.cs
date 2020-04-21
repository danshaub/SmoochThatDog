using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : Pickup
{
    public int keyID;

    override
    public void TakePickup()
    {
        PlayerManager.instance.AddKey(keyID);
    }
}
