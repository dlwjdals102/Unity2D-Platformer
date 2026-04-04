using UnityEngine;

public class EnemyState : State<Enemy>
{
    // 하위 상태들(Patrol, Chase 등)에서 entity. 대신 enemy. 로 
    // 직관적으로 쓸 수 있도록 프로퍼티를 하나 만들어 둡니다.
    protected Enemy enemy => entity;

    public EnemyState(Enemy entity, StateMachine<Enemy> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    // 공통 로직이 필요하다면 여기에 추가 (예: 모든 적은 매 프레임 플레이어와의 거리를 잰다 등)
}
