using UnityEngine;

public abstract class Placeable : MonoBehaviour
{
    public GameObject prefab;
    public abstract void Place(int selectionIndex, int playerNumber, Transform playerTransform, Vector2 lastMoveDirection);
}
