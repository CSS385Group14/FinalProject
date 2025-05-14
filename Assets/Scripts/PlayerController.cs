using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
    public GameObject barricadePrefabTop;
    public GameObject barricadePrefabSide;
    public float speed = 5f;
    public bool isPlayerOne = true;
    public bool isDead = false;
    public int playerXP = 0;
    public int playerHP = 100;
    public int level = 1;
    private GameManager gameManager;
    private Vector2 lastMoveDirection;
    private int playerNumber;
    private float horizontalInput;
    private float verticalInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (isPlayerOne) // is player 1
        {
            playerNumber = 1;
        }
        else // is player 2
        {
            playerNumber = 2;
        }
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
            if (Input.GetKeyDown(KeyCode.E)) // fire projectile
            {
                FireProjectile();
            }

            if (Input.GetKeyDown(KeyCode.Q)) // place barricade
            {
                PlaceBarricade();
            }
        }
        else // player 2 controls
        {
            if (Input.GetKeyDown(KeyCode.U)) // fire projectile
            {
                FireProjectile();
            }

            if (Input.GetKeyDown(KeyCode.O)) // place barricade
            {
                PlaceBarricade();
            }
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
        UpdateUI(playerXP, level);
    }

    public void LevelUp()
    {
        level++;
        UpdateUI(playerXP, level);
    }

    private void UpdateUI(int xp, int level)
    {
        if (playerNumber == 1) // player 1
        {
            GameObject.Find("P1LevelText").GetComponent<TextMeshProUGUI>().SetText("" + (level - 1));
            GameObject.Find("P1XPCountText").GetComponent<TextMeshProUGUI>().SetText("P1 XP: " + xp);
        }
        else // player 2
        {
            GameObject.Find("P2LevelText").GetComponent<TextMeshProUGUI>().SetText("" + (level - 1));
            GameObject.Find("P2XPCountText").GetComponent<TextMeshProUGUI>().SetText("P2 XP: " + xp);
        }
    }

    private void FireProjectile()
    {
        // mark the projectile with the player's number
        PlayerProjectile playerProjectile = projectilePrefab.GetComponent<PlayerProjectile>();
        playerProjectile.projectileOwner = playerNumber;

        // shoot projectile toward the last move direction
        playerProjectile.direction = lastMoveDirection;

        // spawn a projectile at location of player
        Instantiate(projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        //Instantiate(effectPrefab, transform.position, projectilePrefab.transform.rotation);
    }
    
    private void PlaceBarricade()
    {
        // mark the barricade with the player's number
        Barricade playerBarricade = barricadePrefabSide.GetComponent<Barricade>();

        // place barricade toward the last move direction
        playerBarricade.direction = lastMoveDirection;

        // spawn an appropriate barricade model at location of player
        if (lastMoveDirection == Vector2.right || lastMoveDirection == Vector2.left) {
            Instantiate(barricadePrefabTop, transform.position, barricadePrefabSide.transform.rotation);
        }
        else if (lastMoveDirection == Vector2.up || lastMoveDirection == Vector2.down) {
            Instantiate(barricadePrefabSide, transform.position, barricadePrefabSide.transform.rotation);
        }
    }
}
