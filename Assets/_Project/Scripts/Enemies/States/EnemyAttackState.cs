using UnityEngine;

public class EnemyAttackState : EnemyState
{
    public EnemyAttackState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 공격할 때는 미끄러지지 않도록 X축 속도를 강제로 0으로 멈춥니다.
        enemy.SetVelocity(0f, enemy.RB.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 애니메이션이 완전히 끝날 때까지 대기합니다.
        if (isAnimationFinished)
        {
            // 공격이 끝난 시간을 기록합니다.
            enemy.lastAttackTime = Time.time;

            // 무조건 자기 자신(AttackState)이 아닌 다른 상태로 빠져나갑니다!
            if (enemy.IsPlayerInSight())
            {
                stateMachine.ChangeState(enemy.ChaseState);
            }
            else
            {
                stateMachine.ChangeState(enemy.PatrolState);
            }
        }
    }
}