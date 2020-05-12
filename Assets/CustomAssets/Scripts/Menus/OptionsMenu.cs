using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OptionsMenu : MonoBehaviour
{
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider brightnessSlider;
    public Toggle dynamicLightsToggle;
    public Slider pixelationSlider;
    public Toggle crtEffectToggle;
    public Slider fovSlider;
    public Slider sensitivitySlider;

    public void SetValues()
    {
        musicVolumeSlider.value = Options.currentOptions.musicVolume;
        sfxVolumeSlider.value = Options.currentOptions.sfxVolume;
        brightnessSlider.value = Options.currentOptions.brightness;
        dynamicLightsToggle.isOn = Options.currentOptions.dynamicLighting;
        pixelationSlider.value = 600f - Options.currentOptions.pixelation;
        crtEffectToggle.isOn = Options.currentOptions.crtEffect;
        fovSlider.value = Options.currentOptions.fov;
        sensitivitySlider.value = Options.currentOptions.sensitivity;
    }

    public void ChangeMusicVolume()
    {
        Options.currentOptions.musicVolume = musicVolumeSlider.value;
        GameManager.instance.RealizeOptions();
    }

    public void ChangeSFXVolume()
    {
        Options.currentOptions.sfxVolume = sfxVolumeSlider.value;
        GameManager.instance.RealizeOptions();
    }

    public void ChangeBrightness()
    {
        Options.currentOptions.brightness = brightnessSlider.value;
        GameManager.instance.RealizeOptions();
    }

    public void ChangeDynamicLightToggle()
    {
        Options.currentOptions.dynamicLighting = dynamicLightsToggle.isOn;
        GameManager.instance.RealizeOptions();
    }

    public void ChangePixelation()
    {
        Options.currentOptions.pixelation = 600f - pixelationSlider.value;
        GameManager.instance.RealizeOptions();
    }

    public void ChangeCRTEffect()
    {
        Options.currentOptions.crtEffect = crtEffectToggle.isOn;
        GameManager.instance.RealizeOptions();
    }

    public void ChangeFOV()
    {
        Options.currentOptions.fov = fovSlider.value;
        GameManager.instance.RealizeOptions();
    }

    public void ChangeSensitivity()
    {
        Options.currentOptions.sensitivity = sensitivitySlider.value;
        GameManager.instance.RealizeOptions();
    }
}

