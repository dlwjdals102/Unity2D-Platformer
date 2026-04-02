using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(PlayerController player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Mathf.Abs(player.MoveInput.x) < 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        player.CheckDirectionToFace(player.MoveInput.x);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        player.SetVelocityX(player.MoveInput.x * player.moveSpeed);
    }
}


