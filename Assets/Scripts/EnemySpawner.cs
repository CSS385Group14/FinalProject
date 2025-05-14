using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] enemyPreFabs;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 10;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyscalingFactor = 0.75f;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();
    public GameManager gameManager;
    private int currentWave = 1;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;
    private bool gameStarted = false;

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave();
        UpdateUI();
    }

    private void EnemyDestroyed() {
        enemiesAlive -= 1;
        Debug.Log("Left to spawn: " + enemiesLeftToSpawn);
        Debug.Log("Enemies on screen: " + enemiesAlive);
    }

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyscalingFactor));
    }

    // Update is called once per frame
    void Update()
    {   
        // only spawn waves once the game is started
        // check for game start and if game is ended
        if (gameStarted && !gameManager.gameEnd)
        {
            if (!isSpawning) {
                return;
            }

            timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn >= (1f / enemiesPerSecond) && enemiesLeftToSpawn > 0)
            {
                SpawnEnemy();
                enemiesLeftToSpawn--;
                enemiesAlive++;
                UpdateUI();
                timeSinceLastSpawn = 0f;
            }

            if (enemiesAlive == 0 && enemiesLeftToSpawn == 0)
            {
                EndWave();
            }        
        }
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;
        currentWave++;
        StartCoroutine(StartWave());
    }

    private void UpdateUI()
    {
        GameObject.Find("WaveCountText").GetComponent<TextMeshProUGUI>().SetText("Wave " + currentWave);
        GameObject.Find("IncomingEnemiesText").GetComponent<TextMeshProUGUI>().SetText("Incoming enemies: " + enemiesLeftToSpawn);
    }

    private void SpawnEnemy() {
        GameObject prefabToSpawn = enemyPreFabs[0];
        Instantiate(prefabToSpawn, LevelManager.main.startPoint.position, Quaternion.identity);
    }

    // invoked by game manager
    public void BeginSpawning()
    {
        gameStarted = true;
        StartCoroutine(StartWave());
    }
}
