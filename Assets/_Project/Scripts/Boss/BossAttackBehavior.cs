using UnityEngine;

public abstract class BossAttackBehavior : ScriptableObject
{
    public abstract BossState CreateState(Boss owner, StateMachine<Boss> stateMachine);
}
