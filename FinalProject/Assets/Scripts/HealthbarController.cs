using UnityEngine;
using UnityEngine.UI;

public class HealthbarController : MonoBehaviour
{
    [Range(0, 100)]
    public int playerMaxHealth = 100;
    public float playerCurrentHealth;
    private int playerNumber;
    private Image playerHealthFillImage;
    
    private void Start()
    {
        playerNumber = transform.GetComponent<PlayerController>().playerNumber;
        playerHealthFillImage = GameObject.Find("P" + playerNumber + "HPBarFill").GetComponent<Image>();
        playerCurrentHealth = playerMaxHealth;
        UpdateHealthUI();
    }

    public void HealPlayer(int amount)
    {
        playerCurrentHealth = Mathf.Min(playerCurrentHealth + amount, playerMaxHealth);
        UpdateHealthUI();
    }

    public void TakePlayerDamage(int amount)
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
