using UnityEngine;

public class PlayerHurtState : PlayerState
{
    private float knockbackForceX = 5f; // X축으로 밀려나는 힘
    private float knockbackForceY = 6f; // Y축으로 살짝 띄워지는 힘

    public PlayerHurtState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        // 진입하자마자 X, Y축 속도를 강제로 덮어씌워서 넉백 적용!
        // (지금은 자신이 바라보는 방향의 반대쪽으로 밀려나게 세팅합니다)
        float knockbackDir = -player.FacingDirection;
        player.SetVelocity(knockbackDir * knockbackForceX, knockbackForceY);
    }

    public override void Update()
    {
        base.Update();

        // 애니메이션(Hurt)이 완전히 끝나면 다시 대기 상태로 복귀
        if (isAnimationFinished)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 피격 중에는 플레이어의 입력(MoveInput)을 일절 받지 않으므로,
        // 강제로 부여된 넉백 속도(SetVelocity)에 의해 물리적으로만 날아갑니다.
    }

}
