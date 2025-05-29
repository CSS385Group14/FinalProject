using UnityEngine;

public class Tower : MonoBehaviour
{
    public float health = 100;
    public GameObject levelManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) {
            levelManager.GetComponent<LevelManager>().gameOver();
            Destroy(gameObject);
        }
    }
}
