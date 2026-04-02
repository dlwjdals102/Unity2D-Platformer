using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(PlayerController player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 대기 상태 진입 시 X축 속도만 0으로 깔끔하게 초기화
        player.RB.linearVelocityX = 0f;
    }

    public override void Update()
    {
        base.Update();

        if (Mathf.Abs(player.MoveInput.x) > 0.01f)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }



}
