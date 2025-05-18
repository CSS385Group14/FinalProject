using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
    public float speed = 5f;
    public bool isPlayerOne = true;
    public bool isDead = false;
    public int playerXP = 0;
    public int playerGold = 100;
    public int playerHP = 100;
    public int level = 1;
    public float fireRate = 1f; // seconds between shots
    public int weaponDamage = 5; // damage of player projectiles
    public float weaponProjectileSpeed = 15; // speed of player projectiles
    public float xBound = 22.08901f;
    public float yBound = 12.305f;
    private float nextFireTime = 0f;
    private GameManager gameManager;
    private DefenseManager defenseManager;
    private InventoryManager inventoryManager;
    private PlayerAttackManager playerAttackManager;
    private Vector2 lastMoveDirection;
    private int playerNumber;
    private float horizontalInput;
    private float verticalInput;
    public Placeable placeable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        defenseManager = GameObject.Find("LevelManager").GetComponent<DefenseManager>();
        playerAttackManager = transform.Find("FireRange").GetComponent<PlayerAttackManager>();
        inventoryManager = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();

        // get player numbers
        if (isPlayerOne) // is player 1
        {
            playerNumber = 1;
        }
        else // is player 2
        {
            playerNumber = 2;
        }

        // update UI at the start to reflect starting values
        UpdateUI(playerXP, playerGold, level);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameEnd || isDead) // block player controls if game ended or if dead
            {
                return;
            }

        if (playerHP < 1) // check health
        {
            gameManager.playersAlive--;

            // show death indicator
            transform.Find("DeathIndicator").gameObject.SetActive(true);

            // disable this player's controls
            isDead = true;
        }

        // get the input axes based on player number
        horizontalInput = Input.GetAxis("HorizontalP" + playerNumber);
        verticalInput = Input.GetAxis("VerticalP" + playerNumber);
        Vector2 moveInput = new Vector2(horizontalInput, verticalInput);

        // only update lastMoveDirection if there's movement
        if (moveInput.sqrMagnitude > 0.001f)
        {
            // get the last move direction for the projectile fired
            lastMoveDirection = moveInput.normalized;

            // rotate sprite to face the movement direction
            float angle = Mathf.Atan2(lastMoveDirection.y, lastMoveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }

        // interpret player control of the game object
        transform.position += new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;


        if (isPlayerOne) // player 1 controls
        {
            if (Input.GetKey(KeyCode.E) && Time.time >= nextFireTime)
            {
                FireProjectile();
                nextFireTime = Time.time + fireRate;
            }

            if (Input.GetKeyDown(KeyCode.Q)) // place item
            {
                if (placeable != null)
                {
                    placeable.Place(0, playerNumber, transform, lastMoveDirection);
                }
            }

            if (Input.GetKeyDown(KeyCode.R)) // place defense
            {
                defenseManager.Place(0, playerNumber, transform, lastMoveDirection);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) // hotbar select
            {
                inventoryManager.SelectItem(1, 0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) // hotbar select
            {
                inventoryManager.SelectItem(1, 1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3)) // hotbar select
            {
                inventoryManager.SelectItem(1, 2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4)) // hotbar select
            {
                inventoryManager.SelectItem(1, 3);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5)) // hotbar select
            {
                inventoryManager.SelectItem(1, 4);
            }
        }
        else // player 2 controls
        {
            if (Input.GetKey(KeyCode.U) && Time.time >= nextFireTime)
            {
                FireProjectile();
                nextFireTime = Time.time + fireRate;
            }

            if (Input.GetKeyDown(KeyCode.O)) // place item
            {
                if (placeable != null)
                {
                    placeable.Place(0, playerNumber, transform, lastMoveDirection);
                }
            }

            if (Input.GetKeyDown(KeyCode.Y)) // place defense
            {
                defenseManager.Place(0, playerNumber, transform, lastMoveDirection);
            }

            if (Input.GetKeyDown(KeyCode.Alpha6)) // hotbar select
            {
                inventoryManager.SelectItem(2, 0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha7)) // hotbar select
            {
                inventoryManager.SelectItem(2, 1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha8)) // hotbar select
            {
                inventoryManager.SelectItem(2, 2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha9)) // hotbar select
            {
                inventoryManager.SelectItem(2, 3);
            }

            if (Input.GetKeyDown(KeyCode.Alpha0)) // hotbar select
            {
                inventoryManager.SelectItem(2, 4);
            }
        }

        if (transform.position.x > xBound)
        {
            Vector2 pos = transform.position;
            pos.x = xBound;
            transform.position = pos;
        }

        if (transform.position.x < -xBound)
        {
            Vector2 pos = transform.position;
            pos.x = -xBound;
            transform.position = pos;
        }

        if (transform.position.y > yBound)
        {
            Vector2 pos = transform.position;
            pos.y = yBound;
            transform.position = pos;
        }

        if (transform.position.y < -yBound)
        {
            Vector2 pos = transform.position;
            pos.y = -yBound;
            transform.position = pos;
        }
    }

    public void TakeDamage(int damageTaken)
    {
        playerHP -= damageTaken;
        transform.GetComponent<HealthbarController>().TakePlayerDamage(damageTaken);
    }

    public void GainXP(int xpAmount)
    {
        playerXP += xpAmount;
        UpdateUI(playerXP, playerGold, level);
    }

    public void GainGold(int goldAmount)
    {
        playerGold += goldAmount;
        UpdateUI(playerXP, playerGold, level);
    }

    public bool DeductGold(int goldAmount)
    {
        if (goldAmount > playerGold)
        {
            Debug.LogError("Insufficient gold to deduct.");
            return false;
        }
        playerGold -= goldAmount;
        UpdateUI(playerXP, playerGold, level);
        return true;
    }

    public void LevelUp()
    {
        level++;
        UpdateUI(playerXP, playerGold, level);
    }

    private void UpdateUI(int xp, int gold, int level)
    {
        if (playerNumber == 1) // player 1
        {
            GameObject.Find("P1LevelText").GetComponent<TextMeshProUGUI>().SetText("" + (level - 1));
            GameObject.Find("P1XPCountText").GetComponent<TextMeshProUGUI>().SetText("XP: " + xp);
            GameObject.Find("P1GoldCountText").GetComponent<TextMeshProUGUI>().SetText("Gold: " + gold);
        }
        else // player 2
        {
            GameObject.Find("P2LevelText").GetComponent<TextMeshProUGUI>().SetText("" + (level - 1));
            GameObject.Find("P2XPCountText").GetComponent<TextMeshProUGUI>().SetText("XP: " + xp);
            GameObject.Find("P2GoldCountText").GetComponent<TextMeshProUGUI>().SetText("Gold: " + gold);
        }
    }

    private void FireProjectile()
    {
        // mark the projectile with the player's number
        PlayerProjectile playerProjectile = projectilePrefab.GetComponent<PlayerProjectile>();
        playerProjectile.projectileOwner = playerNumber;

        // shoot projectile toward enemies in range
        if (playerAttackManager.GetClosestEnemyTransformToAttack() != null)
        {
            playerProjectile.direction = (playerAttackManager.GetClosestEnemyTransformToAttack().position - transform.position).normalized;

            // spawn a projectile at location of player
            PlayerProjectile instance = Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation).GetComponent<PlayerProjectile>();

            // set projectile parameters based on player state
            instance.SetDamage(weaponDamage);
            instance.SetSpeed(weaponProjectileSpeed);
        }
        else
        {
            Debug.LogError("No enemies in range to shoot.");
        }
    }
}
