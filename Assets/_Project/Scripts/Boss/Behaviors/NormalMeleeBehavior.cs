using UnityEngine;

[CreateAssetMenu(fileName = "NewNormalMeleeBehavior", menuName = "Data/Boss/Behaviors/Normal Melee")]
public class NormalMeleeBehavior : BossAttackBehavior
{
    public override BossState CreateState(Boss owner, StateMachine<Boss> stateMachine)
    {
        return new BossMeleeAttackState(owner, stateMachine, "");
    }

}
