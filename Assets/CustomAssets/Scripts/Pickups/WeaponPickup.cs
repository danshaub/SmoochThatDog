using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Gun gun;

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.instance.currentGunIndex == 0)
        {

            ((Smooch)PlayerManager.instance.CurrentGun()).RemoveFromTargetList(gameObject);
        }
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(PlayerManager.instance.weaponPickupSound);
            PlayerManager.instance.AddGun(gun);
            Destroy(gameObject);
        }
    }
}
