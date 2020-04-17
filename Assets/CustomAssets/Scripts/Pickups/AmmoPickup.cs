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
        PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(PlayerManager.instance.ammoPickupSound);
        for (int i = 0; i < PlayerManager.instance.guns.Count; i++)
        {
            if (gunsEffected.Contains(PlayerManager.instance.guns[i].gunName))
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
