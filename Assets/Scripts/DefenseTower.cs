using System.Collections.Generic;
using UnityEngine;

public class DefenseTower : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float projectileSpeed = 10f;
    public int projectileDamage = 20;
    public int goldCost = 100;
    private int playerNumber;
    private float fireCooldown;
    private List<Transform> enemiesInRange = new List<Transform>();

    void Start()
    {

    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (enemiesInRange.Count > 0 && fireCooldown <= 0f)
        {
            Transform target = enemiesInRange[0]; // grab the first enemy
            FireAt(target);
            fireCooldown = 1f / fireRate;
        }
    }

    void FireAt(Transform enemy)
    {
        GameObject instance = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // set parameters
        DefenseProjectile proj = instance.GetComponent<DefenseProjectile>();
        proj.SetTarget(enemy); // target enemy's location
        proj.SetSpeed(projectileSpeed);
        proj.SetDamage(projectileDamage);
        proj.SetOwner(playerNumber);
    }

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

    public void SetOwner(int playerNumber)
    {
        this.playerNumber = playerNumber;
    }
}
