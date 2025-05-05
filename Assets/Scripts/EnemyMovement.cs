using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = LevelManager.main.path[pathIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;
            if (pathIndex == LevelManager.main.path.Length)
            {
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //Is this a barricade that touched me?
        Barricade barricade = collision.GetComponent<Barricade>();
        if (barricade != null)
        {
            Debug.Log("Barricade reached");
            //stopped = true;
        }

        //Is this a bullet?
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            Debug.Log("Hit by bullet");
        }
    }
}
