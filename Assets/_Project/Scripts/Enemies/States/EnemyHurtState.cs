using UnityEngine;

public class EnemyHurtState : EnemyState
{
    public EnemyHurtState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 피격 상태에 돌입하면 자동 뒤집기 기능을 잠급니다. (때린 놈을 계속 노려보게 됨)
        enemy.Movement.CanAutoFlip = false;

        if (enemy.IsLedgeDetected() && enemy.IsLedgeBehindDetected())
        // 피격 시 플레이어가 바라보는 반대 방향으로 넉백!
            enemy.Movement.SetVelocity(enemy.KnockbackDirection * enemy.knockbackForceX, enemy.knockbackForceY);
    }

    public override void Update()
    {
        base.Update();

        // 공중에 떠 있는 동안에는 아무것도 못 하고 굳은 채로 떨어지게 됩니다.
        if (isAnimationFinished && enemy.Movement.IsGrounded())
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        // 피격 상태가 끝나고 다시 순찰/추적을 시작할 때는 기능을 켜줍니다!
        enemy.Movement.CanAutoFlip = true;
    }
}