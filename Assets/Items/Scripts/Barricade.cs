using UnityEngine;

public class Barricade : MonoBehaviour
{
    public Vector2 direction;

    public float health = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage) {
        health -= damage;

        if(health <= 0) {
            Destroy(gameObject);
        }
    }
}
