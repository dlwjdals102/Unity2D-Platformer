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
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        player.Movement.FlipController(player.MoveInput.x);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        player.Movement.SetVelocity(player.MoveInput.x * player.Data.moveSpeed, player.Movement.RB.linearVelocity.y);
    }
}


