using UnityEngine;
using System.Collections;

public class EnemyLv5 : BaseEnemy
{
    private int lastHitPlayer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float nextFireTime;
    public PlayerController player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireSpeed = 10f;
    private float nextAttackTime;
    private GameManager gameManager;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private EnemyMovement movement;
    private bool isFacingRight = true;
    private Vector3 originalFirePointLocalPos;
    private bool wasAttackingLastFrame = false;
    public int flamesPerInterval = 10;

    public GameObject flamePrefab;
    public float flameDamage = 10f;
    public float healAmount = 5f;
    public Vector2 spawnAreaMin = new Vector2(-10, -5); 
    public Vector2 spawnAreaMax = new Vector2(10, 5);

    private float nextSuperPowerTime;
    private bool isSuperPowerActive = false;

    public float superPowerCooldown = 15f; // cooldown between bursts
    public float superPowerDuration = 3f;  // how long the burst lasts
    public float superPowerInterval = 0.5f; // interval between flames during burst
    private Coroutine flashCoroutine = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        nextSuperPowerTime = Time.time + 2f;

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!gameManager.gameStart || tower == null) return;

        if (currentHP <= 0)
        {
            HandleDeath();
            return; 
        }

        // Refresh player refs
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

        GameObject[] defenses = GameObject.FindGameObjectsWithTag("Defense");
        GameObject closestDefense = null;
        float closestDistDefense = Mathf.Infinity;
        foreach (GameObject def in defenses)
        {
            float dist = Vector2.Distance(transform.position, def.transform.position);
            if (dist < closestDistDefense && dist <= statsSO.detectionRangeDefense)
            {
                closestDefense = def;
                closestDistDefense = dist;
            }
        }
        bool inRangeDefense = closestDefense != null;
        bool targetInRange = inRangeDefense || towerInRange || (player1InRange && !isPlayer1Dead) || (player2InRange && !isPlayer2Dead);
        movement?.StopMovement(targetInRange);
        movement.lockedInCombat = targetInRange;

        // Ranged attacks
        if (inRangeDefense)
        {
            base.SetTargetToDefense();
            TryShoot(closestDefense);
        }
        else if (towerInRange)
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

        // Melee attacks
        if (!isPlayer1Dead && distanceToPlayer1 <= statsSO.attackRange)
        {
            TryAttack(player1);
        }
        else if (!isPlayer2Dead && distanceToPlayer2 <= statsSO.attackRange)
        {
            TryAttack(player2);
        }

        HandleFacingDirection();

        // SuperPower timer and activation
        if (!isSuperPowerActive && Time.time > nextSuperPowerTime)
        {
            StartCoroutine(SuperPowerRoutine());
        }
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
            if (target == null) return;
            currentTarget = target;
            animator?.SetTrigger("ThrowTrigger");
            nextFireTime = Time.time + statsSO.attackRate;
        }
    }

    public override void TakeDamage(int damage, int owner)
    {
        currentHP -= damage;
        lastHitPlayer = owner;

        if (animator != null)
            animator.SetTrigger("HurtTrigger");
    }

    public void DealDamage()
    {
        if (currentTarget == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > statsSO.detectionRangePlayer) return;

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
        animator?.SetTrigger("DeadTrigger");
        player = GameObject.Find("Player" + lastHitPlayer + "(Clone)")?.GetComponent<PlayerController>();
        if (player != null)
        {
            player.GainXP(statsSO.xpValue);
            player.GainGold(statsSO.goldValue);
        }
        EnemySpawner.onEnemyDestroy?.Invoke();
        StartCoroutine(WaitAndDestroy());
    }
    private IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(0.5f); // Wait for death animation to play (adjust to match animation length)
        Destroy(gameObject);
    }

    public void FireProjectile(GameObject target)
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

    public void Throw()
    {
       
        if (currentTarget == null)
        {
           
            return;
        }
      
        FireProjectile(currentTarget);
    }

    private void TryAttack(GameObject target)
    {
        if (Time.time > nextAttackTime)
        {
            Attack(target);
            nextAttackTime = Time.time + statsSO.attackRate;
        }
    }

    private void Attack(GameObject target)
    {
        int randomIndex = Random.Range(0, 3);
        animator?.SetInteger("AttackIndex", randomIndex);
        animator?.SetTrigger("AttackTrigger");
        currentTarget = target; // Used in animation event
    }

    private IEnumerator SuperPowerRoutine()
    {
        isSuperPowerActive = true;

        animator?.SetTrigger("SuperPowerTrigger");

        float elapsed = 0f;

        while (elapsed < superPowerDuration)
        {
            // Spawn multiple flames per interval
            for (int i = 0; i < flamesPerInterval; i++)
            {
                SpawnFlame();
            }

            yield return new WaitForSeconds(superPowerInterval);
            elapsed += superPowerInterval;
        }

        nextSuperPowerTime = Time.time + superPowerCooldown;
        isSuperPowerActive = false;
    }

    private void SpawnFlame()
    {
        Vector2 randomPosition = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

        GameObject flame = Instantiate(flamePrefab, randomPosition, Quaternion.identity);

        // Destroy the flame object after 3 seconds
        Destroy(flame, superPowerDuration);

        // Immediately check for damage/heal around the flame position
        CheckFlameHit(randomPosition);
    }


    private void CheckFlameHit(Vector2 position)
    {
        // Damage players and heal enemy if they are inside the flame radius
        float flameRadius = 1.0f; // adjust as needed

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, flameRadius);
        foreach (Collider2D collider in hitColliders)
        {
            PlayerController pc = collider.GetComponent<PlayerController>();
            if (pc != null && !pc.isDead)
            {
          
                pc.TakeDamage((int)flameDamage);
                
             
            }
            DefenseTower defense = collider.GetComponent<DefenseTower>();
            if (defense != null) {
                
                defense.TakeDamage((int)flameDamage); 
            }
            
        }


    }


}
