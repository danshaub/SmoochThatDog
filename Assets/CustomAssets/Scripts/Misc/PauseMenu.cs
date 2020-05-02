using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class PauseMenu : MonoBehaviour
{
    public enum State
    {
        UNPAUSED,
        PAUSED,
        CONTROLMENU
    }

    public GameObject menuPannel;
    public GameObject controlsPannel;
    public GameObject backButton;
    public EventSystem eventSystem;
    [HideInInspector] public bool paused;

    public State state = State.UNPAUSED;
    private void Start()
    {
        menuPannel.SetActive(false);
        controlsPannel.SetActive(false);
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
        SetState(State.CONTROLMENU);
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
                menuPannel.SetActive(false);
                controlsPannel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case State.PAUSED:
                paused = true;
                Time.timeScale = 0;
                CharacterActions.instance.lockControl = true;
                menuPannel.SetActive(true);
                controlsPannel.SetActive(false);
                eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
                break;

            case State.CONTROLMENU:
                paused = true;
                Time.timeScale = 0;
                CharacterActions.instance.lockControl = true;
                menuPannel.SetActive(false);
                controlsPannel.SetActive(true);
                eventSystem.SetSelectedGameObject(backButton);
                break;

            default:
                Debug.LogError("Unknown Menu State");
                break;
        }
    }
}
