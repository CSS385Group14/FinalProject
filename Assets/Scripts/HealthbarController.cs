using UnityEngine;
using UnityEngine.UI;

public class HealthbarController : MonoBehaviour
{
    [Range(0, 100)]
    public float playerMaxHealth = 100f;
    public float towerMaxHealth = 50f;
    public float playerCurrentHealth;
    public float towerCurrentHealth;
    public int playerNumber = 1;
    private Image playerHealthFillImage;
    private Image towerHealthFillImage;
    
    private void Start()
    {
        playerHealthFillImage = GameObject.Find("P" + playerNumber + "HPBarFill").GetComponent<Image>();
        towerHealthFillImage = GameObject.Find("TowerHPBarFill").GetComponent<Image>();
        playerCurrentHealth = playerMaxHealth;
        towerCurrentHealth = towerMaxHealth;
        UpdateHealthUI();
    }

    public void TakePlayerDamage(float amount)
    {
        playerCurrentHealth = Mathf.Max(playerCurrentHealth - amount, 0);
        UpdateHealthUI();
    }

    public void TakeTowerDamage(float amount)
    {
        towerCurrentHealth = Mathf.Max(towerCurrentHealth - amount, 0);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (playerHealthFillImage != null)
        {
            playerHealthFillImage.fillAmount = playerCurrentHealth / playerMaxHealth;
        }

        if (towerHealthFillImage != null)
        {
            towerHealthFillImage.fillAmount = towerCurrentHealth / towerMaxHealth;
        }
    }
}
