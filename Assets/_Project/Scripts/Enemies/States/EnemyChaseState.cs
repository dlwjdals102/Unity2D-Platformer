using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("침입자 발견! 추적 시작!");
        // (선택) 여기서 느낌표 파티클을 띄우거나, 괴성을 지르는 사운드를 재생하면 효과가 아주 좋습니다.
    }

    public override void Update()
    {
        base.Update();

        // 1. 공격 조건 체크 (사거리 안인가? AND 쿨타임이 다 지났는가?)
        if (enemy.IsPlayerInAttackRange())
        {
            if (Time.time >= enemy.lastAttackTime + enemy.Data.attackCooldown)
            {
                stateMachine.ChangeState(enemy.AttackState);
                return;
            }
        }

        // 2. 시야에서 놓쳤는가?
        if (!enemy.IsPlayerInSight())
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // [종료 조건 2] 맹목적으로 쫓아가다가 절벽에 떨어지거나 벽에 박으면 안 됩니다.
        // 길이 막혔다면 즉시 추적을 멈추고 순찰로 복귀합니다.
        if (!enemy.IsLedgeDetected() || enemy.Movement.IsWallDetected())
        {
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
            return;
        }

        // 사거리 안에는 있는데 쿨타임 중일 때의 처리
        if (enemy.IsPlayerInAttackRange())
        {
            // 플레이어에게 겹치지 않게 제자리에 서서 노려봅니다 (숨 고르기)
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
        }
        else
        {
            // 사거리 밖이라면 다시 쫓아갑니다.
            enemy.Movement.SetVelocity(enemy.Data.chaseSpeed * enemy.Movement.FacingDirection, enemy.Movement.RB.linearVelocity.y);
        }

    }
}