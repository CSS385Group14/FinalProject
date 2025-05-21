//OBSOLETE

// using UnityEngine;

// public class DefenseManager : Placeable
// {
//     public GameObject[] defensePrefabs;

//     public override void Place(int selectionIndex, int playerNumber, Transform playerTransform, Vector2 lastMoveDirection)
//     {
//         if (defensePrefabs[selectionIndex] == null) return;

//         // try to get the DefenseTower component from the prefab
//         DefenseTower defense = defensePrefabs[selectionIndex].GetComponent<DefenseTower>();
//         if (defense == null)
//         {
//             Debug.LogWarning("Prefab does not have a DefenseTower component.");
//             return;
//         }

//         // do build check here

//         // instantiate this defense at the player position
//         GameObject instance = Instantiate(defensePrefabs[selectionIndex], playerTransform.position, defensePrefabs[selectionIndex].transform.rotation);
//         //GameObject instance = Instantiate(prefab, playerTransform.position, defensePrefabs[selectionIndex].transform.rotation);
//         DefenseTower def = instance.GetComponent<DefenseTower>();

//         // pass the player number so that gold and xp can be routed toward that player on enemy killed
//         def.SetOwner(playerNumber);

//         // deduct gold cost from player who placed this defense
//         PlayerController player = GameObject.Find("Player" + playerNumber + "(Clone)").GetComponent<PlayerController>();
//         if (!player.DeductGold(def.goldCost)) // if player does not have enough gold, undo instantiation
//         {
//             Destroy(instance);
//         }
//     }
// }
