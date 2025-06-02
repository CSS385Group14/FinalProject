using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject popupTextPrefab;
    public Transform popupSpawnLocation;
    public float speed = 5f;
    public bool isPlayerOne = true;
    public bool isDead = false;
    public int playerXP = 0;
    public int playerGold = 100;
    public int playerMaxHP = 100;
    public int level = 1;
    public float fireRate = 1f; // seconds between shots
    public int weaponDamage = 5; // damage of player projectiles
    public float weaponProjectileSpeed = 15; // speed of player projectiles
    public float xBound = 22.08901f;
    public float yBound = 12.305f;
    private float nextFireTime = 0f;
    private int validBuildZoneCount = 0; // 1 for testing purposes, make 0 later
    private int validBarricadeBuildZoneCount = 0;
    private int invalidBuildZoneCount = 0;
    private int currentHP = 100;
    private GameManager gameManager;
    //private DefenseManager defenseManager;
    private InventoryManager inventoryManager;
    private PlayerAttackManager playerAttackManager;
    private HealthbarController hpBarController;
    private XPBarController xpBarController;
    private Vector2 lastMoveDirection;
    private GameObject deathUI;
    public int playerNumber = 1;
    private float horizontalInput;
    private float verticalInput;
    public Placeable placeable;
    Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //defenseManager = GameObject.Find("LevelManager").GetComponent<DefenseManager>();
        playerAttackManager = transform.Find("FireRange").GetComponent<PlayerAttackManager>();
        inventoryManager = transform.GetComponent<InventoryManager>();
        hpBarController = transform.GetComponent<HealthbarController>();
        xpBarController = transform.GetComponent<XPBarController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();
        // get player numbers
        if (isPlayerOne) // is player 1
        {
            playerNumber = 1;
        }
        else // is player 2
        {
            playerNumber = 2;
        }

        deathUI = GameObject.Find("P" + playerNumber + "DeathBG"); // get death UI handle
        deathUI.SetActive(false); // turn off death indicator

        // update UI at the start to reflect starting values
        UpdateUI(playerGold, level);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameEnd || isDead) // block player controls if game ended or if dead
        {
            return;
        }


        if (currentHP < 1) // check health
        {
            animator.SetTrigger("DeadTrigger");
            gameManager.playersAlive--;
            Debug.Log("Players alive: " + gameManager.playersAlive);

            // show death indicators
            transform.Find("DeathIndicator").gameObject.SetActive(true);
            deathUI.SetActive(true);

            // disable this player's controls
            isDead = true;
        }

        // get the input axes based on player number
        horizontalInput = Input.GetAxis("HorizontalP" + playerNumber);
        verticalInput = Input.GetAxis("VerticalP" + playerNumber);
        Vector2 moveInput = new Vector2(horizontalInput, verticalInput);
        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false; // Facing right
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true; // Facing left
        }

        // // only update lastMoveDirection if there's movement
        if (moveInput.sqrMagnitude > 0.001f)
        {
            //     // get the last move direction for the projectile fired
            lastMoveDirection = moveInput.normalized;
            animator.SetBool("isWalking", moveInput.sqrMagnitude > 0.001f);


            //     // rotate sprite to face the movement direction
            //     float angle = Mathf.Atan2(lastMoveDirection.y, lastMoveDirection.x) * Mathf.Rad2Deg;
            //     transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            //     animator.SetTrigger("WalkTrigger");
        }

        // interpret player control of the game object
        transform.position += new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;


        if (isPlayerOne) // player 1 controls
        {
            if (Input.GetKey(KeyCode.E) && Time.time >= nextFireTime)
            {
                //FireProjectile();
    
                animator.SetTrigger("AttackTrigger");
                nextFireTime = Time.time + fireRate;
            }

            // && (InValidBuildZone() || InValidBarricadeBuildZone())

            if (Input.GetKeyDown(KeyCode.Q) && !CannotBuild()) // place item
            {
                if (placeable != null)
                {
                    // Debug.Log("tag: " + placeable.tag);
                    // Debug.Log("is in barricade zone: " + InValidBarricadeBuildZone());
                    if (placeable.CompareTag("Barricade") && InValidBarricadeBuildZone())
                    {
                        placeable.Place(0, playerNumber, transform, lastMoveDirection);
                    }
                    else if (placeable.CompareTag("Defense") && InValidBuildZone())
                    {
                        placeable.Place(0, playerNumber, transform, lastMoveDirection);
                    }
                }
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
            if ((Input.GetKey(KeyCode.U) || Input.GetKey(KeyCode.RightShift)) && Time.time >= nextFireTime)
            {
                FireProjectile();
                nextFireTime = Time.time + fireRate;
            }

            if ((Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.RightControl)) && !CannotBuild()) // place item
            {
                if (placeable != null)
                {
                    // Debug.Log("tag: " + placeable.tag);
                    // Debug.Log("is in barricade zone: " + InValidBarricadeBuildZone());
                    if (placeable.CompareTag("Barricade") && InValidBarricadeBuildZone())
                    {
                        placeable.Place(0, playerNumber, transform, lastMoveDirection);
                    }
                    else if (placeable.CompareTag("Defense") && InValidBuildZone())
                    {
                        placeable.Place(0, playerNumber, transform, lastMoveDirection);
                    }
                }
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

            // arrow movements
            if (!isPlayerOne)
            {
                horizontalInput = Input.GetAxis("HorizontalP2Arrows");
                verticalInput = Input.GetAxis("VerticalP2Arrows");
                Vector2 moveInputArrows = new Vector2(horizontalInput, verticalInput);

                if (moveInputArrows.sqrMagnitude > 0.001f)
                {
                    // get the last move direction for the projectile fired
                    lastMoveDirection = moveInputArrows.normalized;
                }

                    // interpret player control of the game object
                transform.position += new Vector3(moveInputArrows.x, moveInputArrows.y, 0) * speed * Time.deltaTime;
            }
        }

        // Boundaries
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
        animator.SetTrigger("HurtTrigger");
        currentHP -= damageTaken;
        hpBarController.TakePlayerDamage(damageTaken);
    }

    public void Heal(int healAmount)
    {
        currentHP = Mathf.Min(currentHP + healAmount, playerMaxHP);
        hpBarController.HealPlayer(healAmount);
    }

    public void GainXP(int xpAmount)
    {
        playerXP += xpAmount;
        xpBarController.FillXPBar(xpAmount);
        UpdateUI(playerGold, level);
    }

    public void GainGold(int goldAmount)
    {
        playerGold += goldAmount;
        UpdateUI(playerGold, level);
    }

    public bool DeductGold(int goldAmount)
    {
        if (goldAmount > playerGold)
        {
            Debug.LogError("Insufficient gold to deduct.");
            return false;
        }
        playerGold -= goldAmount;
        UpdateUI(playerGold, level);
        return true;
    }

    public void LevelUp()
    {
        level++;

        // reward player after level up
        GainGold(25 * level); // scale reward w/ level
        Heal(100); // full recovery
        BoostStats(1);
        xpBarController.ResetXPBar();
        UpdateUI(playerGold, level);
        ShowPopup("Level-up!");
    }

    public void BoostStats(int numberBoosts)
    {
        for (int i = 0; i < numberBoosts; i++)
        {
            speed *= 1.07f;
            weaponDamage = (int)(weaponDamage * 1.14); // 14% damage boost/level (max: 200%)
            weaponProjectileSpeed *= 1.07f; // 7% proj speed boost/level (max: double proj speed)
            fireRate *= 0.93f; // 7% firerate boost/level (max: halved fire rate)
        }
    }

    private void ShowPopup(string message)
    {
        GameObject popup = Instantiate(popupTextPrefab, popupSpawnLocation.position, Quaternion.identity, popupSpawnLocation.parent);
        popup.GetComponent<PopupText>().SetText(message, Color.white);
    }

    private bool InValidBuildZone()
    {
        // if in any buildzone trigger (even overlapping)
        // return true
        return validBuildZoneCount > 0;
    }

    private bool InValidBarricadeBuildZone()
    {
        return validBarricadeBuildZoneCount > 0;
    }

    private bool CannotBuild()
    {
        return invalidBuildZoneCount > 0;
    }

    private void UpdateUI(int gold, int level)
    {
        if (playerNumber == 1) // player 1
        {
            GameObject.Find("P1LevelText").GetComponent<TextMeshProUGUI>().SetText("" + (level - 1));
            GameObject.Find("P1GoldCountText").GetComponent<TextMeshProUGUI>().SetText("Gold: " + gold);
        }
        else // player 2
        {
            GameObject.Find("P2LevelText").GetComponent<TextMeshProUGUI>().SetText("" + (level - 1));
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
            Debug.Log("No enemies in range to shoot.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BuildZone"))
        {
            validBuildZoneCount++;
        }

        if (collision.CompareTag("BarricadeBuildZone"))
        {
            validBarricadeBuildZoneCount++;
        }

        if (collision.CompareTag("NoBuildZone"))
        {
            invalidBuildZoneCount++;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("BuildZone"))
        {
            validBuildZoneCount = Mathf.Max(0, validBuildZoneCount - 1);
        }

        if (collision.CompareTag("BarricadeBuildZone"))
        {
            validBarricadeBuildZoneCount = Mathf.Max(0, validBarricadeBuildZoneCount - 1);
        }

        if (collision.CompareTag("NoBuildZone"))
        {
            invalidBuildZoneCount = Mathf.Max(0, invalidBuildZoneCount - 1);
        }
    }

}
