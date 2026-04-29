using UnityEngine;

public class BossIntroState : BossState
{
    public BossIntroState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        boss.Movement.RB.linearVelocity = Vector2.zero;

        Transform player = boss.PlayerTransform;
        if (player == null) return;

        boss.TurnTowards(player);

        // 여기서 "크아앙" 하는 등장 이펙트나 화면 흔들림(Camera Shake)을 호출하면 좋습니다!
    }

    public override void Update()
    {
        base.Update();
       
        if (isAnimationFinished)
        {
            // 포효가 끝나면, 잠깐 쉬면서 다음 패턴을 생각합니다!
            stateMachine.ChangeState(boss.IdleState);
        }
    }
}