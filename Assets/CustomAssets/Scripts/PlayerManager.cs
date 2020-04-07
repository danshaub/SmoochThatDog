using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Animator playerAnimation;
    [SerializeField] private Gun defaultGun;
    public List<Gun> guns;
    public int currentGunIndex;

    public AudioSource audio;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip weaponPickupSound;
    public AudioClip ammoPickupSound;


    public Text ammoDisplay;

    private void Awake()
    {
        instance = this;
        AddGun(defaultGun);
        currentGunIndex = 0;
    }

    void Start()
    {
        CurrentGun().ResetAmmo();
        UpdateAmmoText();
        playerAnimation.runtimeAnimatorController = CurrentGun().animations;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Gun CurrentGun()
    {
        return guns[currentGunIndex];
    }

    public void UpdateAmmoText()
    {
        if (CurrentGun().unlimitedAmmo)
        {
            ammoDisplay.text = CurrentGun().name;
        }
        else
        {
            ammoDisplay.text = CurrentGun().gunName + "\n" + CurrentGun().ammoRemaining.ToString() + "/" + CurrentGun().maxAmmo.ToString();
        }
        
    }

    public void UpdateAnimator()
    {
        playerAnimation.runtimeAnimatorController = CurrentGun().animations;
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
        currentGunIndex = (int)Mathf.Clamp(gunIndex, 0, guns.Count - 1);
        UpdateAmmoText();
        UpdateAnimator();
    }

    public void SwapGun(bool up)
    {
        if (up)
        {
            currentGunIndex = (currentGunIndex + 1) % guns.Count;
        }
        else
        {
            currentGunIndex = (currentGunIndex - 1) < 0 ? guns.Count - 1 : currentGunIndex - 1;
        }

        UpdateAmmoText();
        UpdateAnimator();
    }
}
