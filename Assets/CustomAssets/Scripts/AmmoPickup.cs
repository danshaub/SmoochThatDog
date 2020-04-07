using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public bool maxAmmo;
    public int ammoReturned;
    public List<string> gunsEffected;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.audio.PlayOneShot(PlayerManager.instance.ammoPickupSound);
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
