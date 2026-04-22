using UnityEngine;

public class BossState : State<Boss>
{
    // 자식 상태들에서 쉽게 접근할 수 있도록 캐싱
    protected Boss boss => entity;

    public BossState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }
}