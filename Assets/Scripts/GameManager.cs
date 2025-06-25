using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool GameEnded => gameEnded;

    public Stats GameStats;

    private bool gameEnded = false;

    private GameObject map1Root;
    private GameObject enemiesRoot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
    }

    public void EndGame()
    {
        UIManager.Instance.ShowYouDiedText();
        gameEnded = true;
    }

    public void EnterBeehive()
    {
        Player.LocalPlayer.HasPollen = false;
        GameStats.DepositedPollen += GameStats.CarriedPollen;
        GameStats.CarriedPollen = 0;
        SceneManager.LoadScene("Map_Beehive", LoadSceneMode.Additive);
        map1Root.SetActive(false);
        enemiesRoot.SetActive(false);
        StartCoroutine(SetActiveSceneWhenLoaded("Map_Beehive", MovePlayerToSpawn));
    }

    public void ExitBeehive()
    {
        SceneManager.UnloadSceneAsync("Map_Beehive");
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Map1"));
        map1Root.SetActive(true);
        enemiesRoot.SetActive(true);
        MovePlayerToSpawn();
    }

    private void MovePlayerToSpawn()
    {
        GameObject spawnPointGo = null;
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform spawn = root.transform.Find("PlayerSpawn");
            if (spawn != null)
                spawnPointGo = spawn.gameObject;

            // Or, search all children with tag
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.CompareTag("PlayerSpawn"))
                    spawnPointGo = t.gameObject;
            }
        }
        
        if (spawnPointGo is null)
        {
            Debug.LogError("No spawn point set-up for this scene! Please make a gameobject and give it the PlayerSpawn tag");
            return;
        }
        
        Player.LocalPlayer.transform.position = spawnPointGo.transform.position;
    }

    private IEnumerator SetActiveSceneWhenLoaded(string sceneName, [CanBeNull] Action callback)
    {
        while (!SceneManager.GetSceneByName(sceneName).isLoaded)
            yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

        // Wait one more frame to ensure all Awake/Start methods have run
        yield return null;
        
        callback?.Invoke();
    }

    public void RestartGame()
    {
        gameEnded = false;
        GameStats = new Stats();
        UIManager.Instance.HideYouDiedText();
        SceneManager.LoadScene("Map1", LoadSceneMode.Single);
    }

    public void NewGame()
    {
        gameEnded = false;
        GameStats = new Stats();
        SceneManager.LoadScene("Map1", LoadSceneMode.Single);
    }

    private void SceneManagerOnsceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        if (loadedScene.name != "Map1") return;
        
        map1Root = GameObject.FindGameObjectWithTag("MapRoot");
        enemiesRoot = GameObject.FindGameObjectWithTag("Enemy");
    }

    public void ContinueGame()
    {
        gameEnded = false;
        GameStats = LoadStats();
        if (GameStats is null)
        {
            Debug.LogError("No save file in player prefs found, ignoring \"continue\" button!");
            return;
        } // Don't allow continue if we don't have a save
        SceneManager.LoadScene("Map1", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        gameEnded = false;
        Instance = null;
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        Destroy(gameObject); // Destroy GameManager to prevent multiple instances
    }

    public void SaveStats()
    {
        string json = JsonUtility.ToJson(GameStats);
        PlayerPrefs.SetString("save_data", json);
        PlayerPrefs.Save();
    }

    public Stats LoadStats()
    {
        if (PlayerPrefs.HasKey("save_data"))
        {
            string json = PlayerPrefs.GetString("save_data");
            return JsonUtility.FromJson<Stats>(json);
        }

        return null;
    }

    private void OnEnable()
    {
        Application.quitting += OnApplicationQuitHandler;
    }

    private void OnDisable()
    {
        Application.quitting -= OnApplicationQuitHandler;
    }

    private void OnApplicationQuitHandler()
    {
        Debug.Log("Application is quitting. Saving game...");
        SaveStats();
    }
}
