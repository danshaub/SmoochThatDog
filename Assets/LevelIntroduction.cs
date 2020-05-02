using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelIntroduction : MonoBehaviour
{
    public GameObject introPanel;
    public GameObject button;
    public EventSystem eventSystem;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Dismiss();
        }
    }
    void Start()
    {
        Time.timeScale = 0f;
        introPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(button);
        CharacterActions.instance.lockControl = true;
    }

    public void Dismiss()
    {
        introPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CharacterActions.instance.lockControl = false;
        Time.timeScale = 1f;
    }
}
