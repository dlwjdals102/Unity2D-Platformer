using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newEnemyData", menuName = "Data/Enemy Data")]
public class EnemyData : EntityData
{
    [Header("Enemy AI")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionDistance = 8f;
    public float attackDistance = 1.5f;

    // 시야에서 사라진 후 추적을 유지하는 시간
    public float aggroDuration = 2.0f;

    [Header("Enemy Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float specialAttackCooldown = 8f;

    [Header("Patrol Timers")]
    public float patrolDuration = 3f;     // 평소에 걷는 시간
    public float idleDuration = 2f;       // 평소에 가만히 쉬는 시간
    public float turnWaitDuration = 0.5f; // 절벽/벽을 만났을 때 멈칫! 하고 바라보는 시간

    [Header("Ranged Attack Settings")]
    public string projectilePoolTag = "Arrow"; // ObjectPool에 등록된 투사체 이름
    public float retreatDistance = 3f; // 이 거리 안으로 들어오면 도망침
    public float retreatSpeed = 3f;    // 도망치는 속도 (보통 chaseSpeed보다 살짝 느리거나 같게)
}