using UnityEngine;
using UnityEngine.UI;

public class HealthbarController : MonoBehaviour
{
    [Range(0, 100)]
    public float playerMaxHealth = 100f;
    public float playerCurrentHealth;
    public int playerNumber = 1;
    private Image playerHealthFillImage;
    
    private void Start()
    {
        playerHealthFillImage = GameObject.Find("P" + playerNumber + "HPBarFill").GetComponent<Image>();
        playerCurrentHealth = playerMaxHealth;
        UpdateHealthUI();
    }

    public void TakePlayerDamage(float amount)
    {
        playerCurrentHealth = Mathf.Max(playerCurrentHealth - amount, 0);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (playerHealthFillImage != null)
        {
            playerHealthFillImage.fillAmount = playerCurrentHealth / playerMaxHealth;
        }
    }
}
