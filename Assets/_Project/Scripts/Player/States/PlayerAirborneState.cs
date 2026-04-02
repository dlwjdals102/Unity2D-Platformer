using UnityEngine;

public class PlayerAirborneState : State<PlayerController>
{
    public PlayerAirborneState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }


    public override void Update()
    {
        base.Update();

        // 대시 버튼을 눌렀고, 쿨타임이 돌지 않았다면 즉시 대시 상태로 전환!
        if (entity.DashInput && entity.CanDash)
        {
            stateMachine.ChangeState(entity.DashState);
            return; // 대시로 상태가 바뀌었으니 아래 로직(점프 등)은 무시
        }

        // 코요테 타임 적용: 
        // 선입력(JumpBuffer)이 있고 + 아직 코요테 타이머가 남아있다면 점프 상태로 전환!
        if (entity.HasJumpInputBuffer && entity.CanCoyoteJump)
        {
            stateMachine.ChangeState(entity.JumpState);
        }

        // 착지 로직
        if (entity.IsGrounded && entity.CurrentVelocityY < 0.1f)
        {
            if (Mathf.Abs(entity.MoveInput.x) > 0.01f)
                stateMachine.ChangeState(entity.MoveState);
            else
                stateMachine.ChangeState(entity.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 공중에서도 방향을 전환하고 좌우로 움직일 수 있도록 공통 처리 (Air Control)
        entity.CheckDirectionToFace(entity.MoveInput.x);

        // moveSpeed는 추후 PlayerController의 스탯으로 빼는 것이 좋습니다.
        float moveSpeed = 8f;
        entity.SetVelocityX(entity.MoveInput.x * moveSpeed);
    }
}
