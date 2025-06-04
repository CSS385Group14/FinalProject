using UnityEngine;
using UnityEngine.UI;

public class MainTower : MonoBehaviour
{
    private int towerHP = 300;
    [SerializeField]  private GameManager gameManager;
    private float towerCurrentHealth = 0;
    [SerializeField] private Image towerHealthFillImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        towerCurrentHealth = towerHP;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        towerHealthFillImage = GameObject.Find("TowerHPBarFill").GetComponent<Image>();
     
        UpdateHealthUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (towerCurrentHealth < 1) // check health
        {
            Destroy(gameObject);

            // trigger game over state
            gameManager.EndGame();
        }
    }

    public void TakeDamage(int damageTaken)
    {
        towerCurrentHealth = Mathf.Max(towerCurrentHealth - damageTaken, 0);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (towerHealthFillImage != null)
        {
            towerHealthFillImage.fillAmount = towerCurrentHealth / towerHP;
        }
    }
}
