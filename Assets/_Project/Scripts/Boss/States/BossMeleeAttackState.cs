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
        bool hitSomething = boss.Combat.PerformCustomMeleeAttack(
            damage: boss.NextAttack.attackDamage,
            offset: boss.NextAttack.hitOffset,
            customRadius: boss.NextAttack.hitRadius,
            facingDirection: boss.Movement.FacingDirection
        );

        if (hitSomething)
        {
            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.TriggerHitStop(0.07f);
                FeedbackManager.Instance.TriggerCameraShake(1f);
            }

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