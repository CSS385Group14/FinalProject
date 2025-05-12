using UnityEngine;

// limitations:
// - does not destroy itself, use a trigger to destroy this if
//   it flies off screen w/o hitting an enemy
public class Projectile : MonoBehaviour
{
    public int projectileDamage = 10;
    public float speed = 20f;
    public int projectileOwner;
    public Vector2 direction;
    public int despawnX = 20;
    public int despawnY = 12;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // the projectile is moves up when instantiated
        transform.Translate(direction * Time.deltaTime * speed);

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
}
