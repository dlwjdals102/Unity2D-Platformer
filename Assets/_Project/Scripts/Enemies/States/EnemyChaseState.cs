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

        // 내가 원거리 몬스터인데, 플레이어가 너무 가깝다? -> 쫓아가지 말고 도망쳐!
        // (RangedEnemyData를 사용하는 적만 후퇴 행동을 가짐)
        if (enemy.Data is RangedEnemyData && enemy.IsPlayerInRetreatRange())
        {
            stateMachine.ChangeState(enemy.RetreatState);
            return;
        }

        // 공격 사거리 진입 시 -> 묻지도 따지지도 않고 공격 상태로!
        if (enemy.IsPlayerInAttackRange())
        {
            stateMachine.ChangeState(enemy.AttackState);
            return;
        }

        // 추격 중이라면 무조건 플레이어의 현재 위치를 향해 방향을 실시간으로 갱신합니다!
        chaseDir = Mathf.Sign(enemy.PlayerTransform.position.x - enemy.transform.position.x);

        // 2. 시야 판정 (어그로 유지 시스템)
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