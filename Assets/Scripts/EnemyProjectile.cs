using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public int damage = 10;
    public float maxDistance = 10f;
    private Vector2 startPosition;
    void Awake()
    {
        GetComponent<Rigidbody2D>().gravityScale = 0;
    }
    private void Start()
    {
        // Store the starting position when the projectile is instantiated
        startPosition = transform.position;
    }

    private void Update()
    {
        // Check if the projectile has moved the desired distance
        if (Vector2.Distance(startPosition, transform.position) >= maxDistance)
        {
            Destroy(gameObject); // Destroy the projectile
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)

    {
        if (collision.CompareTag("Tower"))
        {
            MainTower tower = collision.GetComponent<MainTower>();
            if (tower != null)
            {
                tower.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
