using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public int startingLevelThreshold = 20;
    public const int MAX_LEVEL = 10;
    private GameManager gameManager;
    private PlayerController player1;
    private PlayerController player2;
    private int[] levelThresholds = new int[MAX_LEVEL]; // store all level thresholds
    private bool gameStarted = false; // script only starts after player starts the game

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // if player1's xp total exceeds the level threshold, level up
        if (gameStarted && player1.level <= MAX_LEVEL && player1.playerXP >= levelThresholds[player1.level - 1])
        {
            Debug.Log("Player1 LEVEL UP!");
            player1.LevelUp();
        }

        if (gameStarted && gameManager.isCoopEnabled) // if there are 2 players
        {
            // check player2
            if (player2.level <= MAX_LEVEL && player2.playerXP >= levelThresholds[player2.level - 1])
            {
                Debug.Log("Player2 LEVEL UP!");
                player2.LevelUp();
            }
        }
    }

    public void BeginLevelCheck()
    {
        gameStarted = true;

        // find game manager and player controller scripts
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        player1 = GameObject.Find("Player1(Clone)").GetComponent<PlayerController>();
        if (gameManager.isCoopEnabled) // there are 2 players
        {
            player2 = GameObject.Find("Player2(Clone)").GetComponent<PlayerController>();
        }

        // calculate the xp thresholds to exceed for level-up
        SetLevelThresholds();
    }

    private void SetLevelThresholds()
    {
        // thresholds double for every level
        // if 1->2, threshold = 20xp
        // if 2->3, threshold = 40xp
        // if 3->4, threshold = 80xp
        // if 4->5, threshold = 160xp
        //           ... 
        // xp is pooled, meaning that after reaching a threshold,
        // the total current xp will always be half of the next
        // required threshold
        levelThresholds[0] = startingLevelThreshold;
        for (int i = 1; i < MAX_LEVEL; i++)
        {
            levelThresholds[i] = levelThresholds[i - 1] * 2;
            Debug.Log("Level " + (i + 1) + " XP required: " + levelThresholds[i]);
        }
    }
}
