using UnityEngine;

public class BossSleepState : BossState
{
    public BossSleepState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        // 잠들어 있으므로 물리력을 0으로 만들어 고정시킵니다.
        boss.Movement.RB.linearVelocity = Vector2.zero;
    }

    public override void Update()
    {
        base.Update();
        // 플레이어를 찾지도, 공격하지도 않습니다. 오직 룸 매니저가 깨워주길 기다립니다.
    }
}