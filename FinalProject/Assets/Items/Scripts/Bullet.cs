using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float damage = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setDamage(float damage) {
        this.damage = damage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //collision.harm(damage); //set up trigger on enemies to take damage when collided with bullet
        Debug.Log("Hit for " + damage + " damage");
    }
}
