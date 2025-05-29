using System;
using UnityEngine;
using UnityEngine.UI;

public class XPBarController : MonoBehaviour
{
    public int playerNumber = 1;
    public int playerTotalXP = 0;
    public int playerCurrentLevelXP = 0;
    public int lastXPThreshold;
    public int nextXPThreshold;
    private int playerLevel = 1;
    private int[] levelThresholds = new int[ProgressionManager.MAX_LEVEL]; // store all level thresholds
    private Image playerXPFillImage;
    private ProgressionManager progressionManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerXPFillImage = GameObject.Find("P" + playerNumber + "XPBarFill").GetComponent<Image>();
        progressionManager = GameObject.Find("ProgressionManager").GetComponent<ProgressionManager>();
        SetLevelThresholds();
        lastXPThreshold = 0;
        nextXPThreshold = levelThresholds[0]; // 20
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetXPBar()
    {
        // when player levels up, set last and next thresholds
        // and set xp bar to correct value
        lastXPThreshold = levelThresholds[playerLevel - 1]; // 20

        if (playerLevel != ProgressionManager.MAX_LEVEL)
            nextXPThreshold = levelThresholds[playerLevel]; // 40

        playerLevel++; // 2

        playerCurrentLevelXP = playerTotalXP;
        playerCurrentLevelXP -= lastXPThreshold; // 20 - 20 = 0
        UpdateXPBar();
    }

    public void FillXPBar(int xpAmount)
    {
        playerTotalXP += xpAmount;
        playerCurrentLevelXP = Math.Min(playerCurrentLevelXP + xpAmount, nextXPThreshold - lastXPThreshold);
        UpdateXPBar();
    }

    void UpdateXPBar()
    {
        if (playerXPFillImage != null)
        {
            playerXPFillImage.fillAmount = playerCurrentLevelXP / (float)(nextXPThreshold - lastXPThreshold); // 0 / 20
        }
    }

    private void SetLevelThresholds()
    {
        levelThresholds[0] = progressionManager.startingLevelThreshold;
        for (int i = 1; i < ProgressionManager.MAX_LEVEL; i++)
        {
            levelThresholds[i] = levelThresholds[i - 1] * 2;
        }
    }
}
