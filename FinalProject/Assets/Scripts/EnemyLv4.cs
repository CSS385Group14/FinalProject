using UnityEngine;

public class EnemyLv4 : BaseEnemy
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyChase movement;
    private GameManager gameManager;

    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;

    private float nextAttackTime;
    private int lastHitPlayer;
    private GameObject attackTarget;



    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<EnemyChase>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tower = GameObject.Find("Tower");


        nextAttackTime = Time.time;
        StartChasing(); // Start chasing immediately
    }
    private void StartChasing()
    {
        UpdateTargets();
        bool isPlayer1Dead = player1?.GetComponent<PlayerController>().isDead ?? true;
        bool isPlayer2Dead = player2?.GetComponent<PlayerController>().isDead ?? true;
        float distanceToPlayer1 = player1 != null ? Vector2.Distance(transform.position, player1.transform.position) : Mathf.Infinity;
        float distanceToPlayer2 = player2 != null ? Vector2.Distance(transform.position, player2.transform.position) : Mathf.Infinity;

        HandleChase(distanceToPlayer1, distanceToPlayer2, isPlayer1Dead, isPlayer2Dead);
        movement.SetStats(statsSO);

    }

    protected override void Update()
    {
        base.Update();

        if (tower == null || !gameManager.gameStart) return;

        if (currentHP <= 0)
        {
            HandleDeath();
        }

        UpdateTargets();
        HandleCombat();
        UpdateFacingDirection();

    }
    private void UpdateFacingDirection()
    {
        // Check if we have a valid sprite renderer
        if (spriteRenderer == null) return;

        // Determine direction based on movement or target
        if (movement.rb.linearVelocity.magnitude > 0.1f)
        {
            // Moving - face movement direction
            spriteRenderer.flipX = movement.rb.linearVelocity.x < 0;
        }
        else if (currentTarget != null)
        {
            // Stationary - face current target
            Vector2 directionToTarget = currentTarget.transform.position - transform.position;
            spriteRenderer.flipX = directionToTarget.x < 0;
        }
        // If neither moving nor targeting, maintain current facing
    }

    private void UpdateTargets()
    {
        player1 = GameObject.Find("Player1(Clone)");
        if (gameManager.isCoopEnabled)
        {
            player2 = GameObject.Find("Player2(Clone)");
        }
    }

    private void HandleCombat()
    {
        bool isPlayer1Dead = player1?.GetComponent<PlayerController>().isDead ?? true;
        bool isPlayer2Dead = player2?.GetComponent<PlayerController>().isDead ?? true;

        float distanceToTower = Vector2.Distance(transform.position, tower.transform.position);
        float distanceToPlayer1 = player1 != null ? Vector2.Distance(transform.position, player1.transform.position) : Mathf.Infinity;
        float distanceToPlayer2 = player2 != null ? Vector2.Distance(transform.position, player2.transform.position) : Mathf.Infinity;

        bool towerInRange = distanceToTower <= statsSO.detectionRangeTower;
        bool player1InRange = distanceToPlayer1 <= statsSO.detectionRangePlayer && Mathf.Abs(transform.position.y - player1.transform.position.y) < 1.5f;
        bool player2InRange = distanceToPlayer2 <= statsSO.detectionRangePlayer && Mathf.Abs(transform.position.y - player2.transform.position.y) < 1.5f;

        bool anyTargetInRange = towerInRange || (player1InRange && !isPlayer1Dead) || (player2InRange && !isPlayer2Dead);

        movement?.StopMovement(anyTargetInRange);
        movement.lockedInCombat = anyTargetInRange;

        if (towerInRange)
        {
            base.SetTargetToTower();
            TryAttack(tower);
        }
        else if (player1InRange && !isPlayer1Dead)
        {
            base.SetTargetToPlayer1();
            TryAttack(player1);
        }
        else if (player2InRange && !isPlayer2Dead)
        {
            base.SetTargetToPlayer2();
            TryAttack(player2);
        }
        else
        {

            HandleChase(distanceToPlayer1, distanceToPlayer2, isPlayer1Dead, isPlayer2Dead);
        }
    }

    private void HandleChase(float distanceToPlayer1, float distanceToPlayer2, bool isPlayer1Dead, bool isPlayer2Dead)
    {
        GameObject chaseTarget = null;

        // Always choose closest alive player or tower
        if (!isPlayer1Dead || !isPlayer2Dead)
        {
            if (!isPlayer1Dead && !isPlayer2Dead)
            {
                chaseTarget = (distanceToPlayer1 < distanceToPlayer2) ? player1 : player2;
            }
            else if (!isPlayer1Dead)
            {
                chaseTarget = player1;
            }
            else
            {
                chaseTarget = player2;
            }
        }
        else
        {
            chaseTarget = tower; // Both players dead, target tower
        }

        if (chaseTarget != null)
        {
            currentTarget = chaseTarget;
            movement.ChaseTarget(chaseTarget.transform);
        }
        else
        {
            // Fallback if no valid target
            movement.ResumeWaypoints();
            currentTarget = tower;
        }
    }

    private void TryAttack(GameObject target)
    {
        if (Time.time > nextAttackTime)
        {
            attackTarget = target;
            Attack(target);
            nextAttackTime = Time.time + statsSO.attackRate;
        }
    }

    private void Attack(GameObject target)
    {
        animator?.SetTrigger("AttackTrigger");
        currentTarget = target;
    }

    public override void TakeDamage(int damage, int ownerID)
    {
        currentHP -= damage;
        animator?.SetTrigger("HurtTrigger");
        lastHitPlayer = ownerID;
    }

    public void DealDamage()
    {
        if (attackTarget == null) return;

        PlayerController targetPlayer = currentTarget.GetComponent<PlayerController>();
        MainTower targetTower = currentTarget.GetComponent<MainTower>();

        if (targetPlayer != null && !targetPlayer.isDead)
        {
            targetPlayer.TakeDamage(statsSO.damageAmount);
        }
        else if (targetTower != null)
        {
            targetTower.TakeDamage(statsSO.damageAmount);
        }
    }

    private void HandleDeath()
    {
        PlayerController player = GameObject.Find("Player" + lastHitPlayer + "(Clone)")?.GetComponent<PlayerController>();
        Debug.Log("Last hit player: " + lastHitPlayer);
        if (player != null)
        {
            player.GainXP(statsSO.xpValue);
            player.GainGold(statsSO.goldValue);
        }
        EnemySpawner.onEnemyDestroy?.Invoke();
        Destroy(gameObject);
    }
}
