using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    // store all enemies that are in range of player
    private List<Transform> enemiesInRange = new List<Transform>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other.transform);
        }
    }

    public Transform GetClosestEnemyTransformToAttack()
    {
        if (enemiesInRange == null || enemiesInRange.Count == 0)
            return null;

        Transform closestEnemy = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.parent.position; // assuming script is child of PlayerController

        foreach (Transform enemy in enemiesInRange) // for each enemy in range, measure distance
        {
            if (enemy == null) continue; // skip null entries

            float distanceSqr = (enemy.position - currentPosition).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
}
