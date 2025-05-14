using UnityEngine;

public class MainTower : MonoBehaviour
{
    public int towerHP;
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (towerHP < 1) // check health
        {
            Destroy(gameObject);

            // trigger game over state
            gameManager.EndGame();
        }
    }

    public void TakeDamage(int damageTaken)
    {
        towerHP -= damageTaken;
        transform.GetComponent<TowerHealthbarController>().TakeTowerDamage(damageTaken);
    }
}
