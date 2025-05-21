using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;
    public bool isCoopEnabled = false; // track coop state
    public bool gameEnd = false; // track game state
    public bool gameStart = false;
    public float p1SpawnX = 14.75f; // player 1's x-spawn position
    public float p1SpawnY = 0; // player 1's y-spawn position
    public float p2SpawnX = 14.75f; // player 2's x-spawn position
    public float p2SpawnY = -2.5f; // player 2's y-spawn position
    public int playersAlive = 1; // track number of players alive
    private GameObject sceneObjects;
    private GameObject mainMenu;
    private GameObject player1UI;
    private GameObject player2UI;
    private GameObject towerUI;
    private GameObject waveInfo;
    private GameObject gameOverUI;
    private EnemySpawner enemySpawner;
    private ProgressionManager progManager;
    private TextMeshProUGUI coopButtonText;
    public GameObject testItem; // DELETE
    public GameObject testItem2; // DELETE

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get scene object handle
        sceneObjects = GameObject.Find("SceneObjects"); // obstacles in the scene

        // get UI object handles
        mainMenu = GameObject.Find("MainMenu"); // parent of buttons, title, player 1 & 2 UI
        player1UI = GameObject.Find("Player1UI"); // parent of xp and level counter text game objects
        player2UI = GameObject.Find("Player2UI");
        towerUI = GameObject.Find("TowerUI");
        waveInfo = GameObject.Find("WaveInfo"); // parent of wave count and enemy count text
        gameOverUI = GameObject.Find("GameOverUI");

        // get script handles
        enemySpawner = GameObject.Find("LevelManager").GetComponent<EnemySpawner>();
        progManager = GameObject.Find("ProgressionManager").GetComponent<ProgressionManager>();
        //inventoryManager = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
        coopButtonText = GameObject.Find("CoopButtonText").GetComponent<TextMeshProUGUI>();

        // hide scene objects and player UIs
        sceneObjects.SetActive(false);
        player1UI.SetActive(false);
        player2UI.SetActive(false);
        towerUI.SetActive(false);
        waveInfo.SetActive(false);
        gameOverUI.SetActive(false);

        // DELETE
        // EnableCoop();
        // StartGame();
        // InventoryManager inventoryManager = GameObject.Find("Player" + 1 + "(Clone)").GetComponent<InventoryManager>();
        // inventoryManager.AddItem(1, testItem);
        // inventoryManager.AddItem(1, testItem2);
        //Debug.Log("Added " + GameObject.Find("Player" + 1 + "(Clone)").GetComponent<InventoryManager>().p1Inv[0].name);
    }

    // Update is called once per frame
    void Update()
    {
        if (playersAlive < 1) // if all players are dead, end the game
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        // hide main menu, reveal scene objects and UI
        mainMenu.SetActive(false);
        sceneObjects.SetActive(true);
        player1UI.SetActive(true);
        towerUI.SetActive(true);
        waveInfo.SetActive(true);

        // instantiate player 1 and configure attack
        Instantiate(playerOnePrefab, new Vector2(p1SpawnX, p1SpawnY), playerOnePrefab.transform.rotation).GetComponent<PlayerController>();
        
        //FOR TESTING - DELETE
        InventoryManager inventoryManager1 = GameObject.Find("Player" + 1 + "(Clone)").GetComponent<InventoryManager>();
        inventoryManager1.AddItem(1, testItem);
        inventoryManager1.AddItem(1, testItem2);

        // check if coop is enabled
        if (isCoopEnabled)
        {
            playersAlive++; // increment for player 2

            // instantiate player 2
            Instantiate(playerTwoPrefab, new Vector2(p2SpawnX, p2SpawnY), playerTwoPrefab.transform.rotation).GetComponent<PlayerController>();

            // enable player 2 UI
            player2UI.SetActive(true);

            //FOR TESTING - DELETE
            InventoryManager inventoryManager2 = GameObject.Find("Player" + 2 + "(Clone)").GetComponent<InventoryManager>();
            inventoryManager2.AddItem(2, testItem);
            inventoryManager2.AddItem(2, testItem2);
        }

        // start checking for xp, level updates
        enemySpawner.BeginSpawning();
        progManager.BeginLevelCheck();
        gameStart = true;
    }

    public void EndGame()
    {
        // disable enemy spawns and player controls
        gameEnd = true;

        // show game over UI
        gameOverUI.SetActive(true);
    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void EnableCoop()
    {
        // toggle coop flag
        isCoopEnabled = !isCoopEnabled;

        if (isCoopEnabled) // was disabled, now enabled
        {
            coopButtonText.SetText("Disable Coop");
        }
        else // was enabled, now disabled
        {
            coopButtonText.SetText("Enable Coop");
        }
    }
}
