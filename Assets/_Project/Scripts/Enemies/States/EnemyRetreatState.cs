using UnityEngine;

public class EnemyRetreatState : EnemyState
{

    public EnemyRetreatState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Update()
    {
        base.Update();

        // 거리가 충분히 벌어졌다면? -> 추격 상태로! 
        // (추격 상태로 가면 코앞이 공격 사거리이므로 바로 뒤돌아서 화살을 쏘게 됩니다)
        if (!enemy.IsPlayerInRetreatRange())
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 1. 도망칠 방향 계산 (플레이어의 반대 방향)
        float retreatDir = -Mathf.Sign(enemy.PlayerTransform.position.x - enemy.transform.position.x);
        // 2. 센서 1프레임 함정 회피 (도망치는 방향과 바라보는 방향이 일치할 때만 센서 작동)
        bool isFacingCorrectly = (Mathf.Sign(retreatDir) == Mathf.Sign(enemy.Movement.FacingDirection));

        // 도망치다가 등 뒤가 벽이거나 낭떠러지라면? -> 궁지에 몰림!
        if (isFacingCorrectly && (!enemy.IsLedgeDetected() || enemy.Movement.IsWallDetected()))
        {
            // 멈춰 세웁니다.
            enemy.Movement.SetVelocity(0f, enemy.Movement.RB.linearVelocity.y);

            // 벽을 보고 쏘는 바보짓을 막기 위해, 강제로 플레이어 쪽으로 몸을 돌립니다.
            enemy.TurnTowards(enemy.PlayerTransform);

            // 발악 공격 시작!
            stateMachine.ChangeState(enemy.AttackState);
        }
        else
        {
            // 뒤가 안전하다면 도망 (RangedEnemyData에서 retreatSpeed 가져오기)
            float speed = (enemy.Data is RangedEnemyData rangedData) ? rangedData.retreatSpeed : enemy.Data.chaseSpeed;
            enemy.Movement.SetVelocity(retreatDir * speed, enemy.Movement.RB.linearVelocity.y);
        }
    }
}
