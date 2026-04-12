using UnityEngine;

// State<PlayerController>를 상속받는 플레이어 전용 베이스 클래스!
public class PlayerState : State<PlayerController>
{
    // 꿀팁: 자식 클래스들에서 entity 대신 player라는 이름으로 직관적으로 쓰기 위함
    protected PlayerController player => entity;

    public PlayerState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    // 플레이어만의 공통 기능이 필요하다면 여기에 추가!
}