using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public StateMachine stateMachine { get; private set; }

    // 구체적인 상태들
    /*public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }*/

    // 필요한 컴포넌트들
    public Rigidbody2D RB { get; private set; }
    public Animator Anim { get; private set; }

    private void Awake()
    {
        stateMachine = new StateMachine();

        /*IdleState = new PlayerIdleState(this, stateMachine);
        MoveState = new PlayerMoveState(this, stateMachine);*/

        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
    }

    private void Start()
    {
        // 시작 시 Idle 상태로 초기화
        /*stateMachine.Initialize(IdleState);*/
    }

    private void Update()
    {
        stateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.FixedUpdate();
    }
}
