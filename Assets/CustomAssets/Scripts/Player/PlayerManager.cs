﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Camera minimapCamera;
    public Animator gunAnimations;
    public Animator smoochAnimations;
    public Animator faceAnimations;
    public Animator damageAnimations;
    public Gun defaultGun;
    public Smooch smooch;
    public string[] gunNames;
    public List<KeyPickup.Key> keys;
    public int currentGunIndex;

    [Range(0f, 1f)]
    public float maxVignetteOpacity;
    [Range(0f, 1f)]
    public float vignetteOpacityPerHit;
    [Range(0f, 1f)]
    public float vignetteDecreaseRate;
    public int maxHealth = 1000;
    public int currentHealth;
    public int maxArmorDurability;
    public int armorDurability;

    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip playerHurtSound;
    public AudioClip rageModeSound;

    public int rageAmount { get; private set; } = 0;
    public bool isRaged { get; private set; } = false;
    public bool isAlive { get; private set; } = true;
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
    public Image crosshair;
    public Graphic hurtVignette;
    public TMPro.TextMeshPro infoText;

    public GameObject deathText;

    public Gun[] guns;
    private int numGuns = 0;
    private int rageGunStorage = 0;

    private int minimapState = 0;

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
        infoText.enabled = false;
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
        faceAnimations.SetInteger("Health", 100);
        UpdateRageBar();
    }

    public LevelManager.CheckpointData.PlayerData MakeCheckpoint()
    {
        LevelManager.CheckpointData.PlayerData data = new LevelManager.CheckpointData.PlayerData();
        data.worldPosition = transform.position;
        data.localRotation = transform.localEulerAngles;
        data.health = currentHealth;
        data.armor = armorDurability;
        data.maxArmor = maxArmorDurability;
        data.rageAmount = rageAmount;

        data.guns = new LevelManager.CheckpointData.GunData[guns.Length];

        for (int i = 1; i < guns.Length; i++)
        {
            if (guns[i] != null)
            {
                data.guns[i].collected = true;
                data.guns[i].ammoRemaining = guns[i].ammoRemaining;
            }
            else
            {
                data.guns[i].collected = false;
            }
        }

        data.keys = new List<KeyPickup.Key>(keys);

        data.gunIndex = currentGunIndex;

        return data;
    }
    public void LoadCheckpoint(LevelManager.CheckpointData.PlayerData data)
    {
        deathText.SetActive(false);
        isAlive = true;
        GetComponent<CharacterController>().enabled = false;
        StartCoroutine(LockControlTemporarily());
        transform.position = data.worldPosition;
        transform.localEulerAngles = data.localRotation;

        minimapCamera.enabled = false;
        CharacterActions.instance.fpsCamera.enabled = false;
        CharacterActions.instance.fpsCamera.transform.localEulerAngles = Vector3.zero;
        CharacterActions.instance.fpsCamera.enabled = true;
        minimapCamera.enabled = true;

        currentHealth = data.health;
        armorDurability = data.armor;
        maxArmorDurability = data.maxArmor;
        rageAmount = data.rageAmount;
        hurtVignette.color = new Color(1, 1, 1, 0);

        for (int i = 1; i < data.guns.Length; i++)
        {
            if (data.guns[i].collected)
            {
                guns[i].SetAmmo(data.guns[i].ammoRemaining);
            }
            else if (guns[i] != null)
            {
                guns[i] = null;
            }
        }

        ClearKeys();
        keys = new List<KeyPickup.Key>(data.keys);

        SwapGun(data.gunIndex);

        GetComponent<CharacterController>().enabled = true;

        ResetUI();
    }

    public IEnumerator LockControlTemporarily()
    {
        CharacterActions.instance.lockControl = true;
        yield return new WaitForSeconds(0.01f);
        CharacterActions.instance.lockControl = false;
    }
    #region UI Updating
    public void ResetUI()
    {
        UpdateKeySlots();
        UpdateAmmoText();
        UpdateAnimator();
        UpdateHealthText();
        UpdateArmorText();
        UpdateGunTexts();
        UpdateRageBar();
        ResetMinimap();

        faceAnimations.SetTrigger("Reset");
    }

    public void DisplayInfoText(string message, float duration)
    {
        if (infoText.enabled)
        {
            StopCoroutine("HideInfoText");
        }

        infoText.enabled = true;

        infoText.text = message;

        StartCoroutine(HideInfoText(duration));
    }

    private IEnumerator HideInfoText(float duration)
    {
        yield return new WaitForSeconds(duration);

        infoText.enabled = false;
    }

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
        faceAnimations.SetInteger("Health", healthPercentage);
    }

    public void UpdateArmorText()
    {
        int armorPercentage = (int)Mathf.Round((armorDurability / (float)maxArmorDurability) * 100);
        armorDisplay.text = armorPercentage.ToString() + "%";
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

    public void ToggleMinimap()
    {
        minimapState = (minimapState + 1) % 3;
        UpdateMinimap();
    }

    public void ResetMinimap()
    {
        minimapState = 0;
        UpdateMinimap();
    }

    public void UpdateMinimap()
    {
        LayerMask mask;
        switch (minimapState)
        {

            case 0:
                minimapCamera.depth = -2;
                crosshair.enabled = true;
                break;
            case 1:
                minimapCamera.depth = 0;
                mask = LayerMask.GetMask("LevelWireframe", "MinimapElement");
                minimapCamera.cullingMask = mask;
                minimapCamera.clearFlags = CameraClearFlags.Nothing;
                crosshair.enabled = false;
                break;
            case 2:
                minimapCamera.depth = 0;
                mask = LayerMask.GetMask("FloorWall", "MinimapElement");
                minimapCamera.cullingMask = mask;
                minimapCamera.clearFlags = CameraClearFlags.Color;
                crosshair.enabled = false;
                break;
            default:
                Debug.LogError("Unknown minimap state");
                break;
        }
    }

    #endregion

    #region Keys
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
        rageAmount = (int)Mathf.Clamp(rageAmount + rageToAdd, 0, 1000);
        UpdateRageBar();
    }

    public bool RageFull()
    {
        return (!isRaged && rageAmount == 1000);
    }

    public void UpdateRageBar()
    {

        rageBar.value = rageAmount;
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
        hurtVignette.color = Color.Lerp(hurtVignette.color, new Color(1, 1, 1, maxVignetteOpacity), vignetteOpacityPerHit);
        if (armorDurability == 0)
        {
            currentHealth = Mathf.Clamp(currentHealth - baseDamage, 0, maxHealth);
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth - (int)Mathf.Round(baseDamage / 3f), 0, maxHealth);
            armorDurability = Mathf.Clamp(armorDurability - baseDamage, 0, maxArmorDurability);
        }

        GetComponent<AudioSource>().PlayOneShot(playerHurtSound);
        UpdateArmorText();
        UpdateHealthText();

        if (currentHealth == 0)
        {
            KillPlayer();
        }
    }

    private void Update()
    {
        hurtVignette.color = Color.Lerp(hurtVignette.color, new Color(1, 1, 1, 0f), vignetteDecreaseRate * Time.deltaTime);
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
        CharacterActions.instance.lockControl = true;
        isAlive = false;
        deathText.SetActive(true);
        StartCoroutine(Death());
    }

    public IEnumerator Death()
    {
        yield return new WaitForSeconds(2f);
        LevelManager.instance.LoadCheckpoint();
    }

    #endregion

    #endregion
}
