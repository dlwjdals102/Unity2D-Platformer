using UnityEngine;

public class PlayerAttackState : State
{
    public PlayerAttackState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        player.UseAttackInput();

        // 지상 공격이므로 미끄러지지 않도록 X축 속도를 즉시 0으로 만듭니다.
        player.SetVelocityX(0f);
    }

    public override void Update()
    {
        base.Update();

        // 타이머가 아닌, 애니메이션의 종료 신호를 기다립니다!
        if (isAnimationFinished)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 공격 중에도 중력은 적용받아야 하므로 Y축 제어는 하지 않습니다.
    }
}
