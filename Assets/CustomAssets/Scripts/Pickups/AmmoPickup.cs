using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : Pickup
{
    public bool maxAmmo;
    public int ammoReturned;
    public List<string> gunsEffected;

    override
    public void TakePickup()
    {
        for (int i = 0; i < PlayerManager.instance.guns.Length; i++)
        {
            if (PlayerManager.instance.guns[i] != null && gunsEffected.Contains(PlayerManager.instance.gunNames[i]))
            {
                if (maxAmmo)
                {
                    PlayerManager.instance.guns[i].ResetAmmo();
                }
                else
                {
                    PlayerManager.instance.guns[i].AddAmmo(ammoReturned);
                }
            }
        }
        PlayerManager.instance.UpdateAmmoText();
    }
}
