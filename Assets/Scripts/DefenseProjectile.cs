using UnityEngine;

public class DefenseProjectile : MonoBehaviour
{
    public float maxLifetime = 5f;
    public float hitRadius = 1f;
    private float speed;
    private int damage;
    private int playerNumber;
    private Vector2 moveDirection;
    private Transform target;
    private bool initialized = false;

    public void SetTarget(Transform target)
    {
        this.target = target;

        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
            initialized = true;
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetOwner(int playerNumber)
    {
        this.playerNumber = playerNumber;
    }

    void Update()
    {
        if (!initialized)
            return;

        // Move in the originally locked direction
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // Lifetime limit in case the projectile misses
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // If the target still exists and is within a hit radius
        if (target != null && Vector2.Distance(transform.position, target.position) < hitRadius)
        {
            // TODO: Apply damage here, e.g., target.GetComponent<Enemy>().TakeDamage(damage);
            Enemy enemy = target.GetComponent<Enemy>();
            enemy.TakeDamage(damage, playerNumber);
            Destroy(gameObject);
        }
    }
}
