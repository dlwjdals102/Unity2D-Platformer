using UnityEngine;

public class PlayerAirborneState : PlayerState
{
    public PlayerAirborneState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }


    public override void Update()
    {
        base.Update();

        // 공중 대시 처리
        if (player.HasDashBuffer && player.CanDash)
        {
            player.UseDashBuffer();
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // 코요테 타임 점프 처리
        if (player.HasJumpInputBuffer && player.CanCoyoteJump)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        // 착지 로직 (이때 공격 버퍼가 살아있다면, GroundedState로 넘어가자마자 즉시 공격이 나갑니다!)
        if (player.Movement.IsGrounded() && player.CurrentVelocityY < 0.1f)
        {
            if (Mathf.Abs(player.MoveInput.x) > 0.01f)
                stateMachine.ChangeState(player.MoveState);
            else
                stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        player.Movement.SetVelocity(player.MoveInput.x * player.Data.moveSpeed, player.Movement.RB.linearVelocity.y);
    }
}
