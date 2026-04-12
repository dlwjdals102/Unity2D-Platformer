using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private bool attackRegistered; // 콤보 공격 예약
    private bool dashRegistered;   // 긴급 회피(대시) 예약

    public PlayerAttackState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        // 진입 시 예약 플래그 초기화
        attackRegistered = false;
        dashRegistered = false;

        // 지상 공격이므로 미끄러지지 않도록 X축 속도를 즉시 0으로 만듭니다.
        player.Movement.SetVelocity(0f, player.Movement.RB.linearVelocity.y);

        // 현재 몇 단 공격인지 애니메이터에게 숫자로 알려줍니다!
        player.Anim.SetInteger("ComboCounter", player.comboCounter);
    }

    public override void Update()
    {
        base.Update();

        // 입력 예약 시스템 (애니메이션 재생 중 입력 감지)
        if (player.HasDashBuffer && !dashRegistered)
        {
            dashRegistered = true;
            player.UseDashBuffer(); // 찜해뒀으니 버퍼는 소모
        }
        else if (player.HasAttackBuffer && !attackRegistered)
        {
            attackRegistered = true;
            player.UseAttackBuffer(); // 찜해뒀으니 버퍼는 소모
        }

        // 애니메이션의 마지막 프레임에 도달했을 때 행동 결산!
        if (isAnimationFinished)
        {
            // 결산 1순위: 생존을 위한 긴급 대시! (공격보다 회피가 우선되어야 덜 억울합니다)
            if (dashRegistered && player.CanDash)
            {
                stateMachine.ChangeState(player.DashState);
                return;
            }

            // 결산 2순위: 콤보 공격 이어나가기
            if (attackRegistered && player.comboCounter < 3)
            {
                player.comboCounter++;
                stateMachine.ChangeState(player.AttackState); // 자기 자신으로 다시 진입
                return;
            }

            // 예약된 행동이 없다면 대기 상태로 복귀
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 공격 상태를 완전히 빠져나갈 때 현재 시간을 기록하여, 
        // 콤보 유예 시간(comboWindow) 계산에 사용합니다.
        player.lastAttackTime = Time.time;
    }
}
