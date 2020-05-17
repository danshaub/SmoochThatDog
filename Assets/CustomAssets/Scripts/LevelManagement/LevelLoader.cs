using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Reference Used: https://www.youtube.com/watch?v=YMj2qPq9CP8
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;

    public GameObject loadingPanel;
    public Slider loadBar;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        loadingPanel.SetActive(false);
    }

    public void LoadLevel(int sceneIndex)
    {        
        loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAsynchronously(sceneIndex));
    }

    IEnumerator LoadSceneAsynchronously(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            loadingPanel.SetActive(true);
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadBar.value = progress;
            yield return null;
            loadingPanel.SetActive(false);
        }

        loadingPanel.SetActive(false);
    }
}
