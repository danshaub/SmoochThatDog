using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public enum State
    {
        DEFAULT,
        CONTROL_MENU
    }

    public State state = State.DEFAULT;
    public GameObject mainMenu;
    public GameObject controlMenu;
    public GameObject backButton;
    public EventSystem eventSystem;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetState(State.DEFAULT);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == State.DEFAULT)
            {
                Quit();
            }
            else if (state == State.CONTROL_MENU)
            {
                HideControls();
            }
        }

        if (!(Mathf.Approximately(0f, Input.GetAxis("Mouse X")) || Mathf.Approximately(0f, Input.GetAxis("Mouse Y"))))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowControls()
    {
        SetState(State.CONTROL_MENU);
    }

    public void HideControls()
    {
        SetState(State.DEFAULT);
    }

    public void StartGame()
    {
        LevelLoader.instance.LoadLevel(1);
    }

    public void StartTutorial()
    {
        LevelLoader.instance.LoadLevel(2);
    }

    public void Quit()
    {
        Application.Quit();
    }


    private void SetState(State newState)
    {
        state = newState;

        switch (state)
        {
            case State.DEFAULT:
                controlMenu.SetActive(false);
                mainMenu.SetActive(true);
                eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
                break;

            case State.CONTROL_MENU:
                controlMenu.SetActive(true);
                mainMenu.SetActive(false);
                eventSystem.SetSelectedGameObject(backButton);
                break;

            default:
                Debug.LogError("Unknown Menu State");
                break;
        }
    }
}
