using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    public float attackCooldown = 0.2f; // enemy rate of fire
    public int towerDamageValue = 10; // how much damage it deals to the tower
    public int playerDamageValue = 5; // how much damage it deals to the player
    private Transform target;
    private float nextFireTime = 0f;
    private int pathIndex = 0;  
    private MainTower mainTower;
    
    private bool isStopped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = LevelManager.main.path[pathIndex];
        mainTower = GameObject.Find("Tower").GetComponent<MainTower>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;
            if (pathIndex == LevelManager.main.path.Length)
            {
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    void FixedUpdate()
    {
        if (!isStopped) {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        } else {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Attack when barricade is reached (Placeholder code)
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Wall")) // if impacts a barricade
        { 
            isStopped = true;
            StartCoroutine(AttackBarricade(collision.GetComponent<Barricade>()));
        }

        if (collision.CompareTag("Tower")) // if impacts the main tower
        {
            mainTower.TakeDamage(towerDamageValue);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // keep attacking the player if it remains in contact
        {
            if (Time.time >= nextFireTime)
            {    
                nextFireTime = Time.time + attackCooldown;
                collision.GetComponent<PlayerController>().TakeDamage(playerDamageValue);
            }
        }
    }

    // should create a custom type in which attackable entities inherit from
    // so we do not need to create multiple attack methods (if we ever need to
    // extend the attack function to multiple types of entities)
    private IEnumerator AttackBarricade(Barricade barricade) {
        while (barricade != null && barricade.health > 0)
        {
            barricade.TakeDamage(2);
            yield return new WaitForSeconds(attackCooldown); // wait in between attacks
        }

        isStopped = false;
    }
}
