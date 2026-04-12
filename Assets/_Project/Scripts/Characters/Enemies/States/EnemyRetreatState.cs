using UnityEngine;

public class EnemyRetreatState : EnemyState
{
    private float lostSightTimer; // 어그로 타이머

    public EnemyRetreatState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        lostSightTimer = 0f; // 타이머 초기화
    }

    public override void Update()
    {
        base.Update();

        // 뒤로 물러날 때도 플레이어에게서 눈을 떼지 않습니다!
        enemy.TurnTowards(enemy.PlayerTransform);

        // 1. 공격 쿨타임이 다 찼는가?
        if (Time.time >= enemy.lastAttackTime + enemy.Data.attackCooldown)
        {
            // 사거리 안이면 바로 때리고, 아니면 다시 쫓아갑니다.
            if (enemy.IsPlayerInAttackRange())
                stateMachine.ChangeState(enemy.AttackState);
            else
                stateMachine.ChangeState(enemy.ChaseState);

            return;
        }

        // 2. 시야에서 놓쳤는가? (바로 포기하지 않고 타이머를 돌림!)
        if (!enemy.IsPlayerInSight())
        {
            lostSightTimer += Time.deltaTime;

            if (lostSightTimer >= enemy.Data.aggroDuration)
            {
                Debug.Log("침입자를 놓쳤다... 순찰로 복귀.");
                stateMachine.ChangeState(enemy.PatrolState);
                return;
            }
        }
        else
        {
            // 플레이어가 시야에 있으면 타이머 리셋
            lostSightTimer = 0f;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // [디테일 추가] 
        // 낭떠러지를 등지고 있거나 벽에 닿았다면 뒷걸음질 중단!
        if (!enemy.IsLedgeBehindDetected() || enemy.IsWallBehindDetected())
        {
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
            return;
        }

        // 물리 이동 분기 처리
        if (enemy.IsPlayerInAttackRange())
        {
            // 플레이어가 너무 가깝게 붙어있으면 뒤로 물러남 (거리 벌리기)
            float retreatSpeed = enemy.Data.chaseSpeed * 0.4f;
            enemy.Movement.SetVelocity(-enemy.Movement.FacingDirection * retreatSpeed, enemy.Movement.RB.linearVelocity.y);
        }
        else
        {
            // 사거리 밖으로 밀려났다면 뒷걸음질을 멈추고 제자리에서 대기 (속도 0)
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
        }
    }
}
