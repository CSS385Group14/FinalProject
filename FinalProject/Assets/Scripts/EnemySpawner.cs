using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int baseEnemiesLv1 = 10;
    [SerializeField] private int baseEnemiesLv2 = 5;
    [SerializeField] private int baseEnemiesLv3 = 3;
    [SerializeField] private int baseEnemiesLv4 = 2;
    [SerializeField] private int baseEnemiesLv5 = 1;

    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyScalingFactor = 0.75f;
    [SerializeField] private TextMeshProUGUI nextWaveTimerText;
    private float timeUntilNextWave;
    private bool countingDownNextWave;


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
    private GameObject defense;
    private Dictionary<int, int> enemiesToSpawnPerLevel = new Dictionary<int, int>();

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    public void SetTargets(GameObject p1, GameObject p2, GameObject tower, GameObject defense)
    {
        player1 = p1;
        player2 = p2;
        this.tower = tower;
        this.defense = defense;
    }

    private IEnumerator StartWave()
    {
        isSpawning = true;
        CalculateEnemiesToSpawn();
        enemiesLeftToSpawn = TotalEnemiesToSpawn();
        UpdateUI();
        yield return null; // Let the frame settle
    }



    private void EnemyDestroyed()
    {
        enemiesAlive--;
    }

    private void CalculateEnemiesToSpawn()
    {
        enemiesToSpawnPerLevel.Clear();

        if (currentWave >= 1) AddEnemyCount(1, baseEnemiesLv1, currentWave);
        if (currentWave >= 3) AddEnemyCount(2, baseEnemiesLv2, currentWave - 2);
        if (currentWave >= 5) AddEnemyCount(3, baseEnemiesLv3, currentWave - 4);
        if (currentWave >= 7) AddEnemyCount(4, baseEnemiesLv4, currentWave - 6);
        if (currentWave >= 10) AddEnemyCount(5, baseEnemiesLv5, currentWave - 9);
    }

    private void AddEnemyCount(int level, int baseCount, int waveCountForLevel)
    {
        if (level <= enemyPrefabs.Length)
        {
            int scaled = Mathf.RoundToInt(baseCount * Mathf.Pow(waveCountForLevel, difficultyScalingFactor));
            if (scaled > 0)
            {
                enemiesToSpawnPerLevel[level] = scaled;
            }
        }
    }


    private void AddEnemyCount(int level, int baseCount)
    {
        if (level <= enemyPrefabs.Length)
        {
            int scaled = Mathf.RoundToInt(baseCount * Mathf.Pow(currentWave, difficultyScalingFactor));
            if (scaled > 0)
            {
                enemiesToSpawnPerLevel[level] = scaled;
            }
        }
    }

    private int TotalEnemiesToSpawn()
    {
        int total = 0;
        foreach (var kvp in enemiesToSpawnPerLevel)
        {
            total += kvp.Value;
        }
        return total;
    }

    private void Update()
    {
        if (!gameStarted || !isSpawning) return;

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
        StartCoroutine(CountdownToNextWave());
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



        int selectedLevel = GetRandomAvailableEnemyLevel();
        if (selectedLevel == -1)
        {
            Debug.LogWarning("No available enemy levels to spawn.");
            return;
        }

        GameObject prefab = enemyPrefabs[selectedLevel - 1];
        enemiesToSpawnPerLevel[selectedLevel]--;
        Transform spawnPoint = PathManager.main.GetRandomStartPoint();
        GameObject enemyObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        Transform[] path = PathManager.main.GetRandomPath();

        // Assign movement script
        if (selectedLevel == 4) // Enemy level 4 (index 3)
        {
            EnemyChase chaseEnemy = enemyObj.GetComponent<EnemyChase>();
            if (chaseEnemy != null && path != null && path.Length > 1)
            {
                chaseEnemy.InitPath(path);
            }
        }
        else
        {
            EnemyMovement moveEnemy = enemyObj.GetComponent<EnemyMovement>();
            if (moveEnemy != null && path != null && path.Length > 1)
            {
                moveEnemy.InitPath(path);
            }
        }

        // Assign targets based on enemy level
        
        BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
        if (enemy != null) enemy.SetTargets(player1, player2, tower, defense);

    }


    private int GetRandomAvailableEnemyLevel()
    {
        List<int> availableLevels = new List<int>();
        foreach (var kvp in enemiesToSpawnPerLevel)
        {
            if (kvp.Value > 0)
            {
                availableLevels.Add(kvp.Key);
            }
        }

        if (availableLevels.Count == 0)
            return -1;

        return availableLevels[Random.Range(0, availableLevels.Count)];
    }

    public void BeginSpawning()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(StartWave());
        }
    }
    public void StopSpawning()
    {
        if (gameStarted)
        {
            gameStarted = false;
        }
    }

    private void UpdateNextWaveTimerUI(float timeLeft)
    {
        if (nextWaveTimerText != null)
        {
            nextWaveTimerText.SetText(timeLeft.ToString("F1") + "s");
        }
    }
    private IEnumerator CountdownToNextWave()
    {
        nextWaveTimerText.gameObject.SetActive(true);
        countingDownNextWave = true;
        timeUntilNextWave = timeBetweenWaves;

        while (timeUntilNextWave > 0f)
        {
            UpdateNextWaveTimerUI(timeUntilNextWave);
            yield return null;
            timeUntilNextWave -= Time.deltaTime;
        }

        countingDownNextWave = false;
        if (nextWaveTimerText != null)
        {
            nextWaveTimerText.SetText("");
        }

        StartCoroutine(StartWave());
    }

}
