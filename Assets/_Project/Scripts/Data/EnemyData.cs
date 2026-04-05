using UnityEngine;

[CreateAssetMenu(fileName = "newEnemyData", menuName = "Data/Enemy Data")]
public class EnemyData : EntityData
{
    [Header("Enemy AI")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionDistance = 8f;
    public float attackDistance = 1.5f;

    [Header("Enemy Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
}