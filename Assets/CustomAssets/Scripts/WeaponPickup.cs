using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Gun gun;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.audio.PlayOneShot(PlayerManager.instance.weaponPickupSound);
            PlayerManager.instance.AddGun(gun);
            Destroy(gameObject);
        }
    }
}
