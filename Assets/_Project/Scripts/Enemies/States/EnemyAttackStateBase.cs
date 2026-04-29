using UnityEngine;

/// <summary>
/// 모든 적 공격 상태의 추상 베이스.
/// 근접/원거리/마법/자폭 등 모든 공격 행동은 이 클래스를 상속받습니다.
///
/// 이 추상 레이어 덕분에:
/// 1. Enemy.AttackState가 다형성으로 어떤 공격 행동이든 들어올 수 있고,
/// 2. EnemyData.CreateAttackState()가 자신에게 맞는 공격을 직접 생성할 수 있습니다.
/// </summary>
public abstract class EnemyAttackStateBase : EnemyState
{
    protected EnemyAttackStateBase(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        // 공격 시작 시 쿨타임 시작점 기록
        enemy.lastAttackTime = Time.time;

        // 공격 시작 시 플레이어 쪽으로 회전
        enemy.TurnTowards(enemy.PlayerTransform);
    }
}
