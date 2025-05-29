// attach to LevelManager
using UnityEngine;

public class Barricade : Placeable
{
    public Vector2 direction;
    public float health = 10;
    public GameObject barricadePrefabTop;
    public GameObject barricadePrefabSide;
    public int goldCost = 25;

    public override void Place(int selectionIndex, int playerNumber, Transform playerTransform, Vector2 lastMoveDirection) // usesless selectionIndex for now
    {
        PlayerController player = GameObject.Find("Player" + playerNumber + "(Clone)").GetComponent<PlayerController>();
        if (player.DeductGold(goldCost))
        {
            // get Barricade component
            Barricade playerBarricade = barricadePrefabSide.GetComponent<Barricade>();

            // place barricade toward the last move direction
            playerBarricade.direction = lastMoveDirection;

            // spawn an appropriate barricade model at location of player
            if (lastMoveDirection == Vector2.right || lastMoveDirection == Vector2.left)
            {
                Instantiate(barricadePrefabTop, playerTransform.position, barricadePrefabSide.transform.rotation);
            }
            else if (lastMoveDirection == Vector2.up || lastMoveDirection == Vector2.down)
            {
                Instantiate(barricadePrefabSide, playerTransform.position, barricadePrefabSide.transform.rotation);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
