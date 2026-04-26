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

    // [추가] 애니메이션 타격 프레임 때 호출될 가상 함수
    public virtual void TriggerAttack()
    {
        // 기본적으로는 아무것도 하지 않음. (공격 상태에서만 덮어써서 사용)
    }

    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;
}
