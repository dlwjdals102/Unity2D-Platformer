using UnityEngine;

public class BossChaseState : BossState
{
    public BossChaseState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Update()
    {
        base.Update();

        // 캐시된 PlayerTransform 사용 (매 프레임 GameObject.Find 호출 X)
        Transform player = boss.PlayerTransform;
        if (player == null) return;

        boss.TurnTowards(player);

        // 플레이어와의 거리를 잰다
        float distanceToPlayer = Vector2.Distance(boss.transform.position, player.position);

        // 내가 고른 공격(NextAttack)의 사거리에 들어왔는가?!
        if (distanceToPlayer <= boss.NextAttack.attackDistance)
        {
            // 들어왔다면 즉시 공격!
            switch (boss.NextAttack.behaviorType)
            {
                case BehaviorType.NormalMelee:
                    stateMachine.ChangeState(boss.MeleeAttackState);
                    break;
                case BehaviorType.ShockwaveSlam:
                    stateMachine.ChangeState(boss.ShockwaveState);
                    break;
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // 플레이어를 향해 이동
        boss.Movement.SetVelocity(boss.Data.chaseSpeed * boss.Movement.FacingDirection, boss.Movement.RB.linearVelocity.y);
    }
}