using UnityEngine;

public class EnemyRangedAttackState : EnemyState
{
    public EnemyRangedAttackState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName) : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 공격할 때는 미끄러지지 않도록 X축 속도를 강제로 0으로 멈춥니다.
        enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 애니메이션이 완전히 끝날 때까지 대기합니다.
        if (isAnimationFinished)
        {
            // 핑퐁(추격->도망)을 방지하기 위해 다이렉트로 RetreatState로 넘깁니다!
            if (enemy.IsPlayerInRetreatRange())
            {
                stateMachine.ChangeState(enemy.RetreatState);
                return;
            }

            stateMachine.ChangeState(enemy.ChaseState);
        }
    }

    // 애니메이션 타격 프레임 이벤트가 발생하면 이 곳이 실행됩니다!
    public override void TriggerAttack()
    {
        // 1. 발사 방향 계산: 몬스터가 바라보는 방향(FacingDirection)으로 수평(X축) 궤적 생성
        Vector2 fireDirection = new Vector2(enemy.Movement.FacingDirection, 0f);

        // 2. 투사체 발사! (풀 태그, 방향, 데미지 전달)
        enemy.Combat.FireProjectile(
            enemy.Data.projectilePoolTag,
            fireDirection,
            enemy.Data.attackDamage
        );
    }
}
