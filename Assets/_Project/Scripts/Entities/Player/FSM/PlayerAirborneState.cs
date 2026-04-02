using UnityEngine;

public class PlayerAirborneState : State
{
    public PlayerAirborneState(PlayerController player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Update()
    {
        base.Update();

        // 대시 버튼을 눌렀고, 쿨타임이 돌지 않았다면 즉시 대시 상태로 전환!
        if (player.DashInput && player.CanDash)
        {
            stateMachine.ChangeState(player.DashState);
            return; // 대시로 상태가 바뀌었으니 아래 로직(점프 등)은 무시
        }

        // 코요테 타임 적용: 
        // 선입력(JumpBuffer)이 있고 + 아직 코요테 타이머가 남아있다면 점프 상태로 전환!
        if (player.HasJumpInputBuffer && player.CanCoyoteJump)
        {
            stateMachine.ChangeState(player.JumpState);
        }

        // 착지 로직
        if (player.IsGrounded && player.CurrentVelocityY < 0.1f)
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

        // 공중에서도 방향을 전환하고 좌우로 움직일 수 있도록 공통 처리 (Air Control)
        player.CheckDirectionToFace(player.MoveInput.x);

        // moveSpeed는 추후 PlayerController의 스탯으로 빼는 것이 좋습니다.
        float moveSpeed = 8f;
        player.SetVelocityX(player.MoveInput.x * moveSpeed);
    }
}
