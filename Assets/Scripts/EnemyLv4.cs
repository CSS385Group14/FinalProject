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

    private float chaseRangePlayer = 5f;

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<EnemyChase>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tower = GameObject.Find("Tower");

        nextAttackTime = Time.time;
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
        //
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
            currentTarget = null;
            HandleChase(distanceToPlayer1, distanceToPlayer2, isPlayer1Dead, isPlayer2Dead);
        }
    }

    private void HandleChase(float distanceToPlayer1, float distanceToPlayer2, bool isPlayer1Dead, bool isPlayer2Dead)
    {
        bool player1Chase = distanceToPlayer1 <= chaseRangePlayer && !isPlayer1Dead;
        bool player2Chase = distanceToPlayer2 <= chaseRangePlayer && !isPlayer2Dead;

        if (player1Chase || player2Chase)
        {
            GameObject chaseTarget = player1Chase ? player1 : player2;
            if (player1Chase && player2Chase)
            {
                chaseTarget = (distanceToPlayer1 < distanceToPlayer2) ? player1 : player2;
            }
            currentTarget = chaseTarget;
            movement.ChaseTarget(chaseTarget.transform);
        }
        else
        {
            movement.ResumeWaypoints();
            currentTarget = tower;
        }
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
        if (currentTarget == null) return;

        float attackRange = statsSO.detectionRangePlayer;
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > attackRange) return;

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
