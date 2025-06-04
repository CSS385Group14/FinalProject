using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : Placeable
{
    public const int MAX_LEVEL = 3;
    public GameObject projectilePrefab;
    public GameObject popupTextPrefab;
    public Transform firePoint;
    public Transform popupSpawnLocation;
    public Sprite[] levelSprites = new Sprite[MAX_LEVEL - 1];
    public int level = 1;
    public int upgradeCost = 100;
    public float fireRate = 1f;
    public float projectileSpeed = 10f;
    public int projectileDamage = 20;
    private int playerNumber;
    private float fireCooldown;
    private SpriteRenderer sr;
    private int currentHP;
    private int maxHP = 100;

    [Header("Detection Settings")]
    public float detectionRange = 5f; // NEW: Range at which tower detects enemies
    public LayerMask enemyLayer; // NEW: Assign this in the Inspector to "Enemy" layer

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
        goldCost = 100;
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (currentHP <= 0)
        {
            Destroy(gameObject);
            return;
        }

        Transform target = FindClosestEnemy();
        if (target != null && fireCooldown <= 0f)
        {
            FireAt(target);
            fireCooldown = 1f / fireRate;
        }
    }

    Transform FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayer);
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = hit.transform;
            }
        }

        return closest;
    }

    void FireAt(Transform enemy)
    {
        GameObject instance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        DefenseProjectile proj = instance.GetComponent<DefenseProjectile>();
        proj.SetTarget(enemy);
        proj.SetSpeed(projectileSpeed);
        proj.SetDamage(projectileDamage);
        proj.SetOwner(playerNumber);
    }

    public override void Place(int selectionIndex, int playerNumber, Transform playerTransform, Vector2 lastMoveDirection)
    {
        PlayerController player = GameObject.Find("Player" + playerNumber + "(Clone)").GetComponent<PlayerController>();

        if (player.DeductGold(goldCost))
        {
            GameObject instance = Instantiate(gameObject, playerTransform.position, transform.rotation);
            DefenseTower defTower = instance.GetComponent<DefenseTower>();
            defTower.SetOwner(playerNumber);
        }
    }

    public void SetOwner(int playerNumber)
    {
        this.playerNumber = playerNumber;
    }

    public void Upgrade()
    {
        if (level == MAX_LEVEL) return;

        int playerNumberInRange = GetPlayerNumberInRange();
        if (playerNumberInRange == -1) return;

        if (!GameObject.Find("Player" + playerNumberInRange + "(Clone)").GetComponent<PlayerController>().DeductGold(upgradeCost))
            return;

        level++;
        sr.sprite = levelSprites[level - 2];
        fireRate += 0.5f;
        projectileDamage += 10;
        projectileSpeed += 5;
        upgradeCost += 100;
        ShowPopup("Upgraded!");
    }

    private void ShowPopup(string message)
    {
        GameObject popup = Instantiate(popupTextPrefab, popupSpawnLocation.position, Quaternion.identity, popupSpawnLocation.parent);
        popup.GetComponent<PopupText>().SetText(message, Color.white);
    }

    int GetPlayerNumberInRange()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            return closestPlayer.GetComponent<PlayerController>().playerNumber;
        }

        return -1;
    }

    public void TakeDamage(int damageTaken)
    {
        currentHP = Mathf.Max(currentHP - damageTaken, 0);
    }

    void OnDrawGizmosSelected()
    {
        // Visualize detection range in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
