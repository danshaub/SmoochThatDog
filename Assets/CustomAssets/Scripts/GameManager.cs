using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Assets.Pixelation.Scripts;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Camera playerCamera;
    public Camera minimapCamera;
    public Camera uiCamera;

    public AudioMixer mixer;
    public Light[] basicLights;
    public LightManager lightManager;
    public Pixelation mainPixelation;
    public Pixelation minimapPixelation;
    public ShaderEffect_CRT cameraEffect1;
    public ShaderEffect_CorruptedVram cameraEffect2;
    public ShaderEffect_BleedingColors cameraEffect3;
    public ShaderEffect_Tint cameraEffect4;

    private void Start()
    {
        LevelLoader.instance.loadingPanel.SetActive(false);
        ClearCameras();
        RealizeOptions();
    }

    public void ClearCameras()
    {
        playerCamera.clearFlags = CameraClearFlags.Color;
        minimapCamera.clearFlags = CameraClearFlags.Color;
        uiCamera.clearFlags = CameraClearFlags.Color;

        StartCoroutine(ClearCameras2());
    }

    public IEnumerator ClearCameras2()
    {
        yield return new WaitForEndOfFrame();

        playerCamera.clearFlags = CameraClearFlags.Skybox;
        minimapCamera.clearFlags = CameraClearFlags.Color;
        uiCamera.clearFlags = CameraClearFlags.Depth;
    }

    public void RealizeOptions()
    {
        if (mixer != null)
        {
            mixer.SetFloat("MusicVolume", Options.currentOptions.musicVolume);
            mixer.SetFloat("SFXVolume", Options.currentOptions.sfxVolume);
        }

        foreach (Light light in basicLights)
        {
            if (Options.currentOptions.dynamicLighting)
            {
                light.intensity = Options.currentOptions.brightness / 2;
            }
            else
            {
                light.intensity = Options.currentOptions.brightness;
            }
            
        }

        if (lightManager != null)
        {
            if (Options.currentOptions.dynamicLighting)
            {
                lightManager.SetLightLevel(1);
            }
            else
            {
                lightManager.SetLightLevel(0);
            }
        }

        if (mainPixelation != null && minimapPixelation != null)
        {
            mainPixelation.BlockCount = Options.currentOptions.pixelation;
            minimapPixelation.BlockCount = Options.currentOptions.pixelation;
        }

        if (cameraEffect1 != null && cameraEffect2 != null && cameraEffect3 != null && cameraEffect4 != null)
        {
            if (Options.currentOptions.crtEffect)
            {
                cameraEffect1.enabled = true;
                cameraEffect2.enabled = true;
                cameraEffect3.enabled = true;
                cameraEffect4.enabled = true;
            }
            else
            {
                cameraEffect1.enabled = false;
                cameraEffect2.enabled = false;
                cameraEffect3.enabled = false;
                cameraEffect4.enabled = false;
            }
        }

        if (CharacterActions.instance != null)
        {
            CharacterActions.instance.fieldOfView = Options.currentOptions.fov;
            CharacterActions.instance.mouseSensitivity = Options.currentOptions.sensitivity;
        }
    }

    public void LoadSceneByName(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadSceneByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
