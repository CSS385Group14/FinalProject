using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Rigidbody2D rb;

    [Header("Attributes")]
    private EnemyStats statsSO;
    private SpriteRenderer spriteRenderer;
    public float attackCooldown = 0.2f; // enemy rate of fire
    //public int towerDamageValue = 10; // how much damage it deals to the tower
    //public int playerDamageValue = 5; // how much damage it deals to the player
    private Transform target;
    private float nextFireTime = 0f;
    private Transform[] path;
    private int pathIndex = 0;  
    private MainTower mainTower;

    private bool stopRequestedExternally = false;
    private bool stoppedByBarricade = false;

    public bool lockedInCombat { get; set; } = false;
    private bool isStopped => stopRequestedExternally || stoppedByBarricade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //mainTower = GameObject.Find("Tower").GetComponent<MainTower>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InitPath(Transform[] pathArray)
    {
        path = pathArray;
        pathIndex = 0;

        if (path.Length > 0)
        {
            transform.position = path[0].position;
            target = path[1];
            pathIndex = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null || target == null || isStopped) return;

        if (Vector2.Distance(transform.position, target.position) <= 0.1f)
        {
            pathIndex++;
            if (pathIndex >= path.Length)
            {
                EnemySpawner.onEnemyDestroy?.Invoke();
                Destroy(gameObject);
                return;
            }

            target = path[pathIndex];
        }
        //UpdateFacingDirection();
    }
    public void SetStats(EnemyStats stats)
    {
        statsSO = stats;
    }

    public void StopMovement(bool stop)
    {
        stopRequestedExternally = stop;
    }

    void FixedUpdate()
    {
        // if (isStopped)
        // {
        //     rb.linearVelocity = Vector2.zero; // Ensure it stays zero every frame while stopped
        //     return;
        // }

        if (target == null || path == null) return;

        // Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        // rb.linearVelocity = direction * moveSpeed;

        // temp solution
        if (!isStopped)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * statsSO.moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

    }

    // Attack when barricade is reached (Placeholder code)
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Wall")) // if impacts a barricade
        {
            stoppedByBarricade = true;
            StartCoroutine(AttackBarricade(collision.GetComponent<Barricade>()));
        }


    }
    //We dont need this since it is already handle attack player in enemy scripts
    /*void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // keep attacking the player if it remains in contact
        {
            if (Time.time >= nextFireTime)
            {    
                nextFireTime = Time.time + attackCooldown;
                collision.GetComponent<PlayerController>().TakeDamage(playerDamageValue);
            }
        }*/
    

    // should create a custom type in which attackable entities inherit from
    // so we do not need to create multiple attack methods (if we ever need to
    // extend the attack function to multiple types of entities)
    private IEnumerator AttackBarricade(Barricade barricade) {
        while (barricade != null && barricade.health > 0)
        {
            barricade.TakeDamage(2);
            yield return new WaitForSeconds(attackCooldown); // wait in between attacks
        }
        stoppedByBarricade = false;
    }
}
