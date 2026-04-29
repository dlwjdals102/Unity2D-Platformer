using UnityEngine;

public class PlayerIdleState : PlayerGroundedState
{
    public PlayerIdleState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        // 대기 상태 진입 시 X축 속도만 0으로 깔끔하게 초기화
        player.Movement.SetVelocity(0f, player.Movement.RB.linearVelocity.y);
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
