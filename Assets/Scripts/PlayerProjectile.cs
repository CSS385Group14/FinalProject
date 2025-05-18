using UnityEngine;

// limitations:
// - does not destroy itself, use a trigger to destroy this if
//   it flies off screen w/o hitting an enemy
public class PlayerProjectile : MonoBehaviour
{
    public int projectileOwner;
    public Vector2 direction;
    public int despawnX = 23;
    public int despawnY = 13;
    private int projectileDamage = 10;
    private float projectileSpeed = 20f;

    // Update is called once per frame
    void Update()
    {
        // the projectile is moves up when instantiated
        transform.Translate(direction * Time.deltaTime * projectileSpeed);

        // destroy the projectile if off-screen
        if (transform.position.x < -despawnX || transform.position.x > despawnX ||
            transform.position.y < -despawnY || transform.position.y > despawnY)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // check if projectile hits an Enemy object, destroy projectile
        // and let enemy take damage
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(projectileDamage, projectileOwner);

            Destroy(gameObject);
        }
    }

    public void SetDamage(int damage)
    {
        projectileDamage = damage;
    }

    public void SetSpeed(float speed)
    {
        projectileSpeed = speed;
    }
}
