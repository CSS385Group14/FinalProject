using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    // store all enemies that are in range of player
    private List<Transform> enemiesInRange = new List<Transform>();

<<<<<<< HEAD
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy Level 1"))
        {
            enemiesInRange.Add(other.transform);
        }
        if (other.CompareTag("Enemy Level 2"))
        {
            enemiesInRange.Add(other.transform);
        }
        if (other.CompareTag("Enemy Level 3"))
        {
            enemiesInRange.Add(other.transform);
        }
        if (other.CompareTag("Enemy Level 4"))
        {
            enemiesInRange.Add(other.transform);
        }
        if (other.CompareTag("Enemy Level 5"))
=======
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
>>>>>>> 1bb0681f6ccf692cfc86d9bcee119cb9526059c3
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
<<<<<<< HEAD
        if (other.CompareTag("Enemy Level 1"))
        {
            enemiesInRange.Remove(other.transform);
        }
        if (other.CompareTag("Enemy Level 2"))
        {
            enemiesInRange.Remove(other.transform);
        }
        if (other.CompareTag("Enemy Level 3"))
        {
            enemiesInRange.Remove(other.transform);
        }
        if (other.CompareTag("Enemy Level 4"))
        {
            enemiesInRange.Remove(other.transform);
        }
        if (other.CompareTag("Enemy Level 5"))
=======
        if (other.CompareTag("Enemy"))
>>>>>>> 1bb0681f6ccf692cfc86d9bcee119cb9526059c3
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
