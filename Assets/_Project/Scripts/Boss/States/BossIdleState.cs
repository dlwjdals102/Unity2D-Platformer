using UnityEngine;

public class BossIdleState : BossState
{
    private float idleTimer;

    public BossIdleState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        boss.Movement.SetVelocity(0f, boss.Movement.RB.linearVelocity.y);

        // 데이터의 attackCooldown을 휴식 시간으로 사용합니다.
        idleTimer = boss.Data.attackCooldown;

        // 휴식에 들어갈 때, 다음에 무슨 공격을 할지 미리 뽑아둡니다!
        boss.ChooseNextAttack();
    }

    public override void Update()
    {
        base.Update();

        idleTimer -= Time.deltaTime;

        // 휴식이 끝나면 추격 시작!
        if (idleTimer <= 0)
        {
            stateMachine.ChangeState(boss.ChaseState);
        }
    }
}