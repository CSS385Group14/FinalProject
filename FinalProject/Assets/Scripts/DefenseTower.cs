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
    public int goldCost = 100;
    private int playerNumber;
    private float fireCooldown;
    private List<Transform> enemiesInRange = new List<Transform>();
    private SpriteRenderer sr;

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

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (enemiesInRange.Count > 0 && fireCooldown <= 0f)
        {
            Transform target = enemiesInRange[0]; // grab the first enemy
            FireAt(target);
            fireCooldown = 1f / fireRate;
        }
    }

    void FireAt(Transform enemy)
    {
        GameObject instance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // set parameters
        DefenseProjectile proj = instance.GetComponent<DefenseProjectile>();
        proj.SetTarget(enemy); // target enemy's location
        proj.SetSpeed(projectileSpeed);
        proj.SetDamage(projectileDamage);
        proj.SetOwner(playerNumber);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.transform);
        }
    }

    public void SetOwner(int playerNumber)
    {
        this.playerNumber = playerNumber;
    }

    public void Upgrade()
    {
        if (level == MAX_LEVEL) // do not allow upgrade at max level
        {
            return;
        }

        int playerNumberInRange = GetPlayerNumberInRange();
        if (playerNumberInRange == -1)
        {
            return;
        }

        // deduct gold, if player in range has enough
        if (!GameObject.Find("Player" + playerNumberInRange + "(Clone)").GetComponent<PlayerController>().DeductGold(upgradeCost))
        {
            return;
        }

        level++;

        // change sprite
        sr.sprite = levelSprites[level - 2];

        // improve stats
        fireRate += 0.5f;
        projectileDamage += 10;
        projectileSpeed += 5;

        // increase upgrade cost
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

        // no player found
        return -1;
    }

}
