using UnityEngine;

public class EnemyLv1 : BaseEnemy
{
    private int lastHitPlayer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float nextFireTime;
    public PlayerController player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireSpeed = 10f;

    private GameManager gameManager;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private EnemyMovement movement;
    private bool isFacingRight = true;
    private Vector3 originalFirePointLocalPos;
    private bool wasAttackingLastFrame = false;

    protected override void Start()
    {
        base.Start(); // initialize base stats & components

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextFireTime = Time.time;

        movement = GetComponent<EnemyMovement>();

        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManager>();
        tower = GameObject.Find("Tower");
        movement.SetStats(statsSO);
        isFacingRight = true;
        spriteRenderer.flipX = false;
    }


    protected override void Update()
    {
        base.Update(); // call base movement

        if (!gameManager.gameStart || tower == null) return;

        if (currentHP <= 0)
        {
            HandleDeath();
        }

        // Refresh player refs (in case of respawn/despawn)
        player1 = GameObject.Find("Player1(Clone)");
        player2 = gameManager.isCoopEnabled ? GameObject.Find("Player2(Clone)") : null;

        bool isPlayer1Dead = player1?.GetComponent<PlayerController>().isDead ?? true;
        bool isPlayer2Dead = player2?.GetComponent<PlayerController>().isDead ?? true;

        float distanceToTower = Vector2.Distance(transform.position, tower.transform.position);
        float distanceToPlayer1 = player1 != null ? Vector2.Distance(transform.position, player1.transform.position) : Mathf.Infinity;
        float distanceToPlayer2 = player2 != null ? Vector2.Distance(transform.position, player2.transform.position) : Mathf.Infinity;

        bool towerInRange = distanceToTower <= statsSO.detectionRangeTower;
        bool player1InRange = !isPlayer1Dead && distanceToPlayer1 <= statsSO.detectionRangePlayer &&
                              Mathf.Abs(transform.position.y - player1.transform.position.y) < 1.5f;
        bool player2InRange = !isPlayer2Dead && distanceToPlayer2 <= statsSO.detectionRangePlayer &&
                              Mathf.Abs(transform.position.y - player2.transform.position.y) < 1.5f;

        bool anyTargetInRange = towerInRange || player1InRange || player2InRange;
        movement?.StopMovement(anyTargetInRange);
        movement.lockedInCombat = anyTargetInRange;

        if (towerInRange)
        {
            base.SetTargetToTower();
            TryShoot(tower);
        }
        else if (player1InRange)
        {
            base.SetTargetToPlayer1();
            TryShoot(player1);
        }
        else if (player2InRange)
        {
            base.SetTargetToPlayer2();
            TryShoot(player2);
        }
        else
        {
            currentTarget = null;
        }
        HandleFacingDirection();

    }
    private void HandleFacingDirection()
    {
        bool isAttacking = currentTarget != null;

        // When attacking: Face the target
        if (isAttacking)
        {
            FaceTarget(currentTarget.transform.position);
            wasAttackingLastFrame = true;
        }
        // When stopping attack: Return to default facing
        else if (wasAttackingLastFrame)
        {
            ResetToDefaultFacing();
            wasAttackingLastFrame = false;
        }
        // Maintain facing during movement
        else if (movement != null && movement.rb != null && movement.rb.linearVelocity.magnitude > 0.1f)
        {
            // Face movement direction
            bool shouldFaceRight = movement.rb.linearVelocity.x > 0;
            if (shouldFaceRight != isFacingRight)
            {
                SetFacing(shouldFaceRight);
            }
        }
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector2 direction = targetPosition - transform.position;
        bool shouldFaceRight = direction.x > 0;

        if (shouldFaceRight != isFacingRight)
        {
            SetFacing(shouldFaceRight);
        }
    }

    private void SetFacing(bool faceRight)
    {
        isFacingRight = faceRight;
        spriteRenderer.flipX = !faceRight;
    }

    private void ResetToDefaultFacing()
    {
        SetFacing(true); // Default to right-facing
    }

    private void TryShoot(GameObject target)
    {
        if (Time.time > nextFireTime)
        {
            // Ensure target exists
            if (target == null) return;

            FireProjectile(target);
            nextFireTime = Time.time + statsSO.attackRate;
        }
    }

    public override void TakeDamage(int damage, int owner)
    {
        currentHP -= damage;
        lastHitPlayer = owner;

        if (animator != null)
            animator.SetTrigger("HurtTrigger");

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a *= 0.9f;
            spriteRenderer.color = color;
        }
    }

    private void HandleDeath()
    {
        player = GameObject.Find("Player" + lastHitPlayer + "(Clone)")?.GetComponent<PlayerController>();
        if (player != null)
        {
            player.GainXP(statsSO.xpValue);
            player.GainGold(statsSO.goldValue);
        }
        EnemySpawner.onEnemyDestroy?.Invoke();
        Destroy(gameObject);
    }

    private void FireProjectile(GameObject target)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Projectile or firePoint missing.");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = (target.transform.position - firePoint.position).normalized;
            rb.linearVelocity = direction * fireSpeed;
        }
    }
}
