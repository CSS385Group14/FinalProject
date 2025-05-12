using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;  
    public float attackCooldown = 0.2f;
    private bool isStopped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = LevelManager.main.path[pathIndex];
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
        if (collision.gameObject.CompareTag("Wall")) {
            isStopped = true;
            StartCoroutine(AttackBarricade(collision.gameObject.GetComponent<Barricade>()));
        }
    }

    private IEnumerator AttackBarricade(Barricade barricade) {
    while (barricade != null && barricade.health > 0) {
        barricade.TakeDamage(2);
        yield return new WaitForSeconds(attackCooldown); // wait in between attacks
    }

        isStopped = false;
    }
}
