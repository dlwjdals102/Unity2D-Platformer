using UnityEngine;

public abstract class State<T> where T : Entity
{
    protected T entity; // 플레이어나 적의 본체
    protected StateMachine<T> stateMachine;
    protected bool isAnimationFinished;
    private int animBoolHash;

    public State(T entity, StateMachine<T> stateMachine, string animBoolName)
    {
        this.entity = entity;
        this.stateMachine = stateMachine;
        this.animBoolHash = Animator.StringToHash(animBoolName);
    }

    // 상태에 진입할 때 1회 호출 (예: 애니메이션 재생, 파티클 생성)
    public virtual void Enter()
    {
        // T 타입이 무엇이든 Animator를 가지고 있다고 가정하거나, 
        // 하단에 설명할 공통 인터페이스/베이스를 활용해 접근합니다.
        // 여기서는 직관성을 위해 직접 접근 방식을 예로 듭니다.
        entity.Anim.SetBool(animBoolHash, true);
        isAnimationFinished = false;

    }

    // 매 프레임 호출 (예: 입력 감지, 타이머 체크)
    public virtual void Update() { }

    // 물리 프레임마다 호출 (예: Rigidbody2D 속도 조절)
    public virtual void FixedUpdate() { }

    // 상태를 빠져나갈 때 1회 호출 (예: 변수 초기화)
    public virtual void Exit()
    {
        // 상태 종료 시 해당 애니메이션 Bool 파라미터를 false로 만듭니다.
        entity.Anim.SetBool(animBoolHash, false);
    }

    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;
}
