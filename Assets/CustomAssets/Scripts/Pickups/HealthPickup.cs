using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
    public bool maxHealth;
    public int health;

    override public void TakePickup()
    {
        PlayerManager.instance.AddHealth(maxHealth ? PlayerManager.instance.maxHealth : health);
    }
}
