using UnityEngine;

public class Gun : MonoBehaviour
{
    public RuntimeAnimatorController animations;
    public string gunName;

    public bool automatic;
    public bool unlimitedAmmo;
    public int maxAmmo;

    public int bulletsPerShot;
    public int damagePerBullet;
    public float bulletSpread;
    public float fireRate;
    public float range;
    public GameObject hitParticlePrefab;
    public AudioClip shootSound;
    public AudioClip emptyClipSound;

    [HideInInspector] public int ammoRemaining { get; protected set; }

    public bool AmmoRemaining()
    {
        return ammoRemaining > 0 || unlimitedAmmo;
    }

    public bool AmmoFull()
    {
        return ammoRemaining == maxAmmo;
    }

    public void ResetAmmo()
    {
        ammoRemaining = maxAmmo;
    }

    public void UseAmmo()
    {
        ammoRemaining--;
    }

    public void AddAmmo(int bullets)
    {
        ammoRemaining = (int)Mathf.Clamp(ammoRemaining + bullets, 0, maxAmmo);
    }
}
