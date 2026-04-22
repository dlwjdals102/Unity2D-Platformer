using UnityEngine;

public class BossShockwaveState : BossState
{
    public BossShockwaveState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
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
        // 오직 투사체 발사만 처리!
        boss.Combat.FireCustomProjectile(
            poolTag: boss.NextAttack.projectileTag,
            offset: boss.NextAttack.hitOffset,
            facingDirection: boss.Movement.FacingDirection,
            damage: boss.NextAttack.attackDamage
        );
    }
}