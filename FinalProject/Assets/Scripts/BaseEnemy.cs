using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public EnemyStats statsSO;  // ScriptableObject for stats

    protected int currentHP;
    protected Rigidbody2D rb;
    private GameObject player1;
    private GameObject player2;
    private GameObject tower;
    private GameObject currentTarget;
    private GameObject defense;
    protected virtual void Start()
    {
        if (statsSO == null)
        {
            Debug.LogError($"{name}: EnemyStats ScriptableObject not assigned.");
            return;
        }

        currentHP = statsSO.maxHealth;

    }

    protected virtual void Update()
    {
 
    }


    public virtual void TakeDamage(int damage, int ownerID)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void SetTargets(GameObject p1, GameObject p2, GameObject tower, GameObject defense)
    {
        player1 = p1;
        player2 = p2;
        this.tower = tower;
        this.defense = defense;
    }
    protected void SetTargetToPlayer1() => currentTarget = player1;
    protected void SetTargetToPlayer2() => currentTarget = player2;
    protected void SetTargetToTower() => currentTarget = tower;
    protected void SetTargetToDefense() => currentTarget = defense;
}
