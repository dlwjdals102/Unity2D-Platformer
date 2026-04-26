using UnityEngine;

/// <summary>
/// 근접 공격형 적 데이터.
/// 새 근접 적을 추가할 때는 이 SO만 만들면 됩니다 (Enemy.cs 수정 불필요).
/// </summary>
[CreateAssetMenu(fileName = "newMeleeEnemyData", menuName = "Data/Enemy/Melee Enemy Data")]
public class MeleeEnemyData : EnemyData
{
    /// <summary>근접 공격 상태를 생성하여 반환합니다.</summary>
    public override EnemyAttackStateBase CreateAttackState(Enemy owner, StateMachine<Enemy> stateMachine)
    {
        return new EnemyMeleeAttackState(owner, stateMachine, "Attack");
    }
}
