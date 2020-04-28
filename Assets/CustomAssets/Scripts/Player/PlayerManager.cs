using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Animator gunAnimations;
    public Animator smoochAnimations;
    public Animator faceAnimations;
    public Animator damageAnimations;
    public Gun defaultGun;
    public Smooch smooch;
    public string[] gunNames;
    public List<KeyPickup.Key> keys;
    public int currentGunIndex;

    public int maxHealth = 1000;
    public int currentHealth;
    public int maxArmorDurability;
    public int armorDurability;

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

    [Header("UI Elements")]
    public Text currentAmmoDisplay;
    public Text maxAmmoDisplay;
    public Text healthDisplay;
    public Text armorDisplay;
    public Slider rageBar;
    public Image rageOverlay;
    public List<Image> keySlots;
    public Text[] gunTexts;
    public Image[] gunTextBackgrounds;
    public string undiscoveredGunText;
    public Color activeGunHighlight;
    public Color inactiveGun;
    public Color emptyGunSlot;

    [HideInInspector] public Gun[] guns;
    private int numGuns = 0;
    private int rageGunStorage = 0;

    #region Methods
    private void Awake()
    {
        instance = this;
        guns = new Gun[gunNames.Length];
        AddGun(defaultGun);
        currentGunIndex = 0;
    }

    void Start()
    {
        currentHealth = maxHealth;
        armorDurability = 0;
        maxArmorDurability = 1;
        UpdateArmorText();
        UpdateHealthText();
        CurrentGun().ResetAmmo();
        smooch.CallStart();
        UpdateAmmoText();
        gunAnimations.runtimeAnimatorController = CurrentGun().animations;
        damageAnimations.SetInteger("Health", 100);
        UpdateRageBar();
    }

    #region UI Updating
    public void UpdateKeySlots()
    {
        foreach (Image img in keySlots)
        {
            img.sprite = null;
            img.color = Color.clear;
        }

        for (int i = 0; i < keySlots.Count && i < keys.Count; i++)
        {
            keySlots[i].sprite = keys[i].image;
            keySlots[i].color = Color.white;
        }
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
        gunAnimations.runtimeAnimatorController = CurrentGun().animations;
    }

    public void UpdateHealthText()
    {
        int healthPercentage = (int)Mathf.Round((currentHealth / (float)maxHealth) * 100);
        healthDisplay.text = healthPercentage.ToString() + "%";
        damageAnimations.SetInteger("Health", healthPercentage);
    }

    public void UpdateArmorText()
    {
        int armorPercentage = (int)Mathf.Round((armorDurability / (float)maxArmorDurability) * 100);
        armorDisplay.text = armorPercentage.ToString() + "%";
    }

    #endregion

    #region  Keys
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
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (keys[i].keyID == keyID)
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

    #endregion

    #region Guns
    public Gun CurrentGun()
    {
        return guns[currentGunIndex];
    }

    public void AddGun(Gun gun)
    {
        for (int i = 0; i < gunNames.Length; i++)
        {
            if (gun.gunName.Equals(gunNames[i]))
            {
                if (guns[i] == null)
                {
                    guns[i] = gun;
                    numGuns++;
                }

                guns[i].ResetAmmo();
                if (currentGunIndex != i)
                {

                    if (isRaged)
                    {
                        rageGunStorage = i;
                    }
                    else
                    {
                        SwapGun(i);
                    }
                }
            }
        }
        UpdateGunTexts();
    }

    public void UpdateGunTexts()
    {
        for (int i = 0; i < gunNames.Length; i++)
        {
            if (guns[i] != null)
            {
                gunTexts[i].text = "[" + ((i + 1) % 10).ToString() + "] " + gunNames[i];
                if (i == currentGunIndex)
                {
                    gunTextBackgrounds[i].color = activeGunHighlight;
                }
                else
                {
                    gunTextBackgrounds[i].color = inactiveGun;
                }
            }
            else
            {
                gunTextBackgrounds[i].color = emptyGunSlot;
                gunTexts[i].text = "[" + ((i + 1) % 10).ToString() + "] " + undiscoveredGunText;
            }
        }
    }

    public void SwapGun(int gunIndex)
    {
        if (gunIndex == currentGunIndex || gunIndex >= gunNames.Length || gunIndex < 0 || guns[gunIndex] == null)
        {
            return;
        }

        currentGunIndex = gunIndex;

        gunAnimations.SetTrigger("PutAway");
        UpdateAmmoText();
        CharacterActions.instance.currentSpread = 0f;
        CharacterActions.instance.recoilOffset = Vector2.zero;

        UpdateGunTexts();
    }

    public void SwapGun(bool up)
    {
        if (numGuns == 1)
        {
            return;
        }
        if (up)
        {
            do
            {
                currentGunIndex = (currentGunIndex + 1) % guns.Length;
            } while (guns[currentGunIndex] == null);

        }
        else
        {
            do
            {
                currentGunIndex = (currentGunIndex - 1) < 0 ? guns.Length - 1 : currentGunIndex - 1;
            } while (guns[currentGunIndex] == null);

        }

        gunAnimations.SetTrigger("PutAway");
        UpdateAmmoText();
        CharacterActions.instance.currentSpread = 0f;

        UpdateGunTexts();
    }

    public IEnumerator ShootCooldown()
    {
        if (CurrentGun().cooldownSound != null && CurrentGun().AmmoRemaining())
        {
            GetComponent<AudioSource>().PlayOneShot(CurrentGun().cooldownSound);
            gunAnimations.SetBool("Cooldown", true);
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

    #endregion

    #region Smooch and Rage
    public IEnumerator SmoochCooldown()
    {
        if (smooch.cooldownSound != null && smooch.AmmoRemaining())
        {
            GetComponent<AudioSource>().PlayOneShot(smooch.cooldownSound);
            smoochAnimations.SetBool("Cooldown", true);
        }
        if (smooch.fireRate == 0f)
        {
            yield return new WaitForFixedUpdate();
        }
        else
        {
            yield return new WaitForSeconds(1f / smooch.fireRate);
        }

        CharacterActions.instance.canShoot = true;
    }

    public void AddRage(int rageToAdd)
    {
        if (isRaged)
        {
            return;
        }
        rageGauge = (int)Mathf.Clamp(rageGauge + rageToAdd, 0, 1000);
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
        faceAnimations.SetTrigger("StartRage");
        rageGunStorage = currentGunIndex;
        if (rageGunStorage != 0)
        {
            SwapGun(0);
        }

        isRaged = true;
        rageOverlay.gameObject.SetActive(true);
        rageBar.fillRect.gameObject.GetComponent<Image>().color = Color.magenta;

        yield return new WaitForSeconds(rageTime);

        isRaged = false;
        rageOverlay.gameObject.SetActive(false);
        rageBar.fillRect.gameObject.GetComponent<Image>().color = Color.blue;

        SwapGun(rageGunStorage);
        AddRage(-1000);
        faceAnimations.SetTrigger("EndRage");
    }

    #endregion

    #region Health and Armor

    public void HurtPlayer(int baseDamage)
    {
        if (armorDurability == 0)
        {
            currentHealth = Mathf.Clamp(currentHealth - baseDamage, 0, maxHealth);
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth - (int)Mathf.Round(baseDamage / 3f), 0, maxHealth);
            armorDurability = Mathf.Clamp(armorDurability - baseDamage, 0, maxArmorDurability);
        }

        UpdateArmorText();
        UpdateHealthText();

        if (currentHealth == 0)
        {
            KillPlayer();
        }
    }

    public void AddHealth(int healthAdded)
    {
        currentHealth = Mathf.Clamp(currentHealth + healthAdded, 0, maxHealth);
        UpdateHealthText();
    }

    public void AddArmor(int armorAdded, int newMaxArmor)
    {
        maxArmorDurability = newMaxArmor > maxArmorDurability ? newMaxArmor : maxArmorDurability;
        armorDurability = Mathf.Clamp(armorDurability + armorAdded, 0, maxArmorDurability);
        UpdateArmorText();

    }

    public void KillPlayer()
    {
        GameManager.instance.ReloadCurrentScene();
    }

    #endregion

    #endregion
}
