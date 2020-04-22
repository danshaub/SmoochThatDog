using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Animator playerAnimation;
    public Gun defaultGun;
    public List<Gun> guns;
    public List<KeyPickup.Key> keys;
    public int currentGunIndex;

    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip weaponPickupSound;
    public AudioClip ammoPickupSound;
    public AudioClip playerHurtSound;
    public AudioClip rageModeSound;
    public AudioClip healthPickupSound;
    public AudioClip armorPickupSound;

    public int rageGauge { get; private set; } = 0;
    public bool isRaged { get; private set; } = false;

    public float rageTime;

    public Text currentAmmoDisplay;
    public Text maxAmmoDisplay;
    public Slider rageBar;
    public Image rageOverlay;
    public List<Image> keySlots;

    private void Awake()
    {
        instance = this;
        AddGun(defaultGun);
        currentGunIndex = 0;
    }

    void Start()
    {
        CurrentGun().ResetAmmo();
        ((Smooch)CurrentGun()).CallStart();
        UpdateAmmoText();
        playerAnimation.runtimeAnimatorController = CurrentGun().animations;
        UpdateRageBar();
    }

    public Gun CurrentGun()
    {
        return guns[currentGunIndex];
    }

    public void UpdateAmmoText()
    {
        if (CurrentGun().unlimitedAmmo)
        {
            currentAmmoDisplay.text = "∞";
            maxAmmoDisplay.text = "∞";
        }
        else
        {
            currentAmmoDisplay.text = CurrentGun().ammoRemaining.ToString();
            maxAmmoDisplay.text = CurrentGun().maxAmmo.ToString();
        }
        
    }

    public void UpdateAnimator()
    {
        playerAnimation.runtimeAnimatorController = CurrentGun().animations;
    }

    public bool HasKey(int keyID)
    {
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (keys[i].keyID == keyID)
            {
                return true;
            }
        }

        return false;
    }
    public void AddKey(KeyPickup.Key key)
    {
        keys.Add(key);
        UpdateKeySlots();
    }
    public void RemoveKey(int keyID)
    {
        for(int i = keys.Count - 1; i >= 0; i--)
        {
            if(keys[i].keyID == keyID)
            {
                keys.RemoveAt(i);
            }
        }
        UpdateKeySlots();
    }
    public void ClearKeys()
    {
        keys.Clear();
        UpdateKeySlots();
    }

    public void UpdateKeySlots()
    {
        foreach(Image img in keySlots)
        {
            img.sprite = null;
            img.color = Color.clear;
        }

        for(int i = 0; i < keySlots.Count && i < keys.Count; i++)
        {
            keySlots[i].sprite = keys[i].image;
            keySlots[i].color = Color.white;
        }
    }

    public void AddGun(Gun gun)
    {
        for (int i = 0; i < guns.Count; i++)
        {
            if (guns[i].name.Equals(gun.name))
            {
                SwapGun(i);
                guns[i].ResetAmmo();
                return;
            }
        }

        gun.ResetAmmo();
        guns.Add(gun);
        SwapGun(guns.Count - 1);
    }

    public void SwapGun(int gunIndex)
    {
        if(gunIndex == currentGunIndex)
        {
            return;
        }
        else if(gunIndex >= guns.Count)
        {
            return;
        }

        currentGunIndex = (int)Mathf.Clamp(gunIndex, 0, guns.Count - 1);

        playerAnimation.SetTrigger("PutAway");
        UpdateAmmoText();
        CharacterActions.instance.currentSpread = 0f;
        CharacterActions.instance.recoilOffset = Vector2.zero;
    }

    public void SwapGun(bool up)
    {
        if(guns.Count == 1)
        {
            return;
        }
        if (up)
        {
            currentGunIndex = (currentGunIndex + 1) % guns.Count;
        }
        else
        {
            currentGunIndex = (currentGunIndex - 1) < 0 ? guns.Count - 1 : currentGunIndex - 1;
        }

        playerAnimation.SetTrigger("PutAway");
        UpdateAmmoText();
        CharacterActions.instance.currentSpread = 0f;
    }

    public IEnumerator ShootCooldown()
    {
        if (CurrentGun().cooldownSound != null && CurrentGun().AmmoRemaining())
        {
            GetComponent<AudioSource>().PlayOneShot(CurrentGun().cooldownSound);
            playerAnimation.SetBool("Cooldown", true);
        }
        if (CurrentGun().fireRate == 0f)
        {
            yield return new WaitForFixedUpdate();
        }
        else
        {
            yield return new WaitForSeconds(1f / CurrentGun().fireRate);
        }

        CharacterActions.instance.canShoot = true;
    }

    public void AddRage(int rageToAdd)
    {
        if (isRaged)
        {
            return;
        }
        rageGauge = (int) Mathf.Clamp(rageGauge + rageToAdd, 0, 1000);
        UpdateRageBar();
    }

    public bool RageFull()
    {
        return (!isRaged && rageGauge == 1000);
    }

    public void UpdateRageBar()
    {

        rageBar.value = rageGauge;
        if (RageFull())
        {
            rageBar.fillRect.gameObject.GetComponent<Image>().color = Color.red;
        }
        else
        {
            rageBar.fillRect.gameObject.GetComponent<Image>().color = Color.blue;
        }
    }

    public IEnumerator Rage()
    {
        isRaged = true;
        rageOverlay.gameObject.SetActive(true);
        rageBar.fillRect.gameObject.GetComponent<Image>().color = Color.magenta;

        yield return new WaitForSeconds(rageTime);

        isRaged = false;
        rageOverlay.gameObject.SetActive(false);
        rageBar.fillRect.gameObject.GetComponent<Image>().color = Color.blue;

        AddRage(-1000);

    }
}
