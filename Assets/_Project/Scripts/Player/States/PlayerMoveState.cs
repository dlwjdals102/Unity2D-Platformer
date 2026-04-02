using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Mathf.Abs(entity.MoveInput.x) < 0.01f)
        {
            stateMachine.ChangeState(entity.IdleState);
            return;
        }

        entity.CheckDirectionToFace(entity.MoveInput.x);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        entity.SetVelocityX(entity.MoveInput.x * entity.moveSpeed);
    }
}


