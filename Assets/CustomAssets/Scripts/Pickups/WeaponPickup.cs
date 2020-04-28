using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : Pickup
{
    public Gun gun;

    override
    public void TakePickup()
    {
        PlayerManager.instance.AddGun(gun);
    }
}
