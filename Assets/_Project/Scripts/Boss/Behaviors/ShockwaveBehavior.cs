using UnityEngine;

[CreateAssetMenu(fileName = "NewShockwaveBehavior", menuName = "Data/Boss/Behaviors/Shockwave")]
public class ShockwaveBehavior : BossAttackBehavior
{
    public override BossState CreateState(Boss owner, StateMachine<Boss> stateMachine)
    {
        return new BossShockwaveState(owner, stateMachine, "");
    }
}
