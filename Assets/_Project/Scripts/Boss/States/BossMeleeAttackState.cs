using UnityEngine;

public class BossMeleeAttackState : BossState
{
    public BossMeleeAttackState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        isAnimationFinished = false;
        boss.Movement.SetVelocity(0f, boss.Movement.RB.linearVelocity.y);
        boss.Anim.SetTrigger(boss.NextAttack.animTriggerName);
    }

    public override void Update()
    {
        base.Update();

        if (isAnimationFinished) stateMachine.ChangeState(boss.IdleState);
    }

    public override void TriggerAttack()
    {
        // 坷流 辟立 单固瘤 惯积父 贸府!
        boss.Combat.PerformCustomMeleeAttack(
            damage: boss.NextAttack.attackDamage,
            offset: boss.NextAttack.hitOffset,
            customRadius: boss.NextAttack.hitRadius,
            facingDirection: boss.Movement.FacingDirection
        );
    }
}