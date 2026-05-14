using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else DestroyImmediate(gameObject);
    }

    public void LoadLevel(string levelName, int entranceIndex)
    {
        StartCoroutine(LoadSceneAsync(levelName, entranceIndex));
    }

    public IEnumerator LoadSceneAsync(string sceneName, int entranceIndex)
    {
        // TRANSITION Fade to black

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!loadOp.isDone)
        {
            yield return null;
        }

        LocalSceneManager sceneManager = FindFirstObjectByType<LocalSceneManager>();
        sceneManager.OnPlayerRoomEnter(entranceIndex);

        // TRANSITION BACK TO NEW SCENE
    }
}
