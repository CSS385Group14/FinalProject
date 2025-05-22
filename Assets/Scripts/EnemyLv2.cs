using UnityEngine;

public class EnemyLv2 : MonoBehaviour
{
    public int maxHealth = 500; // how much health the enemy has
    public int xpValue = 150; // amount of xp gained by the player when killed
    public int goldValue = 5; // amount of gold gained by the player when killed
    public PlayerController player; // do not set in unity!
    private int lastHitPlayer;
    Animator animator;
    public int health;
    private SpriteRenderer spriteRenderer;
    private float detectionRangePlayer = 1f;
    private float detectionRangeTower = 6f;
    private float nextAttackTime;
    private int damageAmount = 10;
    private float attackSpeed = 1f;
    public float attackRate = 1f;
    private GameManager gameManager;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private EnemyMovement movement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextAttackTime = Time.time;
        movement = GetComponent<EnemyMovement>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tower = GameObject.Find("Tower");
    }

    // Update is called once per frame
    void Update()
    {
        if (tower != null)
        {
            if (health < 1)
            {
                player = GameObject.Find("Player" + lastHitPlayer + "(Clone)")?.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.GainXP(xpValue);
                    player.GainGold(goldValue);
                }
                //animator.SetTrigger("DeadTrigger");
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
                Debug.Log($"Distance to Tower: {distanceToTower}, TowerInRange: {towerInRange}");
                // === Movement handling ===
                movement?.StopMovement(anyTargetInRange);
                movement.lockedInCombat = anyTargetInRange;

                // === Targeting priority ===
                if (towerInRange)
                {
                    SetTargetToTower();
                    TryAttack(tower);
                }
                else if (player1InRange && !isPlayer1Dead)
                {
                    SetTargetToPlayer1();
                    TryAttack(player1);
                }
                else if (player2InRange && !isPlayer2Dead)
                {
                    SetTargetToPlayer2();
                    TryAttack(player2);
                }
                else
                {
                    currentTarget = null;
                }

            }
        }
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
    private void TryAttack(GameObject target)
    {
        Debug.Log($"Trying to attack {target.name}");
        if (Time.time > nextAttackTime)
        {
            Attack(target);
            nextAttackTime = Time.time + attackRate;
        }
    }
    private void Attack(GameObject target)
    {
        animator?.SetTrigger("AttackTrigger");
        currentTarget = target; // Store target to use later in animation event
    }
    // Called when the enemy is hit by a projectile
    public void TakeDamage(int damage, int owner)
    {
        //Debug.Log("Animator Hurt Trigger Called");
        animator?.SetTrigger("HurtTrigger");
        lastHitPlayer = owner;
        health -= damage;
        Debug.Log("Enemy took damage: " + damage + ", Remaining HP: " + health);

    }
    public void DealDamage()
    {
        if (currentTarget == null) return;

        // Check distance to currentTarget before dealing damage
        float attackRange = detectionRangePlayer; // or another appropriate range
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

        // Only deal damage if target is still in attack range
        if (distanceToTarget > attackRange)
        {
            return; // Target moved out of range, don't deal damage
        }

        PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();
        MainTower targetTower = currentTarget.GetComponent<MainTower>();

        // Also, optionally check if target is alive before applying damage
        if (targetPlayer != null && !targetPlayer.isDead)
        {
            targetPlayer.TakeDamage(damageAmount);
        }
        else if (targetTower != null)
        {
            targetTower.TakeDamage(damageAmount);
        }
    }
}
