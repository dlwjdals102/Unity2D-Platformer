using UnityEngine;

public class PlayerFallState : PlayerAirborneState
{
    public PlayerFallState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }
    

    public override void Enter()
    {
        base.Enter();
    }
}
