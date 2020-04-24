using System.Collections;
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

    virtual
    public void Shoot(Transform origin)
    {
        if (AmmoRemaining())
        {
            if (!spreadOverTime)
            {
                CharacterActions.instance.currentSpread = maxBulletSpread;
            }
            else
            {
                CharacterActions.instance.currentSpread = Mathf.Lerp(CharacterActions.instance.currentSpread, maxBulletSpread, spreadRate);
            }

            if (CharacterActions.instance.isGrounded)
            {
                CharacterActions.instance.knockbackOffset += (-CharacterActions.instance.transform.forward.normalized * groundedKnockback);
            }
            else
            {
                CharacterActions.instance.knockbackOffset += ((-CharacterActions.instance.transform.forward.normalized - (.25f * CharacterActions.instance.fpsPosition.forward)).normalized * airborneKnockback);
            }


            UseAmmo();
            PlayerManager.instance.UpdateAmmoText();

            PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(shootSound);

            PlayerManager.instance.gunAnimations.SetTrigger("Shoot");
            for (int i = 0; i < bulletsPerShot; i++)
            {
                CharacterActions.instance.recoilOffset.y = Mathf.Clamp(CharacterActions.instance.recoilOffset.y + verticalRecoilStrength, 0f, maxVerticalRecoil);
                float horizRecoil = Random.Range(-horizontalRecoilStrength, horizontalRecoilStrength);
                CharacterActions.instance.recoilOffset.x = Mathf.Clamp(CharacterActions.instance.recoilOffset.x + horizRecoil, -maxHorizontalRecoil, maxHorizontalRecoil);



                Vector3 raycastDirection = origin.forward;

                Vector3 spreadOffset = new Vector3
                {
                    x = Random.Range(-1f, 1f),
                    y = Random.Range(-1f, 1f),
                    z = Random.Range(-1f, 1f)
                };

                spreadOffset = spreadOffset.normalized * Random.Range(0f, CharacterActions.instance.currentSpread / 100);

                raycastDirection = (spreadOffset + raycastDirection).normalized;

                RaycastHit hit;
                if (Physics.Raycast(origin.position, raycastDirection, out hit, range))
                {
                    if (hit.transform.GetComponent<Target>() != null)
                    {
                        Hit(hit);
                    }
                    else
                    {
                        if(hitParticlePrefab != null)
                        {
                            GameObject particles = Instantiate(hitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
                            Destroy(particles, 1f);
                        }
                        
                    }
                }

                //Debug.DrawRay(CharacterActions.instance.fpsCamera.transform.position, raycastDirection * range, Color.black, 1f);
                /*
                Vector3 currentAngles = transform.localEulerAngles;
                currentAngles.y = horizontalAngle;
                transform.localEulerAngles = currentAngles;
                */
            }
        }
        else
        {
            PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(emptyClipSound);
        }
    }

    
    virtual
    public void Hit(RaycastHit hit)
    {
        hit.transform.GetComponent<Target>().Hit(damagePerBullet);
    }
}
