using UnityEngine;

public class PlayerGroundedState : State
{
    public PlayerGroundedState(PlayerController player, StateMachine stateMachine, string animBoolName)
            : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        // 대시 버튼을 눌렀고, 쿨타임이 돌지 않았다면 즉시 대시 상태로 전환!
        if (player.DashInput && player.CanDash)
        {
            stateMachine.ChangeState(player.DashState);
            return; // 대시로 상태가 바뀌었으니 아래 로직(점프 등)은 무시
        }

        if (player.AttackInput)
        {
            stateMachine.ChangeState(player.AttackState);
            return;
        }

        // 공통 로직 1: 땅에 있을 때 점프 버튼을 누르면 즉시 점프 상태로 전환
        if (player.HasJumpInputBuffer && player.IsGrounded)
        {
            stateMachine.ChangeState(player.JumpState);
        }

        // 공통 로직 2: 갑자기 발밑에 땅이 없어지면 낙하 상태로 전환
        if (!player.IsGrounded)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 땅에 있을 때는 항상 방향 전환이 가능하도록 공통 처리
        player.CheckDirectionToFace(player.MoveInput.x);
    }
}
