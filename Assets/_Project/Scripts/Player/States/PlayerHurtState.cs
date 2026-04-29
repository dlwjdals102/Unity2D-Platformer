using UnityEngine;

public class PlayerHurtState : PlayerState
{
    public PlayerHurtState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        player.Movement.CanAutoFlip = false;

        // 정확한 타격 반대 방향으로 튕겨나감!
        player.Movement.SetVelocity(player.KnockbackDirection * player.knockbackForceX, player.knockbackForceY);

        AudioManager.Instance.Play("SFX_Hurt");
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

    public override void Exit()
    {
        base.Exit();

        player.Movement.CanAutoFlip = true;
    }
}
