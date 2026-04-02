using UnityEngine;

public class PlayerGroundedState : State<PlayerController>
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

        // 대시 버튼을 눌렀고, 쿨타임이 돌지 않았다면 즉시 대시 상태로 전환!
        if (entity.DashInput && entity.CanDash)
        {
            stateMachine.ChangeState(entity.DashState);
            return; // 대시로 상태가 바뀌었으니 아래 로직(점프 등)은 무시
        }

        if (entity.AttackInput)
        {
            // 시간이 지났거나(콤보 끊김) || 카운터가 3 이상이면(콤보 끝) 무조건 1타로 초기화!
            if (Time.time >= entity.lastAttackTime + entity.comboWindow || entity.comboCounter >= 3)
            {
                entity.comboCounter = 1;
            }
            // 유예 시간 내에 눌렀다면 -> 콤보를 다음 단계로 이어줌!
            else
            {
                entity.comboCounter++;
            }

            stateMachine.ChangeState(entity.AttackState);
            return;
        }

        // 공통 로직 1: 땅에 있을 때 점프 버튼을 누르면 즉시 점프 상태로 전환
        if (entity.HasJumpInputBuffer && entity.IsGrounded)
        {
            stateMachine.ChangeState(entity.JumpState);
        }

        // 공통 로직 2: 갑자기 발밑에 땅이 없어지면 낙하 상태로 전환
        if (!entity.IsGrounded)
        {
            stateMachine.ChangeState(entity.FallState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 땅에 있을 때는 항상 방향 전환이 가능하도록 공통 처리
        entity.CheckDirectionToFace(entity.MoveInput.x);
    }
}
