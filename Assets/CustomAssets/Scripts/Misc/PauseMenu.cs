using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public bool paused;
    public GameObject go;

    private void Start()
    {
        go.SetActive(false);
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

        if (paused && Input.GetKeyDown(KeyCode.Q))
        {
            Quit();
        }
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        CharacterActions.instance.lockControl = true;
        
        go.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Unpause()
    {
        paused = false;
        Time.timeScale = 1;
        CharacterActions.instance.lockControl = false;
        go.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public void Quit()
    {
        GameManager.instance.LoadSceneByIndex(0);
    } 
}
