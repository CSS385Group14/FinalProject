using UnityEngine;

public class EnemyLv2 : BaseEnemy
{
    private int lastHitPlayer;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float nextAttackTime;
    private GameManager gameManager;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private EnemyMovement movement;
    private bool dying = false;
    public PlayerController player; // do not set in Unity!

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextAttackTime = Time.time;
        movement = GetComponent<EnemyMovement>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tower = GameObject.Find("Tower");
    }

    protected override void Update()
    {
        base.Update();

        if (tower == null || !gameManager.gameStart) return;

        // === Check Death ===
        if (currentHP <= 0 && !dying)
        {
            player = GameObject.Find("Player" + lastHitPlayer + "(Clone)")?.GetComponent<PlayerController>();
            if (player != null)
            {
                player.GainXP(statsSO.xpValue);
                player.GainGold(statsSO.goldValue);
            }

            EnemySpawner.onEnemyDestroy?.Invoke();
            Die();
        }

        // === Fetch Players ===
        player1 = GameObject.Find("Player1(Clone)");
        bool isPlayer1Dead = player1?.GetComponent<PlayerController>().isDead ?? true;

        if (gameManager.isCoopEnabled)
        {
            player2 = GameObject.Find("Player2(Clone)");
        }
        bool isPlayer2Dead = player2?.GetComponent<PlayerController>().isDead ?? true;

        // === Distance Checks ===
        float distanceToTower = Vector2.Distance(transform.position, tower.transform.position);
        float distanceToPlayer1 = player1 != null ? Vector2.Distance(transform.position, player1.transform.position) : Mathf.Infinity;
        float distanceToPlayer2 = player2 != null ? Vector2.Distance(transform.position, player2.transform.position) : Mathf.Infinity;

        bool towerInRange = distanceToTower <= statsSO.detectionRangeTower;
        bool player1InRange = player1 != null && distanceToPlayer1 <= statsSO.detectionRangePlayer && Mathf.Abs(transform.position.y - player1.transform.position.y) < 1.5f;
        bool player2InRange = player2 != null && distanceToPlayer2 <= statsSO.detectionRangePlayer && Mathf.Abs(transform.position.y - player2.transform.position.y) < 1.5f;

        bool anyTargetInRange = towerInRange || (player1InRange && !isPlayer1Dead) || (player2InRange && !isPlayer2Dead);

        movement?.StopMovement(anyTargetInRange);
        movement.lockedInCombat = anyTargetInRange;

        // === Targeting and Attacking ===
        if (towerInRange)
        {
            SetTarget(tower);
            TryAttack(tower);
        }
        else if (player1InRange && !isPlayer1Dead)
        {
            SetTarget(player1);
            TryAttack(player1);
        }
        else if (player2InRange && !isPlayer2Dead)
        {
            SetTarget(player2);
            TryAttack(player2);
        }
        else
        {
            currentTarget = null;
        }
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
        animator?.SetTrigger("AttackTrigger");
        currentTarget = target; // Used in animation event
    }

    public override void TakeDamage(int damage, int ownerID)
    {
        animator?.SetTrigger("HurtTrigger");
        lastHitPlayer = ownerID;
        currentHP -= damage;
        Debug.Log($"Enemy took damage: {damage}, Remaining HP: {currentHP}");
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

    protected override void Die()
    {
        dying = true;
        animator.SetTrigger("DeadTrigger");
        Destroy(gameObject, 1f);
    }
}
