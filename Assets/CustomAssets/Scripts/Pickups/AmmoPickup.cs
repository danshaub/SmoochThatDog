﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : Pickup
{
    public bool maxAmmo;
    public int ammoReturned;
    public List<string> gunsEffected;

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.instance.currentGunIndex == 0)
        {
            ((Smooch)PlayerManager.instance.CurrentGun()).RemoveFromTargetList(gameObject);
        }
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(PlayerManager.instance.ammoPickupSound);
            for(int i = 0; i < PlayerManager.instance.guns.Count; i++)
            {
                if (gunsEffected.Contains(PlayerManager.instance.guns[i].gunName)){
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
            Destroy(gameObject);
        }
    }
}
