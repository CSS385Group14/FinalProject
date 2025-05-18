// attach to GameManager
using UnityEngine.UI;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    GameObject[] p1Inv = { null, null, null, null, null };
    GameObject[] p2Inv = { null, null, null, null, null };

    public bool AddItem(int playerNumber, GameObject item) 
    {
        if (item == null)
        {
            return false;
        }

        GameObject[] inv = p1Inv; // defaults to player 1 inv

        if (playerNumber == 2)
        {
            inv = p2Inv;
        }
      
        for (int i = 0; i < p1Inv.Length; i++)
        {
            if (inv[i] == null) // find first empty inv slot
            {
                inv[i] = item;
                Image img = item.GetComponent<Image>();
                GameObject invIcon = GameObject.Find("P" + playerNumber + "Icon" + i);
                Image iconImage = invIcon.GetComponent<Image>();

                if (img != null && iconImage != null) // image assignment doesn't work
                {
                    iconImage.sprite = img.sprite;     
                    iconImage.color = img.color;              
                    iconImage.preserveAspect = img.preserveAspect;
                }

                return true; // true for success
            }
        }

        return false; // return false if inv full
    }

    public void SelectItem(int playerNumber, int index)
    {
        GameObject[] inv = p1Inv; // defaults to player 1 inv

        if (playerNumber == 2)
        {
            inv = p2Inv;
        }

        if (inv[index] != null)
        {
            GameObject.Find("Player" + playerNumber + "(Clone)").GetComponent<PlayerController>().placeable.prefab = inv[index];
            // Placeable placeable = inv[index].GetComponent<Placeable>(); // check if item is a Placeable
            // if (placeable != null)
            // {
            //     gameObject.GetComponent<PlayerController>().placeable = placeable; // change equiped Placeable
                 Debug.Log("selected");
            // }
            // else
            // {
            //     Debug.Log("null placeable");
            // }
        }
    }
}
