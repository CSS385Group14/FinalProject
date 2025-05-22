using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100; // how much health the enemy has
    public int xpValue = 100; // amount of xp gained by the player when killed
    public int goldValue = 2; // amount of gold gained by the player when killed
    public PlayerController player; // do not set in unity!
    private int lastHitPlayer;
    Animator animator;
    public int health;
    private SpriteRenderer spriteRenderer;
    public float detectionRangePlayer = 2f;
    public float detectionRangeTower = 7f;
    private float nextFireTime;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireSpeed = 10f;
    public float fireRate = 1f;
    private GameManager gameManager;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private EnemyMovement movement;

    void Start()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextFireTime = Time.time;
        movement = GetComponent<EnemyMovement>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tower = GameObject.Find("Tower");
    }

    public void SetTargets(GameObject p1, GameObject p2, GameObject tower)
    {
        player1 = p1;
        player2 = p2;
        this.tower = tower;
    }

    private void SetTargetToPlayer1()
    {
        currentTarget = player1;
    }

    private void SetTargetToPlayer2()
    {
        currentTarget = player2;
    }

    private void SetTargetToTower()
    {
        currentTarget = tower;
    }


    void Update()
    {
        if (tower != null) {
            if (health < 1)
            {
                player = GameObject.Find("Player" + lastHitPlayer + "(Clone)")?.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.GainXP(xpValue);
                    player.GainGold(goldValue);
                }
                EnemySpawner.onEnemyDestroy?.Invoke();
                Destroy(gameObject);
                return;
            }

                if (gameManager.gameStart)
                {
                    bool isPlayer1Dead;
                    bool isPlayer2Dead = false;

                    // find player objects at runtime and get death state
                    player1 = GameObject.Find("Player1(Clone)");
                    isPlayer1Dead = player1.GetComponent<PlayerController>().isDead;
                    if (gameManager.isCoopEnabled)
                    {
                        player2 = GameObject.Find("Player2(Clone)");
                        isPlayer2Dead = player2.GetComponent<PlayerController>().isDead;
                    }

                    // === Calculate distances ===
                    float distanceToTower = Vector2.Distance(transform.position, tower.transform.position);
                    float distanceToPlayer1 = Vector2.Distance(transform.position, player1.transform.position);
                    float distanceToPlayer2 = player2 != null ? Vector2.Distance(transform.position, player2.transform.position) : Mathf.Infinity;

                    // === Range checks ===
                    bool towerInRange = distanceToTower <= detectionRangeTower;
                    bool player1InRange = distanceToPlayer1 <= detectionRangePlayer && Mathf.Abs(transform.position.y - player1.transform.position.y) < 1.5f;
                    bool player2InRange = player2 != null && distanceToPlayer2 <= detectionRangePlayer && Mathf.Abs(transform.position.y - player2.transform.position.y) < 1.5f;

                    bool anyTargetInRange = towerInRange || (player1InRange && !isPlayer1Dead) || (player2InRange && !isPlayer2Dead);

                    // === Movement handling ===
                    movement?.StopMovement(anyTargetInRange);
                    movement.lockedInCombat = anyTargetInRange;

                    // === Targeting priority ===
                    if (towerInRange)
                    {
                        SetTargetToTower();
                        TryShoot(tower);
                    }
                    else if (player1InRange && !isPlayer1Dead)
                    {
                        SetTargetToPlayer1();
                        TryShoot(player1);
                    }
                    else if (player2InRange && !isPlayer2Dead)
                    {
                        SetTargetToPlayer2();
                        TryShoot(player2);
                    }
                    else
                    {
                        currentTarget = null;
                    }

                }
            }
    }

    private void TryShoot(GameObject target)
    {
        if (Time.time > nextFireTime)
        {
            FireProjectile(target);
            nextFireTime = Time.time + fireRate;
        }
    }

    // Called when the enemy is hit by a projectile
    public void TakeDamage(int damage, int owner)
    {
<<<<<<< HEAD
        //Debug.Log("Animator Hurt Trigger Called");
=======
        Debug.Log("Animator Hurt Trigger Called");
>>>>>>> 1bb0681f6ccf692cfc86d9bcee119cb9526059c3
        animator.SetTrigger("HurtTrigger");
        lastHitPlayer = owner;
        health -= damage;
        Debug.Log("Enemy took damage: " + damage + ", Remaining HP: " + health);
        Color color = spriteRenderer.color;
        color.a *= 0.9f;
        spriteRenderer.color = color;
    }

    // Determines the closest target (either player or tower)
    private GameObject GetClosestTarget(float distanceToPlayer1, float distanceToPlayer2, float distanceToTower)
    {
        // Prioritize the closest player and then the tower
        if (distanceToPlayer1 < distanceToPlayer2 && distanceToPlayer1 < distanceToTower)
        {
            return player1;
        }
        else if (distanceToPlayer2 < distanceToPlayer1 && distanceToPlayer2 < distanceToTower)
        {
            return player2;
        }
        else
        {
            return tower;
        }
    }

    // Fires a projectile at the specified target (player or tower)
    void FireProjectile(GameObject target)
    {
        // Instantiate the projectile
        if (projectilePrefab != null && firePoint != null)
        {
            //animator.SetTrigger("AttackTrigger");
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            // Add velocity to the projectile towards the target
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Move the projectile in the direction of the target (in 2D space)
                Vector2 direction = (target.transform.position - firePoint.position).normalized;
                rb.linearVelocity = direction * fireSpeed; // Adjust speed here
            }
        }
        else
        {
            Debug.LogError("No valid prefab or firepoint found");
        }
    }
}
