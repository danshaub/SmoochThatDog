using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPickup : Pickup
{
    public int armorToAdd;

    override public void TakePickup()
    {
        PlayerManager.instance.AddArmor(armorToAdd, armorToAdd);
    }
}
