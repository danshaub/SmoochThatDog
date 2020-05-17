using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class InGameMenu : MonoBehaviour
{
    public enum State
    {
        UNPAUSED,
        PAUSED,
        CONTROL_MENU,
        LEVEL_INTRO,
        OPTIONS_MENU
    }

    public EventSystem eventSystem;

    [Header("Pause Menu")]
    public GameObject pausePanel;
    public GameObject resumeButton;
    [Header("Control Menu")]
    public GameObject controlsPanel;
    public GameObject controlsBackButton;

    [Header("Level Intro")]
    public GameObject introPanel;
    public GameObject introContinueButton;

    [Header("Options Menu")]
    public GameObject optionsPanel;
    public GameObject optionsBackButton;
    [HideInInspector] public bool paused;

    [HideInInspector] public State state = State.UNPAUSED;
    private void Start()
    {
        OpenLevelIntro();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && CharacterActions.instance.canPause)
        {
            if (paused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }

        if (paused && !(Mathf.Approximately(0f, Input.GetAxis("Mouse X")) || Mathf.Approximately(0f, Input.GetAxis("Mouse Y"))))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Pause()
    {
        SetState(State.PAUSED);
    }

    public void Unpause()
    {
        SetState(State.UNPAUSED);
    }

    public void OpenControlMenu()
    {
        SetState(State.CONTROL_MENU);
    }

    public void OpenLevelIntro()
    {
        SetState(State.LEVEL_INTRO);
    }

    public void OpenOptions()
    {
        SetState(State.OPTIONS_MENU);
    }

    public void Quit()
    {
        SetState(State.UNPAUSED);
        GameManager.instance.LoadSceneByIndex(0);
    }

    private void SetState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.UNPAUSED:
                paused = false;
                Time.timeScale = 1;
                CharacterActions.instance.lockControl = false;
                pausePanel.SetActive(false);
                controlsPanel.SetActive(false);
                introPanel.SetActive(false);
                optionsPanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case State.PAUSED:
                paused = true;
                Time.timeScale = 0;
                CharacterActions.instance.lockControl = true;
                pausePanel.SetActive(true);
                controlsPanel.SetActive(false);
                introPanel.SetActive(false);
                optionsPanel.SetActive(false);
                eventSystem.SetSelectedGameObject(resumeButton);
                break;

            case State.CONTROL_MENU:
                paused = true;
                Time.timeScale = 0;
                CharacterActions.instance.lockControl = true;
                pausePanel.SetActive(false);
                controlsPanel.SetActive(true);
                introPanel.SetActive(false);
                optionsPanel.SetActive(false);
                eventSystem.SetSelectedGameObject(controlsBackButton);
                break;

            case State.LEVEL_INTRO:
                LevelLoader.instance.loadingPanel.SetActive(false);
                paused = true;
                Time.timeScale = 0;
                CharacterActions.instance.lockControl = true;
                pausePanel.SetActive(false);
                controlsPanel.SetActive(false);
                introPanel.SetActive(true);
                optionsPanel.SetActive(false);
                eventSystem.SetSelectedGameObject(introContinueButton);
                break;

            case State.OPTIONS_MENU:
                paused = true;
                Time.timeScale = 0;
                CharacterActions.instance.lockControl = true;
                pausePanel.SetActive(false);
                controlsPanel.SetActive(false);
                introPanel.SetActive(false);
                optionsPanel.SetActive(true);
                optionsPanel.GetComponent<OptionsMenu>().SetValues();
                eventSystem.SetSelectedGameObject(optionsBackButton);
                break;

            default:
                Debug.LogError("Unknown Menu State");
                break;
        }
    }
}
