using UnityEngine;

public class PlayerJumpState : PlayerAirborneState
{
    public PlayerJumpState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(player.RB.linearVelocity.x, player.Data.jumpForce);

        // 점프를 실행했으므로 선입력 타이머를 즉시 초기화
        player.UseJumpBuffer();
        player.UseCoyoteTime();
    }

    public override void Update()
    {
        base.Update(); // 부모의 착지 감지 로직 실행

        // 가변 점프 (Jump Cut) 핵심 로직
        // 캐릭터가 위로 상승 중(Y속도 > 0)일 때, 점프 버튼에서 손을 뗐다면(!IsJumpButtonHeld)
        if (player.CurrentVelocityY > 0 && !player.IsJumpButtonHeld)
        {
            // 상승 속도를 확 줄여서(예: 50%) 더 이상 높이 못 올라가게 만듭니다.
            player.SetVelocity(player.RB.linearVelocity.x, player.CurrentVelocityY * player.jumpCutMultiplier);
        }

        // 정점을 찍고 떨어지기 시작하면 Fall 상태로 전환
        if (player.CurrentVelocityY < 0f)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }
}
