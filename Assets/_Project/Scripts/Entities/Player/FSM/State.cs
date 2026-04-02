using UnityEngine;

public abstract class State
{
    protected PlayerController player;
    protected StateMachine stateMachine;
    private int animBoolHash;

    protected bool isAnimationFinished;

    public State(PlayerController player, StateMachine stateMachine, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animBoolHash = Animator.StringToHash(animBoolName);
    }

    // 상태에 진입할 때 1회 호출 (예: 애니메이션 재생, 파티클 생성)
    public virtual void Enter()
    {
        // 상태 진입 시 해당 애니메이션 Bool 파라미터를 true로 만듭니다.
        player.Anim.SetBool(animBoolHash, true);
        isAnimationFinished = false; // 진입 시 초기화
    }

    // 매 프레임 호출 (예: 입력 감지, 타이머 체크)
    public virtual void Update() { }

    // 물리 프레임마다 호출 (예: Rigidbody2D 속도 조절)
    public virtual void FixedUpdate() { }

    // 상태를 빠져나갈 때 1회 호출 (예: 변수 초기화)
    public virtual void Exit()
    {
        // 상태 종료 시 해당 애니메이션 Bool 파라미터를 false로 만듭니다.
        player.Anim.SetBool(animBoolHash, false);
    }

    // 애니메이션 이벤트에서 호출할 가상 함수
    public virtual void AnimationFinishTrigger()
    {
        isAnimationFinished = true;
    }
}
