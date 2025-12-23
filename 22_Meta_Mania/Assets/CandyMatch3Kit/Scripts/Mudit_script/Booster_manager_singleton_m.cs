using UnityEngine;
using UnityEngine.SceneManagement;

public class Booster_manager_singleton_m : MonoBehaviour
{
    public static Booster_manager_singleton_m Instance;

    [Header("Booster Settings for Next Level")]
    public bool giveRandomBooster = false;
    public int boostersToGive = 1;    // how many random boosters to spawn

    [Header("Reset Settings")]
    [SerializeField] private string resetSceneName;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Listen for scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // SCENE LOAD CALLBACK

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == resetSceneName)
        {
            giveRandomBooster = false;
            Debug.Log("Random booster reset on scene: " + scene.name);
        }
    }
}