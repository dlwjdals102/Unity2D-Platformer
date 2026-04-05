using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 진입 시 특별히 초기화할 것이 없다면 base.Enter()만 둡니다. (애니메이션 재생은 부모가 해줌)
    }

    public override void Update()
    {
        base.Update();

        // [센서 확인 1] 시야에 플레이어가 들어왔는가?
        if (enemy.IsPlayerInSight())
        {
            Debug.Log("침입자 발견! 추적 모드 가동!");
            stateMachine.ChangeState(enemy.ChaseState); 
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 공중에 떠 있을 때는 순찰 로직(낭떠러지 체크 및 뒤돌기)을 완전히 멈춥니다!
        if (!enemy.IsGrounded()) return;

        // [센서 확인 2] 앞에 바닥이 없거나(낭떠러지) 벽이 있는가?
        if (!enemy.IsLedgeDetected() || enemy.IsWallDetected())
        {
            enemy.Flip(); // 스스로 180도 회전!
        }

        // [이동 로직] 현재 바라보는 방향(FacingDirection)으로 순찰 속도만큼 걷기
        enemy.SetVelocity(enemy.Data.patrolSpeed * enemy.FacingDirection, enemy.RB.linearVelocity.y);
    }
}