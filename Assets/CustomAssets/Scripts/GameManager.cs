using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
}
