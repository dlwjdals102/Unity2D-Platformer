using UnityEngine;

/// <summary>
/// 원거리 공격형 적 데이터.
/// 투사체 발사 정보와 도망(retreat) 동작에 필요한 추가 필드를 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "newRangedEnemyData", menuName = "Data/Enemy/Ranged Enemy Data")]
public class RangedEnemyData : EnemyData
{
    [Header("Ranged Attack Settings")]
    [Tooltip("ObjectPool에 등록된 투사체 태그 (예: \"Arrow\")")]
    public string projectilePoolTag = "Arrow";

    [Tooltip("이 거리 안으로 들어오면 도망칩니다.")]
    public float retreatDistance = 3f;

    [Tooltip("도망치는 속도 (보통 chaseSpeed보다 살짝 느리거나 같게).")]
    public float retreatSpeed = 3f;

    /// <summary>원거리 공격 상태를 생성하여 반환합니다.</summary>
    public override EnemyAttackStateBase CreateAttackState(Enemy owner, StateMachine<Enemy> stateMachine)
    {
        return new EnemyRangedAttackState(owner, stateMachine, "Attack");
    }
}
