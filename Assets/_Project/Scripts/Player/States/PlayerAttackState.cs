using UnityEngine;

public class PlayerAttackState : State<PlayerController>
{
    private bool comboInputRegistered; // 공격 도중 공격 키를 또 눌렀는지 기억하는 플래그

    public PlayerAttackState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();

        entity.UseAttackInput();
        // 지상 공격이므로 미끄러지지 않도록 X축 속도를 즉시 0으로 만듭니다.
        entity.SetVelocityX(0f);
        comboInputRegistered = false;

        // 현재 몇 단 공격인지 애니메이터에게 숫자로 알려줍니다!
        entity.Anim.SetInteger("ComboCounter", entity.comboCounter);
    }

    public override void Update()
    {
        base.Update();

        // 1. 공격 애니메이션이 재생되는 도중에 공격 버튼을 또 누르면? 
        // 바로 다음 상태로 넘어가지 않고 입력 예약(comboInputRegistered)만 해둡니다.
        if (entity.AttackInput)
        {
            comboInputRegistered = true;
            entity.UseAttackInput();
        }

        // 2. 애니메이션의 마지막 프레임에 도달했을 때 (Animation Event 발동 시점)
        if (isAnimationFinished)
        {
            // 예약된 입력이 있고, 아직 최대 콤보(예: 3타)에 도달하지 않았다면?
            if (comboInputRegistered && entity.comboCounter < 3)
            {
                entity.comboCounter++; // 콤보 단수를 1 올리고
                stateMachine.ChangeState(entity.AttackState); // 자기 자신(AttackState)으로 다시 진입!!
            }
            else
            {
                // 예약된 입력이 없거나 3타까지 다 쳤으면 대기 상태로 돌아감
                stateMachine.ChangeState(entity.IdleState);
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 공격 상태를 완전히 빠져나갈 때 현재 시간을 기록하여, 
        // 콤보 유예 시간(comboWindow) 계산에 사용합니다.
        entity.lastAttackTime = Time.time;
    }
}
