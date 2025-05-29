using UnityEngine;

public class EnemyLv3 : BaseEnemy
{
    private int lastHitPlayer;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    public GameObject projectilePrefab;
    public Transform firePoint;
    private bool dying = false;
    private float nextFireTime;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private EnemyMovement movement;
    private GameManager gameManager;

    protected override void Start()
    {
        base.Start(); // initializes currentHP from BaseEnemy

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        movement = GetComponent<EnemyMovement>();
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        tower = GameObject.Find("Tower");

        nextFireTime = Time.time;
    }

    public void SetTargets(GameObject p1, GameObject p2, GameObject tower)
    {
        player1 = p1;
        player2 = p2;
        this.tower = tower;
    }

    private void SetTarget(GameObject target)
    {
        currentTarget = target;
    }

    protected override void Update()
    {
        base.Update();

        if (!gameManager || !gameManager.gameStart || !tower) return;

        if (currentHP <= 0 && !dying)
        {
            Die();
        }

        // Find players
        player1 = GameObject.Find("Player1(Clone)");
        player2 = gameManager.isCoopEnabled ? GameObject.Find("Player2(Clone)") : null;

        bool isPlayer1Dead = player1?.GetComponent<PlayerController>().isDead ?? true;
        bool isPlayer2Dead = player2?.GetComponent<PlayerController>().isDead ?? true;

        float distToP1 = player1 ? Vector2.Distance(transform.position, player1.transform.position) : Mathf.Infinity;
        float distToP2 = player2 ? Vector2.Distance(transform.position, player2.transform.position) : Mathf.Infinity;
        float distToTower = Vector2.Distance(transform.position, tower.transform.position);

        bool inRangeP1 = distToP1 <= statsSO.detectionRangePlayer && Mathf.Abs(transform.position.y - player1.transform.position.y) < 1.5f;
        bool inRangeP2 = player2 && distToP2 <= statsSO.detectionRangePlayer && Mathf.Abs(transform.position.y - player2.transform.position.y) < 1.5f;
        bool inRangeTower = distToTower <= statsSO.detectionRangeTower;

        bool targetInRange = inRangeTower || (inRangeP1 && !isPlayer1Dead) || (inRangeP2 && !isPlayer2Dead);
        movement?.StopMovement(targetInRange);
        movement.lockedInCombat = targetInRange;

        if (inRangeTower)
        {
            SetTarget(tower);
            TryShoot(tower);
        }
        else if (inRangeP1 && !isPlayer1Dead)
        {
            SetTarget(player1);
            TryShoot(player1);
        }
        else if (inRangeP2 && !isPlayer2Dead)
        {
            SetTarget(player2);
            TryShoot(player2);
        }
        else
        {
            currentTarget = null;
        }
    }

    private void TryShoot(GameObject target)
    {
        if (Time.time >= nextFireTime)
        {
            FireProjectile(target);
            nextFireTime = Time.time + statsSO.attackRate;
        }
    }

    private void FireProjectile(GameObject target)
    {
        if (projectilePrefab && firePoint && target)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();

            if (rb)
            {
                Vector2 dir = (target.transform.position - firePoint.position).normalized;
                rb.linearVelocity = dir * statsSO.moveSpeed;
            }
        }
    }

    public override void TakeDamage(int damage, int ownerID)
    {
        currentHP -= damage;
        lastHitPlayer = ownerID;
        animator?.SetTrigger("HurtTrigger");
        Debug.Log($"{name} took {damage} damage. Remaining HP: {currentHP}");
    }

    protected override void Die()
    {
        dying = true;
        if (animator)
        {
            animator.SetFloat("AnimationSpeed", 2.0f); // Play death faster
            animator.SetTrigger("DieTrigger");
        }

        PlayerController killer = GameObject.Find($"Player{lastHitPlayer}(Clone)")?.GetComponent<PlayerController>();
        if (killer)
        {
            killer.GainXP(statsSO.xpValue);
            killer.GainGold(statsSO.goldValue);
        }

        EnemySpawner.onEnemyDestroy?.Invoke();
        Destroy(gameObject, 0.5f); // Delay so death animation shows
    }
}
