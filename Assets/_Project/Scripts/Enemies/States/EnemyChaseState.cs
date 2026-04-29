using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private float lostSightTimer;
    private float chaseDir; // 쫓아갈 방향

    public EnemyChaseState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        lostSightTimer = 0f;

        // 추격 시작 시 플레이어가 있는 방향을 계산 (1 = 오른쪽, -1 = 왼쪽)
        chaseDir = Mathf.Sign(enemy.PlayerTransform.position.x - enemy.transform.position.x);
    }

    public override void Update()
    {
        base.Update();

        // 플레이어가 없으면 패트롤로 복귀
        if (enemy.PlayerTransform == null)
        {
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        // 원거리 몬스터 + 너무 가까움 → 도망
        if (enemy.Data is RangedEnemyData && enemy.IsPlayerInRetreatRange())
        {
            stateMachine.ChangeState(enemy.RetreatState);
            return;
        }

        // 공격 사거리 진입 + 쿨타임 체크
        if (enemy.IsPlayerInAttackRange())
        {
            // 쿨타임이 끝났을 때만 공격 상태로 전환
            if (Time.time >= enemy.lastAttackTime + enemy.Data.attackCooldown)
            {
                stateMachine.ChangeState(enemy.AttackState);
                return;
            }
            // 쿨타임 중이면 추격 멈추고 플레이어 쪽 바라보기 (다음 공격 대기)
            // chaseDir을 0으로 만들어서 FixedUpdate에서 멈춤
            chaseDir = 0;
            enemy.TurnTowards(enemy.PlayerTransform);
            return;
        }

        // 추격 중 방향 갱신
        chaseDir = Mathf.Sign(enemy.PlayerTransform.position.x - enemy.transform.position.x);

        // 2. 시야 판정 (어그로 유지)
        if (!enemy.IsPlayerInSight())
        {
            lostSightTimer += Time.deltaTime;

            // 시야에서 놓친 지 aggroDuration(2초)이 지났다면 순찰로 복귀
            if (lostSightTimer >= enemy.Data.aggroDuration)
            {
                stateMachine.ChangeState(enemy.PatrolState);
                return;
            }
        }
        else
        {
            // 플레이어가 시야에 있으면 타이머 리셋 및 방향 계속 갱신!
            lostSightTimer = 0f;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 순찰과 동일한 센서 1프레임 함정 회피 로직
        bool isFacingCorrectly = (Mathf.Sign(chaseDir) == Mathf.Sign(enemy.Movement.FacingDirection));

        // 3. 눈앞에 절벽이나 벽이 있으면? -> 제자리 뛰기 (추락/벽뚫기 방지)
        if (isFacingCorrectly && (!enemy.IsLedgeDetected() || enemy.Movement.IsWallDetected()))
        {
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
        }
        else
        {
            // 길이 안전하다면 추격 속도로 맹렬히 돌진! (방향에 맞춰 알아서 Flip 됨)
            enemy.Movement.SetVelocity(chaseDir * enemy.Data.chaseSpeed, enemy.Movement.RB.linearVelocity.y);
        }
    }
}