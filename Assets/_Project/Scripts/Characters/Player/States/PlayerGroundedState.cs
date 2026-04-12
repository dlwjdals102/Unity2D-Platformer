using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        // 대시 선입력 처리 (최우선 순위: 회피)
        if (player.HasDashBuffer && player.CanDash)
        {
            player.UseDashBuffer(); // 버퍼를 소모!
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // 공격 선입력 처리
        if (player.HasAttackBuffer)
        {
            player.UseAttackBuffer(); // 버퍼를 소모!

            // 시간이 지났거나(콤보 끊김) 카운터가 3 이상이면 무조건 1타로 초기화
            if (Time.time >= player.lastAttackTime + player.comboWindow || player.comboCounter >= 3)
            {
                player.comboCounter = 1;
            }
            else
            {
                player.comboCounter++;
            }

            stateMachine.ChangeState(player.AttackState);
            return;
        }

        // 점프 선입력 처리
        if (player.HasJumpInputBuffer && player.Movement.IsGrounded())
        {
            // 점프 버퍼 소모는 PlayerJumpState.Enter() 내부에서 알아서 처리됩니다.
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        // 공통 로직 2: 갑자기 발밑에 땅이 없어지면 낙하 상태로 전환
        if (!player.Movement.IsGrounded())
        {
            stateMachine.ChangeState(player.FallState);
        }
    }
}
