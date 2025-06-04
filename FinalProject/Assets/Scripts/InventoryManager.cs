using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    GameObject[] inv = { null, null, null, null, null };
    TextMeshProUGUI costText;

    void Start()
    {
        costText = GameObject.Find("P" + gameObject.GetComponent<PlayerController>().playerNumber + "CostText").GetComponent<TextMeshProUGUI>();
        costText.gameObject.SetActive(false);
    }

    public bool AddItem(int playerNumber, GameObject item)
    {
        if (item == null)
        {
            return false;
        }

        for (int i = 0; i < inv.Length; i++)
        {
            if (inv[i] == null) // find first empty inv slot
            {
                inv[i] = item;
                SpriteRenderer itemSprite = item.GetComponent<SpriteRenderer>();
                GameObject invIcon = GameObject.Find("P" + playerNumber + "Icon" + i);
                invIcon.GetComponent<Image>().sprite = itemSprite.sprite; // change inventory icon image
                invIcon.GetComponent<Image>().color = itemSprite.color;

                return true; // true for success
            }
        }

        return false; // return false if inv full
    }

    public void SelectItem(int playerNumber, int index)
    {

        if (inv[index] != null)
        {
            Placeable placeable = inv[index].GetComponent<Placeable>();

            if (placeable != null) // is this prefab a Placeable?
            {
                DeselectItem(playerNumber);
                GameObject invBG = GameObject.Find("P" + playerNumber + "ItemBG" + index);
                invBG.GetComponent<Image>().color = Color.yellow;
                gameObject.GetComponent<PlayerController>().placeable = placeable; // change equipped Placeable
                costText.gameObject.SetActive(true);
                costText.text = "Cost: " + placeable.goldCost;
            }
            else
            {
                Debug.LogError("Not a placeable");
            }
        }
        else
        {
            DeselectItem(playerNumber);
            Debug.Log("Empty slot");
        }
    }

    private void DeselectItem(int playerNumber)
    {
        for (int i = 0; i < inv.Length; i++) // turn other BGs white so only one is yellow at a time
        {
            GameObject otherInvBG = GameObject.Find("P" + playerNumber + "ItemBG" + i);
            Color newColor;

            if (ColorUtility.TryParseHtmlString("#D1D1D1", out newColor))
            {
                otherInvBG.GetComponent<Image>().color = newColor;
            }
        }

        gameObject.GetComponent<PlayerController>().placeable = null; 
    }
}
