using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDefenseDetector : MonoBehaviour
{
    public int uiYOffset = -2;
    private PlayerController playerController;
    private Camera mainCamera;
    private RectTransform upgradeButtonUI;
    private Button upgradeButton;
    private List<DefenseTower> defensesInRange = new List<DefenseTower>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = transform.GetComponent<PlayerController>();
        mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
        upgradeButtonUI = GameObject.Find("UpgradeButtonP" + playerController.playerNumber).GetComponent<RectTransform>();
        upgradeButton = GameObject.Find("UpgradeButtonP" + playerController.playerNumber).GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (defensesInRange.Count > 0) // when any defense is in range of the player
        {
            // show upgrade button (aka, move the button to the target location)
            DefenseTower defenseTower = GetClosestDefense();

            // do not show the button if already max
            if (defenseTower.level == DefenseTower.MAX_LEVEL)
            {
                // move button to hide position
                upgradeButtonUI.position = new Vector2(-40, 80);
                return;
            }

            // move the upgrade button
            Vector3 nearestDefensePosition = defenseTower.transform.position;
            MoveUpgradeButton(nearestDefensePosition);

            // add functionality to the button
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => defenseTower.Upgrade());
        }
        else // when no defense is in range, hide the button
        {
            upgradeButtonUI.position = new Vector2(-40, 80);
        }
    }

    void MoveUpgradeButton(Vector3 worldPos)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(new Vector2(worldPos.x, worldPos.y + uiYOffset));
        upgradeButtonUI.position = screenPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Defense"))
        {
            Debug.Log("Defense entered trigger.");
            defensesInRange.Add(other.GetComponent<DefenseTower>());
            upgradeButton.transform.SetAsLastSibling();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Defense"))
        {
            Debug.Log("Defense exited trigger.");
            defensesInRange.Remove(other.GetComponent<DefenseTower>());
        }
    }

    public DefenseTower GetClosestDefense()
    {
        if (defensesInRange == null || defensesInRange.Count == 0)
            return null;

        DefenseTower closestDefense = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (DefenseTower defense in defensesInRange) // for each defense in range, measure distance
        {
            if (defense == null) continue; // skip null entries

            float distanceSqr = (defense.transform.position - currentPosition).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestDefense = defense;
            }
        }

        return closestDefense;
    }
}