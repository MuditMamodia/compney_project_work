using UnityEngine;

public class Booster_manager_singleton_m : MonoBehaviour
{
    public static Booster_manager_singleton_m Instance;

    [Header("Booster Settings for Next Level")]
    public bool giveRandomBooster = false;
    public int boostersToGive = 1;    // how many random boosters to spawn

    private void Awake()
    {


        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // survive scene load
        }
        else
        {
            Destroy(gameObject);
        }
    }
}