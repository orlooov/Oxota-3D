using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool questCompleted = false;
    public int currentKills = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}