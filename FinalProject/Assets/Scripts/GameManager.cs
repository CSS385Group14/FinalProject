using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject player1UI;
    [SerializeField] private GameObject player2UI;
    [SerializeField] private GameObject towerUI;
    [SerializeField] private GameObject waveInfo;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI coopButtonText;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private ProgressionManager progManager;

    [SerializeField] private GameObject testItem;  // DELETE
    [SerializeField] private GameObject testItem2; // DELETE

    public bool isCoopEnabled = false; // track coop state
    public bool gameEnd = false;       // track game state
    public bool gameStart = false;

    public float p1SpawnX = 14.75f;
    public float p1SpawnY = 0;
    public float p2SpawnX = 14.75f;
    public float p2SpawnY = -2.5f;
    public int playersAlive;


    public static bool isPaused = false;

    void Start()
    {
        // Hide scene objects and player UIs
        player1UI.SetActive(false);
        player2UI.SetActive(false);
        towerUI.SetActive(false);
        waveInfo.SetActive(false);
        if (gameOverUI != null)
        {
            Debug.Log("gameOverUI exists and will be disabled.");
            gameOverUI.SetActive(false);
        }
        else
        {
            Debug.LogError("gameOverUI is NULL! Check Inspector assignment.");
        }
        pauseMenuUI.SetActive(false);
        tutorialPanel.SetActive(false);
        playersAlive = isCoopEnabled ? 2 : 1;
        gameStart = true;
        gameEnd = false;
    }

    void Update()
    {
        if (!gameStart) return;

        if (gameStart && playersAlive < 1)
        {
            EndGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void StartGame()
    {
        // Hide main menu, reveal scene objects and UI
        mainMenu.SetActive(false);
        player1UI.SetActive(true);
        towerUI.SetActive(true);
        waveInfo.SetActive(true);

        // Instantiate player 1 and configure
        Instantiate(playerOnePrefab, new Vector2(p1SpawnX, p1SpawnY), playerOnePrefab.transform.rotation);
        playersAlive = 1;

        InventoryManager inventoryManager1 = GameObject.Find("Player1(Clone)").GetComponent<InventoryManager>();
        inventoryManager1.AddItem(1, testItem);
        inventoryManager1.AddItem(1, testItem2);

        if (isCoopEnabled)
        {
            // Instantiate player 2
            Instantiate(playerTwoPrefab, new Vector2(p2SpawnX, p2SpawnY), playerTwoPrefab.transform.rotation);
            playersAlive = 2;

            player2UI.SetActive(true);

            InventoryManager inventoryManager2 = GameObject.Find("Player2(Clone)").GetComponent<InventoryManager>();
            inventoryManager2.AddItem(2, testItem);
            inventoryManager2.AddItem(2, testItem2);
        }
        else
        {
            // Boost stats for solo play
            inventoryManager1.gameObject.GetComponent<PlayerController>().BoostStats(10);
        }

        enemySpawner.BeginSpawning();
        progManager.BeginLevelCheck();
        gameStart = true;
    }

    public void EndGame()
    {
        gameEnd = true;
        gameStart = false;
        gameOverUI.SetActive(true);
    }

    public void ReloadScene()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void EnableCoop()
    {
        isCoopEnabled = !isCoopEnabled;
        coopButtonText.SetText(isCoopEnabled ? "Disable Coop" : "Enable Coop");
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ShowTutorial()
    {
        mainMenu.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    public void HideTutorial()
    {
        tutorialPanel.SetActive(false);
        mainMenu.SetActive(true);
    }
}
