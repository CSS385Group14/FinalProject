using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private TextMeshProUGUI waveCountText;
    [SerializeField] private TextMeshProUGUI incomingEnemiesText;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 10;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyScalingFactor = 0.75f;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave = 1;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;
    private bool gameStarted = false;

    private GameObject player1;
    private GameObject player2;
    private GameObject tower;

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }
    // Set the targets (called by GameManager or another script to assign the targets)
    public void SetTargets(GameObject p1, GameObject p2, GameObject tower)
    {
        player1 = p1;
        player2 = p2;
        this.tower = tower;
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave();
        UpdateUI();
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
        Debug.Log("Left to spawn: " + enemiesLeftToSpawn);
        Debug.Log("Enemies on screen: " + enemiesAlive);
    }

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFactor));
    }

    void Update()
    {
        if (!gameStarted || !isSpawning)
            return;

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

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;
        currentWave++;
        StartCoroutine(StartWave());
    }

    private void UpdateUI()
    {
        if (waveCountText != null)
            waveCountText.SetText("Wave " + currentWave);

        if (incomingEnemiesText != null)
            incomingEnemiesText.SetText("Incoming enemies: " + enemiesLeftToSpawn);
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned in EnemySpawner.");
            return;
        }

        if (PathManager.main == null || PathManager.main.startPoint == null)
        {
            Debug.LogError("PathManager or its start point is not assigned.");
            return;
        }

        GameObject selectedPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject enemyObj = Instantiate(selectedPrefab, PathManager.main.startPoint.position, Quaternion.identity);

        EnemyMovement enemy = enemyObj.GetComponent<EnemyMovement>();
        if (enemy != null)
        {
            Transform[] path = PathManager.main.GetRandomPath();
            if (path != null && path.Length > 1)
            {
                enemy.InitPath(path);
            }
            else
            {
                Debug.LogWarning("Enemy path is invalid or too short.");
            }
            // Assign targets to the spawned enemy
            if (enemyObj.GetComponent<Enemy>() != null)
            {
                enemyObj.GetComponent<Enemy>().SetTargets(player1, player2, tower);
            }
        }
        else
        {
            Debug.LogWarning("Spawned enemy prefab lacks EnemyMovement script.");
        }
    }

    // Called externally to begin spawning waves
    public void BeginSpawning()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(StartWave());
        }
    }
}
