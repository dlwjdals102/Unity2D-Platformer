using UnityEngine;

public class EnemyHurtState : EnemyState
{
    private float knockbackForceX = 3f;
    private float knockbackForceY = 4f;

    public EnemyHurtState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 피격 시 플레이어가 바라보는 반대 방향으로 넉백!
        enemy.Movement.SetVelocity(enemy.KnockbackDirection * knockbackForceX, knockbackForceY);
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
}