using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 30; // how much health the enemy has
    public int xpValue = 100; // amount of xp gained by the player when killed
    public PlayerController player; // do not set in unity!
    private int lastHitPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (health < 1) // check health
        {
            // let the player who killed this enemy receive XP of amount x
            // we add "(Clone)" because the instantiated player game object is a clone of a prefab
            player = GameObject.Find("Player" + lastHitPlayer + "(Clone)").GetComponent<PlayerController>();
            player.GainXP(xpValue);

            EnemySpawner.onEnemyDestroy.Invoke();
            Destroy(gameObject);
        }
    }

    // called when the enemy is hit by a projectile
    public void TakeDamage(int damage, int owner)
    {
        lastHitPlayer = owner;
        health -= damage;
    }
}
