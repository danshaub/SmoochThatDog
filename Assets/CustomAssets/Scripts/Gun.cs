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
    [Range(0f, 1f)]
    public float spreadRate;
    [Range(0f, 180f)]
    public float maxBulletSpread;
    [Range(0f, 50f)]
    public float fireRate;
    [Range(0f, 100f)]
    public float range;
    [Range(0f, 30f)]
    public float verticalRecoilStrength;
    [Range(0f, 90f)]
    public float maxVerticalRecoil;
    [Range(0f, 30f)]
    public float horizontalRecoilStrength;
    [Range(0f, 90f)]
    public float maxHorizontalRecoil;
    [Range(0f, 1f)]
    public float recoilResistance = 0.5f;
    public float groundedKnockback;
    public float airborneKnockback;
    
    public GameObject hitParticlePrefab;
    public AudioClip shootSound;
    public AudioClip cooldownSound;
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
