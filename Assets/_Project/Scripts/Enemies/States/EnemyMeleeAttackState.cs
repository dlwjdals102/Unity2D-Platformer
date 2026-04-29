using UnityEngine;

public class EnemyMeleeAttackState : EnemyAttackStateBase
{
    public EnemyMeleeAttackState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (isAnimationFinished)
        {
            stateMachine.ChangeState(enemy.ChaseState); // 시야에 있으면 즉시 추격 (연속 공격)
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 타격 중 넉백을 받아도 미끄러지지 않도록 꽉 잡아줍니다.
        enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
    }

    // 애니메이션 타격 프레임 이벤트가 발생하면 이 곳이 실행됩니다!
    public override void TriggerAttack()
    {
        bool hitSomething = enemy.Combat.PerformMeleeAttack(enemy.Data.attackDamage, enemy.Movement.FacingDirection, "HitSpark");

        // (이펙트나 사운드를 추가하고 싶다면 여기서 바로 처리하면 됩니다)
        if (hitSomething)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("SFX_Hit");
            }
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("SFX_Miss");
            }
        }
    }
}