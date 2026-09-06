using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        DirectorStudio,
        Settings
    }

    [Header("State Tracking")]
    public GameState currentGameState;
    public GameObject activeActor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            if (transform.parent != null)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }

    private void Start()
    {
        currentGameState = GameState.MainMenu;

        if (activeActor == null)
        {
            var boss = GameObject.Find("The Boss");
            if (boss != null && boss.activeInHierarchy)
            {
                activeActor = boss;
            }
            else
            {
                var remy = GameObject.Find("Remy");
                if (remy != null && remy.activeInHierarchy) activeActor = remy;
                else
                {
                    var peasant = GameObject.Find("Peasant Girl");
                    if (peasant != null && peasant.activeInHierarchy) activeActor = peasant;
                }
            }
        }
    }
}
