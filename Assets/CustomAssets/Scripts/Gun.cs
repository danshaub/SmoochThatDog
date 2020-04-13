using UnityEngine;

public class Gun : MonoBehaviour
{
    public RuntimeAnimatorController animations;
    public string gunName;

    public bool automatic;
    public bool unlimitedAmmo;
    public bool spreadOverTime;
    public int maxAmmo;

    public int bulletsPerShot;
    public int damagePerBullet;
    public float spreadRate;
    public float maxBulletSpread;
    public float fireRate;
    public float range;
    public float recoilStrength;
    public GameObject hitParticlePrefab;
    public AudioClip shootSound;
    public AudioClip emptyClipSound;
    public AudioClip weaponPulloutSound;

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

    public void Hit(RaycastHit hit)
    {
        hit.transform.GetComponent<Target>().Hit(damagePerBullet);
    }
}
