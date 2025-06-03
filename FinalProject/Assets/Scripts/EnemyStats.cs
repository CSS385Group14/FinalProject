using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Stats")]
public class EnemyStats : ScriptableObject
{
    public int maxHealth;
    public int damageAmount;
    public float attackRate;
    public int xpValue;
    public int goldValue;
    public float detectionRangePlayer;
    public float detectionRangeTower;
    public float moveSpeed;
    public float attackRange;
}
