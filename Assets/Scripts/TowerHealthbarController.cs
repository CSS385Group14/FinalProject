using UnityEngine;
using UnityEngine.UI;

public class TowerHealthbarController : MonoBehaviour
{
    [Range(0, 100)]
    public float towerMaxHealth = 50f;
    public float towerCurrentHealth;
    private Image towerHealthFillImage;
    
    private void Start()
    {
        towerHealthFillImage = GameObject.Find("TowerHPBarFill").GetComponent<Image>();
        towerCurrentHealth = towerMaxHealth;
        UpdateHealthUI();
    }

    public void TakeTowerDamage(float amount)
    {
        towerCurrentHealth = Mathf.Max(towerCurrentHealth - amount, 0);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (towerHealthFillImage != null)
        {
            towerHealthFillImage.fillAmount = towerCurrentHealth / towerMaxHealth;
        }
    }
}
