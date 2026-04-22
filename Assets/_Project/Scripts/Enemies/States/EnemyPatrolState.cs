using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    private float stateTimer;
    private bool isMoving;
    private float moveDir;

    public EnemyPatrolState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        isMoving = true;
        stateTimer = enemy.Data.patrolDuration;
        moveDir = enemy.Movement.FacingDirection; // 현재 바라보는 방향으로 출발!
    }

    public override void Update()
    {
        base.Update();

        // 1. 플레이어 발견 시 즉시 추격 상태로!
        if (enemy.IsPlayerInSight())
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // 2. 절벽 및 벽 감지 (1프레임 무한루프 방지 로직 적용)
        // -> 내가 가려는 방향(moveDir)과 내 몸이 바라보는 방향(FacingDirection)이 같을 때만 센서를 믿습니다!
        if (isMoving && (Mathf.Sign(moveDir) == Mathf.Sign(enemy.Movement.FacingDirection)))
        {
            if (!enemy.IsLedgeDetected() || enemy.Movement.IsWallDetected())
            {
                isMoving = false;
                stateTimer = enemy.Data.turnWaitDuration; // 벽 앞에서 잠깐 멈칫!
                moveDir *= -1f; // 다음엔 반대로 가도록 뇌에 메모해둠
                return;
            }
        }

        // 3. 타이머 소모 및 걷기/쉬기 상태 전환
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            isMoving = !isMoving;
            // 걷기 시작하면 patrolDuration, 쉬기 시작하면 idleDuration 타이머 세팅
            stateTimer = isMoving ? enemy.Data.patrolDuration : enemy.Data.idleDuration;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isMoving)
        {
            // SetVelocity 안에 있는 FlipController가 방향에 맞춰 스프라이트를 알아서 뒤집어줍니다!
            enemy.Movement.SetVelocity(moveDir * enemy.Data.patrolSpeed, enemy.Movement.RB.linearVelocity.y);
        }
        else
        {
            // 쉴 때는 확실하게 브레이크를 밟아서 미끄러짐 방지
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);
        }
    }
}