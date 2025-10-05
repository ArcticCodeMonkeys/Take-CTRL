using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    // Remove singleton pattern - just use simple methods

    // Immediate load (blocking)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Async load with optional progress callback
    public void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadAsyncCoroutine(sceneName));
    }

    IEnumerator LoadAsyncCoroutine(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        // optional: allowSceneActivation = false to control when to show scene
        while (!op.isDone)
        {
            // op.progress is 0..0.9 while loading, then isDone when activation allowed
            // You can expose progress to a UI bar here: op.progress / 0.9f
            yield return null;
        }
    }

    // Load additively (useful for a persistent UI or overlay)
    public void LoadAdditive(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    // Unload additive scene
    public void UnloadScene(string sceneName)
    {
        StartCoroutine(UnloadAsync(sceneName));
    }

    IEnumerator UnloadAsync(string sceneName)
    {
        var op = SceneManager.UnloadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextScene()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            Debug.LogWarning("No next scene in build settings");
    }
}
