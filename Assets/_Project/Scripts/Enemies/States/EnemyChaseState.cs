using UnityEngine;

public class EnemyChaseState : EnemyState
{
    // 시야에서 놓친 시간을 기록할 타이머
    private float lostSightTimer;

    public EnemyChaseState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        lostSightTimer = 0f;
        // (선택) 여기서 느낌표 파티클을 띄우거나, 괴성을 지르는 사운드를 재생하면 효과가 아주 좋습니다.
    }

    public override void Update()
    {
        base.Update();

        // 1. 공격 사거리 안에 들어왔는가?
        if (enemy.IsPlayerInAttackRange())
        {
            // 쿨타임이 지났으면 공격!
            if (Time.time >= enemy.lastAttackTime + enemy.Data.attackCooldown)
            {
                stateMachine.ChangeState(enemy.AttackState);
            }
            else // 쿨타임이 안 지났으면 후퇴(거리 조절) 상태로 넘김!
            {
                stateMachine.ChangeState(enemy.RetreatState);
            }

            return;
        }

        // 2. 시야에서 놓쳤는가?
        if (!enemy.IsPlayerInSight())
        {
            lostSightTimer += Time.deltaTime;

            // 타이머가 어그로 유지 시간(aggroDuration)을 초과하면 비로소 순찰로 복귀
            if (lostSightTimer >= enemy.Data.aggroDuration)
            {
                Debug.Log("침입자를 놓쳤다... 순찰로 복귀.");
                stateMachine.ChangeState(enemy.PatrolState);
                return;
            }
        }
        else
        {
            lostSightTimer = 0f;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 이전의 뒷걸음질 꼼수(hack) 코드를 전부 지우고, 아주 깔끔한 본연의 추적 로직만 남깁니다!
        if (!enemy.IsLedgeDetected() || enemy.Movement.IsWallDetected())
        {
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
            return;
        }

        enemy.Movement.SetVelocity(enemy.Data.chaseSpeed * enemy.Movement.FacingDirection, enemy.Movement.RB.linearVelocity.y);

    }
}