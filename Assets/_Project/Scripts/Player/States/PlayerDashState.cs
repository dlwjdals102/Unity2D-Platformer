using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float dashTimer;
    private float dashDirection;

    public PlayerDashState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        player.UseDash(); // 대시 입력 소비 및 쿨타임 초기화
        dashTimer = entity.dashDuration; // 타이머 설정

        // 1. 방향 결정: 키보드를 누르고 있으면 그 방향, 안 누르고 있으면 현재 바라보는 방향
        if (player.MoveInput.x != 0)
            dashDirection = Mathf.Sign(player.MoveInput.x);
        else
            dashDirection = player.FacingDirection;

        // 2. 대시 중에는 중력을 0으로 만들어 일직선으로 날아가게 합니다.
        player.SetGravityScale(0f);
    }

    public override void Update()
    {
        base.Update();

        // 타이머 차감
        dashTimer -= Time.deltaTime;

        // 대시 시간이 끝나면 상태 전환
        if (dashTimer <= 0)
        {
            if (player.IsGrounded())
                stateMachine.ChangeState(player.IdleState);
            else
                stateMachine.ChangeState(player.FallState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // Y축 속도는 0으로 고정, X축으로만 고속 이동
        player.SetVelocity(dashDirection * player.dashSpeed, 0f);
    }

    public override void Exit()
    {
        base.Exit();

        // 3. 상태를 빠져나갈 때 반드시 중력을 원래대로 복구!
        player.SetGravityScale(player.DefaultGravity);

        // 대시가 끝났을 때 관성으로 미끄러지는 것을 방지하기 위해 X축 속도 초기화
        player.SetVelocity(0f, player.RB.linearVelocity.y);
    }


}
