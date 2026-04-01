using UnityEngine;

public abstract class State
{
    protected PlayerController player;
    protected StateMachine stateMachine;

    public State(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    // 상태에 진입할 때 1회 호출 (예: 애니메이션 재생, 파티클 생성)
    public virtual void Enter() { }

    // 매 프레임 호출 (예: 입력 감지, 타이머 체크)
    public virtual void Update() { }

    // 물리 프레임마다 호출 (예: Rigidbody2D 속도 조절)
    public virtual void FixedUpdate() { }

    // 상태를 빠져나갈 때 1회 호출 (예: 변수 초기화)
    public virtual void Exit() { }
}
