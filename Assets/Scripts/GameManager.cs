using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;
    public bool isCoopEnabled = false;
    public int p1SpawnX = -1;
    public int p1SpawnY = -8;
    public int p2SpawnX = 1;
    public int p2SpawnY = -8;
    private GameObject sceneObjects;
    private GameObject mainMenu;
    private GameObject player1UI;
    private GameObject player2UI;
    private GameObject waveInfo;
    private EnemySpawner enemySpawner;
    private ProgressionManager progManager;
    private Button coopButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // find objects
        sceneObjects = GameObject.Find("SceneObjects"); // obstacles in the scene
        mainMenu = GameObject.Find("MainMenu"); // parent of buttons, title, player 1 & 2 UI
        player1UI = GameObject.Find("Player1UI"); // parent of xp and level counter text game objects
        player2UI = GameObject.Find("Player2UI");
        waveInfo = GameObject.Find("WaveInfo"); // parent of wave count and enemy count text
        enemySpawner = GameObject.Find("LevelManager").GetComponent<EnemySpawner>();
        progManager = GameObject.Find("ProgressionManager").GetComponent<ProgressionManager>();
        coopButton = GameObject.Find("CoopButton").GetComponent<Button>();

        // hide scene objects and player UIs
        sceneObjects.SetActive(false);
        player1UI.SetActive(false);
        player2UI.SetActive(false);
        waveInfo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        // hide main menu, reveal scene objects and UI
        mainMenu.SetActive(false);
        sceneObjects.SetActive(true);
        player1UI.SetActive(true);
        waveInfo.SetActive(true);

        // instantiate player 1
        Instantiate(playerOnePrefab, new Vector2(p1SpawnX, p1SpawnY), playerOnePrefab.transform.rotation);
        
        // check if coop is enabled
        if (isCoopEnabled)
        {
            // instantiate player 2
            Instantiate(playerTwoPrefab, new Vector2(p2SpawnX, p2SpawnY), playerTwoPrefab.transform.rotation);

            // enable player 2 UI
            player2UI.SetActive(true);
        }

        // start checking for xp, level updates
        enemySpawner.BeginSpawning();
        progManager.BeginLevelCheck();
    }

    public void EnableCoop()
    {
        isCoopEnabled = true;

        // hide button when it is clicked
        coopButton.gameObject.SetActive(false);
    }
}
