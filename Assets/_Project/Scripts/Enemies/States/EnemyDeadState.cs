using UnityEngine;

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 1. 죽었으므로 모든 움직임을 즉시 멈춥니다.
        enemy.Movement.ZeroVelocity();

        // 2. 핵심: 플레이어가 시체를 통과할 수 있도록 콜라이더를 비활성화합니다.
        Collider2D coll = enemy.GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        // (선택) 중력의 영향을 받지 않게 하려면 RB.bodyType = RigidbodyType2D.Kinematic 으로 변경할 수도 있습니다.
        enemy.Movement.RB.bodyType = RigidbodyType2D.Kinematic;
    }

    public override void Update()
    {
        base.Update();

        // 사망 애니메이션이 완전히 끝나면 오브젝트를 파괴합니다.
        if (isAnimationFinished)
        {
            Object.Destroy(enemy.gameObject);
        }
    }
}